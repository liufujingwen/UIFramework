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
        Normal = 1,
        Dialog = 2,
        Guide = 4,
        Tips = 8,
        TopMask = 16,
        Top = 32,

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
        public UIContex UIContext = null;
        public GameObject GameObject = null;
        public Transform Transform = null;
        public Transform ChildParentNode = null;
        public UIStateType UIState = UIStateType.None;
        public AnimationStateType AnimationState = AnimationStateType.None;
        public bool AwakeState = false;

        protected Dictionary<Canvas, int> CanvasDic = null;
        protected Animator Animator = null;
        protected UIProxy UIProxy = null;

        //当前是否播放动画
        protected TaskCompletionSource<bool> IsPlayingAniamtionTask = null;

        public void SetContext(GameObject gameObject, UIContex uiContext)
        {
            this.UIContext = uiContext;
            this.GameObject = gameObject;
            this.GameObject.name = uiContext.UIData.UIName;
            this.Transform = this.GameObject.transform;
            this.ChildParentNode = this.Transform.FindTransform("ChildParentNode");

            UIManager.Instance.SetUIParent(this.Transform, this.UIContext.UIData.UIType);

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
            if (CanvasDic != null)
            {
                foreach (var kv in CanvasDic)
                    kv.Key.sortingOrder = kv.Value + order;
            }
        }

        #region Awake

        public virtual void Awake()
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

        public void OnAwake()
        {
            this.UIProxy?.OnAwake();
        }

        #endregion


        #region Start

        public virtual async Task StartAsync(params object[] args)
        {
            Start(args);
            Enable();

            //播放进场动画
            if (this.UIContext.UIData.HasAnimation)
            {
                this.AnimationState = AnimationStateType.Start;
                this.Animator.enabled = true;
                this.Animator.Play("Enable");
                this.Animator.Update(0);
                await GetPlayingAniamtionTask();
            }
        }

        public virtual void Start(params object[] args)
        {
            this.UIState = UIStateType.Start;
            this.GameObject.SetActive(true);
            this.OnStart(args);
        }

        public void OnStart(params object[] args)
        {
            this.UIProxy?.OnStart(args);
        }


        #endregion


        #region Enable

        public async Task EnableAsync()
        {
            Enable();
            //播放Resume动画
            if (this.UIContext.UIData.HasAnimation)
            {
                this.AnimationState = AnimationStateType.Disable;
                this.Animator.enabled = true;
                this.Animator.Play("Enable");
                this.Animator.Update(0);
                await this.GetPlayingAniamtionTask();
            }
        }

        public virtual void Enable()
        {
            this.UIState = UIStateType.Enable;
            this.GameObject.SetActive(true);
            this.OnEnable();
            GameEventManager.Instance.RegistEvent(UIProxy);
        }

        public void OnEnable()
        {
            this.UIProxy?.OnEnable();
        }

        #endregion


        #region Disable

        public async Task DisableAsync()
        {
            //播放暂停动画
            if (this.UIContext.UIData.HasAnimation)
            {
                this.AnimationState = AnimationStateType.Enable;
                this.Animator.enabled = true;
                this.Animator.Play("Disable");
                this.Animator.Update(0);
                await this.GetPlayingAniamtionTask();
            }
            Disable();
        }

        public virtual void Disable()
        {
            this.UIState = UIStateType.Disable;
            GameEventManager.Instance.RemoveEvent(UIProxy);
            OnDisable();
            this.GameObject.SetActive(false);
        }

        public void OnDisable()
        {
            this.UIProxy?.OnDisable();
        }

        #endregion


        #region Destroy

        public async Task DestroyAsync()
        {
            this.AnimationState = AnimationStateType.Destroy;

            //播放退出动画
            if (this.UIContext.UIData.HasAnimation)
            {
                this.Animator.enabled = true;
                this.Animator.Play("Disable");
                this.Animator.Update(0);
                await this.GetPlayingAniamtionTask();
            }

            Destroy();
        }

        public virtual void Destroy()
        {
            Disable();
            this.OnDestroy();
            GameObject.Destroy(this.GameObject);
            this.GameObject = null;
            this.Transform = null;
            this.UIState = UIStateType.Destroy;
            this.AnimationState = AnimationStateType.Destroy;
            AwakeState = false;
            this.IsPlayingAniamtionTask = null;
        }

        public void OnDestroy()
        {
            this.UIProxy?.OnDestroy();
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
            IsPlayingAniamtionTask = new TaskCompletionSource<bool>();
            return IsPlayingAniamtionTask.Task;
        }

        public void OnNotifyAnimationState()
        {
            this.IsPlayingAniamtionTask.SetResult(true);
        }
    }
}
