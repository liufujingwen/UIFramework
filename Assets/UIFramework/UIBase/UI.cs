using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace UIFramework
{
    /// <summary>
    /// UI类型
    /// </summary>
    public enum UIType
    {
        Hud,
        Normal = 1,
        Popup = 2,
        Dialog = 4,
        Guide = 8,
        Tips = 16,
        TopMask = 32,
        Top = 64,

        Child,//子UI
    }

    /// <summary>
    /// UI加载方式
    /// </summary>
    public enum UIResType
    {
        Resorces,//使用Resource.Load()
        Bundle,//使用AssetBundle
        SetGameObject,//外部设置GameObject的方式（子UI）
    }

    /// <summary>
    /// UI关闭方式
    /// </summary>
    public enum UICloseType
    {
        Hide,//隐藏不销毁，放回poolCanvas
        Destroy,//直接销毁
    }

    /// <summary>
    /// UI动画状态
    /// </summary>
    public enum AnimationStateType
    {
        None,
        Start,
        Enable,
        Disable,
        Destroy,
    }

    /// <summary>
    /// UI状态
    /// </summary>
    public enum UIStateType
    {
        None,
        Awake,
        Start,
        Enable,
        Disable,
        Destroy,
    }

    public abstract class UI
    {
        public UIContext UIContext = null;
        public GameObject GameObject = null;
        public Transform Transform = null;
        public Transform ChildParentNode = null;
        public UIStateType UIState = UIStateType.None;
        public AnimationStateType AnimationState = AnimationStateType.None;
        public bool AwakeState = false;
        public int SortingOrder = 0;

        protected Dictionary<Canvas, int> CanvasDic = null;
        protected Animator Animator = null;
        protected UIProxy UIProxy = null;

        //当前是否播放动画
        protected TaskCompletionSource<bool> IsPlayingAniamtionTask = null;

        public void SetContext(GameObject gameObject, UIContext uiContext)
        {
            this.UIContext = uiContext;
            this.GameObject = gameObject;
            this.GameObject.name = uiContext.UIData.UIName;
            this.Transform = this.GameObject.transform;
            this.ChildParentNode = this.Transform.FindTransform("ChildParentNode");

            UIManager.Instance.SetUIParent(this.Transform, this.UIContext.UIData.UIType, false);

            if (uiContext.UIData.IsChildUI)
            {
                GameUI parentGameUI = UIManager.Instance.FindUI(this.UIContext.UIData.ParentUIName) as GameUI;
                if (this.Transform.parent != parentGameUI.ChildParentNode)
                {
                    this.Transform.SetParent(parentGameUI.ChildParentNode, false);
                    this.Transform.localScale = Vector3.one;
                    this.Transform.localPosition = Vector3.zero;
                    this.Transform.localRotation = Quaternion.identity;
                }
            }

            if (this.UIContext.UIData.IsLuaUI)
            {
                //创建lua代理
                this.UIProxy = new UILuaProxy(this.UIContext);
            }
            else
            {
                //创建Mono代理
                System.Type type = UIManager.Instance.GetType(UIContext.UIData.UIName);
                this.UIProxy = System.Activator.CreateInstance(type) as UIProxy;
                this.UIProxy.SetContext(this.UIContext);
            }
        }

        /// <summary>
        /// 设置界面层级
        /// </summary>
        /// <param name="order"></param>
        public void SetCavansOrder(int order)
        {
            this.SortingOrder = order;

            if (CanvasDic != null)
            {
                foreach (var kv in CanvasDic)
                    kv.Key.sortingOrder = kv.Value + order;
            }
        }

        #region Awake

        public virtual void Awake()
        {
            if (this.UIState == UIStateType.None)
            {
                this.UIState = UIStateType.Awake;
                AwakeState = true;

                //记录所有Canvas初始化的sortingOrder
                Canvas[] tempCanvases = this.GameObject.GetComponentsInChildren<Canvas>(true);
                CanvasDic = new Dictionary<Canvas, int>(tempCanvases.Length);
                for (int i = 0; i < tempCanvases.Length; i++)
                {
                    Canvas tempCanvas = tempCanvases[i];
                    CanvasDic[tempCanvas] = tempCanvas.sortingOrder;
                }

                if (this.UIContext.UIData.HasAnimation)
                    this.Animator = this.GameObject.GetComponent<Animator>();
                this.OnAwake();
                this.GameObject.SetActive(false);
            }
        }

        public void OnAwake()
        {
            this.UIProxy?.OnAwake();
        }

        #endregion


        #region Start

        public virtual async Task StartAsync(params object[] args)
        {
            if (this.UIState == UIStateType.Awake)
            {
                Start(args);
                BeforeEnable();
                Enable();
                this.AnimationState = AnimationStateType.Start;

                //播放进场动画
                if (this.UIContext.UIData.HasAnimation)
                {
                    IsPlayingAniamtionTask = new TaskCompletionSource<bool>();
                    this.Animator.enabled = true;
                    this.Animator.Play("Enable");
                    this.Animator.Update(0);
                }

                await this.WaitAnimationFinished();
            }
        }

        public virtual void Start(params object[] args)
        {
            if (this.UIState == UIStateType.Awake)
            {
                this.UIState = UIStateType.Start;
                this.GameObject.SetActive(true);
                this.OnStart(args);
            }
        }

        public void OnStart(params object[] args)
        {
            this.UIProxy?.OnStart(args);
        }

        #endregion


        #region Enable

        public async Task EnableAsync()
        {
            if (this.UIState == UIStateType.Start || this.UIState == UIStateType.Disable)
            {
                BeforeEnable();
                Enable();
                this.AnimationState = AnimationStateType.Disable;

                //播放Resume动画
                if (this.UIContext.UIData.HasAnimation)
                {
                    IsPlayingAniamtionTask = new TaskCompletionSource<bool>();
                    this.Animator.enabled = true;
                    this.Animator.Play("Enable");
                    this.Animator.Update(0);
                }

                await this.WaitAnimationFinished();
            }
        }

        public virtual void BeforeEnable()
        {
        }

        public virtual void Enable()
        {
            if (this.UIState == UIStateType.Start || this.UIState == UIStateType.Disable)
            {
                this.UIState = UIStateType.Enable;
                this.GameObject.SetActive(true);
                this.OnEnable();
                GameEventManager.Instance.RegisterEvent(UIProxy);
            }
        }

        public void OnEnable()
        {
            this.UIProxy?.OnEnable();
        }

        #endregion


        #region Disable

        public async Task DisableAsync()
        {
            if (this.UIState == UIStateType.Start || this.UIState == UIStateType.Enable)
            {
                BeforeDisable();
                this.AnimationState = AnimationStateType.Disable;

                //播放暂停动画
                if (this.UIContext.UIData.HasAnimation)
                {
                    IsPlayingAniamtionTask = new TaskCompletionSource<bool>();
                    this.Animator.enabled = true;
                    this.Animator.Play("Disable");
                    this.Animator.Update(0);
                }
                await this.WaitAnimationFinished();

                Disable();
            }
        }

        /// <summary>
        /// 播放动画之前执行
        /// </summary>
        public virtual void BeforeDisable()
        {
        }

        public virtual void Disable()
        {
            //只有状态为Start、Enable才能执行disable
            if (this.UIState == UIStateType.Start || this.UIState == UIStateType.Enable)
            {
                this.UIState = UIStateType.Disable;
                GameEventManager.Instance.RemoveEvent(UIProxy);
                OnDisable();
                this.GameObject.SetActive(false);
            }
        }

        public void OnDisable()
        {
            this.UIProxy?.OnDisable();
        }

        #endregion


        #region Destroy

        public virtual void Destroy(bool delete)
        {
            Disable();
            if (this.UIState != UIStateType.Destroy)
            {
                this.OnDestroy(delete);

                //真删除,直接Destroy GameObject
                if (delete)
                {
                    GameObject.Destroy(this.GameObject);
                    this.GameObject = null;
                    this.Transform = null;
                    this.UIState = UIStateType.Destroy;
                    this.AnimationState = AnimationStateType.Destroy;
                    AwakeState = false;
                    this.IsPlayingAniamtionTask = null;
                }
                else
                {
                    this.UIState = UIStateType.Awake;
                    this.AnimationState = AnimationStateType.None;
                    this.IsPlayingAniamtionTask = null;
                }
            }
        }

        public void OnDestroy(bool delete)
        {
            this.UIProxy?.OnDestroy(delete);
        }

        #endregion

        /// <summary>
        /// 等待动画播放完成Task
        /// </summary>
        /// <returns></returns>
        public virtual async Task WaitAnimationFinished()
        {
            if (IsPlayingAniamtionTask != null)
                await GetPlayingAniamtionTask();
        }

        /// <summary>
        /// 当前是否播放动画
        /// </summary>
        /// <returns></returns>
        public Task GetPlayingAniamtionTask()
        {
            return IsPlayingAniamtionTask.Task;
        }

        public void OnNotifyAnimationState()
        {
            if (this.IsPlayingAniamtionTask != null)
                this.IsPlayingAniamtionTask.SetResult(true);
        }
    }
}
