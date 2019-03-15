using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UIFramework
{
    public class GameUI
    {
        public void SetContext(GameObject gameObject, UIContex uiContext)
        {
            this.UIContext = uiContext;
            this.GameObject = gameObject;
            this.GameObject.name = uiContext.UIData.UIName;
            this.Transform = this.GameObject.transform;

            UIManager.Instance.SetUIParent(this.Transform, this.UIContext.UIData.UIType);

            if (this.UIContext.UIData.IsLuaUI)
            {
                //创建lua代理
                this.uiProxy = new UILuaProxy(this.UIContext);
            }
            else
            {
                //创建Mono代理
                System.Type type = UIManager.Instance.GetType(UIContext.UIData.UIName);
                this.uiProxy = System.Activator.CreateInstance(type) as UIProxy;
                this.uiProxy.SetContext(this.UIContext);
            }
        }

        public enum AnimationStateType
        {
            None,
            Start,
            Enable,
            Disable,
            Destroy,
        }

        public enum UIStateType
        {
            None,
            Awake,
            Start,
            Enable,
            Disable,
            Destroy,
        }

        public UIContex UIContext = null;
        public GameObject GameObject = null;
        public Transform Transform = null;
        public UIStateType UIState = UIStateType.None;
        public AnimationStateType AnimationState = AnimationStateType.None;
        public bool AwakeState = false;

        private Dictionary<Canvas, int> canvasDic = null;
        private Animator animator = null;
        private UIProxy uiProxy = null;
        private TaskCompletionSource<bool> startTask = null;
        private TaskCompletionSource<bool> disableTask = null;
        private TaskCompletionSource<bool> enableTask = null;
        private TaskCompletionSource<bool> destroyTask = null;

        public void Awake()
        {
            this.UIState = GameUI.UIStateType.Awake;
            AwakeState = true;

            //记录所有Canvas初始化的sortingOrder
            Canvas[] tempCanvases = this.GameObject.GetComponentsInChildren<Canvas>(true);
            canvasDic = new Dictionary<Canvas, int>(tempCanvases.Length);
            for (int i = 0; i < tempCanvases.Length; i++)
            {
                Canvas tempCanvas = tempCanvases[i];
                canvasDic[tempCanvas] = tempCanvas.sortingOrder;
            }

            if (this.UIContext.UIData.HasAnimation)
                this.animator = this.GameObject.GetComponent<Animator>();
            this.OnAwake();
            this.GameObject.SetActive(false);
        }

        public void OnAwake()
        {
            this.uiProxy?.OnAwake();
        }

        #region Start

        public async Task StartAsync(params object[] args)
        {
            Start(args);
            Enable();

            //播放进场动画
            if (this.UIContext.UIData.HasAnimation)
            {
                this.AnimationState = AnimationStateType.Start;
                this.animator.enabled = true;
                this.animator.Play("Enable");
                this.animator.Update(0);
                await GetStartTask();
                startTask = null;
            }
        }

        public void Start(params object[] args)
        {
            this.UIState = GameUI.UIStateType.Start;
            this.GameObject.SetActive(true);
            this.OnStart(args);
        }

        public Task GetStartTask()
        {
            this.startTask = new TaskCompletionSource<bool>();
            return this.startTask.Task;
        }

        public void OnStart(params object[] args)
        {
            this.uiProxy?.OnStart(args);
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
                this.animator.enabled = true;
                this.animator.Play("Enable");
                this.animator.Update(0);
                await this.GetEnableTask();
                enableTask = null;
            }
        }

        public void Enable()
        {
            this.UIState = GameUI.UIStateType.Enable;
            this.GameObject.SetActive(true);
            this.OnEnable();
            GameEventManager.Instance.RegistEvent(uiProxy);
        }

        public Task GetEnableTask()
        {
            this.enableTask = new TaskCompletionSource<bool>();
            return this.enableTask.Task;
        }

        public void OnEnable()
        {
            this.uiProxy?.OnEnable();
        }

        #endregion

        #region Disable

        public async Task DisableAsync()
        {
            //播放暂停动画
            if (this.UIContext.UIData.HasAnimation)
            {
                this.AnimationState = AnimationStateType.Enable;
                this.animator.enabled = true;
                this.animator.Play("Disable");
                this.animator.Update(0);
                await GetDisableTask();
                this.disableTask = null;
            }
            Disable();
        }

        public void Disable()
        {
            this.UIState = GameUI.UIStateType.Disable;
            GameEventManager.Instance.RemoveEvent(uiProxy);
            OnDisable();
            this.GameObject.SetActive(false);
        }

        public Task GetDisableTask()
        {
            this.disableTask = new TaskCompletionSource<bool>();
            return disableTask.Task;
        }

        public void OnDisable()
        {
            this.uiProxy?.OnDisable();
        }

        #endregion

        #region Desrtory

        public async Task DestroyAsync()
        {
            this.AnimationState = AnimationStateType.Destroy;

            //播放退出动画
            if (this.UIContext.UIData.HasAnimation)
            {
                this.animator.enabled = true;
                this.animator.Play("Disable");
                this.animator.Update(0);
                await this.GetDestroyTask();
                destroyTask = null;
            }

            Destroy();
        }

        public void Destroy()
        {
            Disable();
            this.OnDestroy();
            GameObject.Destroy(this.GameObject);
            this.GameObject = null;
            this.Transform = null;
            this.UIState = GameUI.UIStateType.Destroy;
            this.AnimationState = GameUI.AnimationStateType.Destroy;
            AwakeState = false;
            this.startTask = null;
            this.disableTask = null;
            this.enableTask = null;
            this.destroyTask = null;
        }

        public Task GetDestroyTask()
        {
            this.destroyTask = new TaskCompletionSource<bool>();
            return this.destroyTask.Task;
        }

        public void OnDestroy()
        {
            this.uiProxy?.OnDestroy();
        }

        #endregion

        /// <summary>
        /// 设置界面层级
        /// </summary>
        /// <param name="order"></param>
        public void SetCavansOrder(int order)
        {
            if (canvasDic != null)
            {
                foreach (var kv in canvasDic)
                    kv.Key.sortingOrder = kv.Value + order;
            }
        }

        /// <summary>
        /// UI直接关闭，不会播放退场动画
        /// </summary>
        //public void ExitImmediate()
        //{
        //    OnDestroy();
        //    GameEventManager.Instance.RemoveEvent(uiProxy);
        //    this.UIState = GameUI.UIStateType.Disable;
        //    this.GameObject.SetActive(false);
        //}


        public void OnNotifyAnimationState()
        {
            switch (AnimationState)
            {
                case AnimationStateType.Start:
                    this.startTask.SetResult(true);
                    break;
                case AnimationStateType.Enable:
                    this.disableTask.SetResult(true);
                    break;
                case AnimationStateType.Disable:
                    this.enableTask.SetResult(true);
                    break;
                case AnimationStateType.Destroy:
                    this.destroyTask.SetResult(true);
                    break;
            }
        }
    }
}
