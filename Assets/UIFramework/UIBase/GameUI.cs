using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UIFramework
{
    public class GameUI
    {
        public void SetContext(GameObject gameObject,UIContex uiContext)
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
            Empty,
            Enter,
            Pause,
            Resume,
            Exit,
        }

        public enum UIStateType
        {
            None,
            Init,
            Show,
            Hide,
        }

        public UIContex UIContext = null;
        public GameObject GameObject = null;
        public Transform Transform = null;
        public UIStateType UIState = UIStateType.None;
        public AnimationStateType AnimationState = AnimationStateType.Empty;
        public bool InitState = false;

        private Dictionary<Canvas, int> canvasDic = null;
        private Animator animator = null;
        private UIProxy uiProxy = null;
        private TaskCompletionSource<bool> enterTask = null;
        private TaskCompletionSource<bool> pauseTask = null;
        private TaskCompletionSource<bool> resumeTask = null;
        private TaskCompletionSource<bool> exitTask = null;

        public void Init()
        {
            this.UIState = GameUI.UIStateType.Init;
            InitState = true;

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
            this.OnInit();
            this.GameObject.SetActive(false);
        }

        public void OnInit()
        {
            this.uiProxy?.OnInit();
        }

        public async Task EnterAsync(params object[] args)
        {
            this.AnimationState = AnimationStateType.Enter;
            this.UIState = GameUI.UIStateType.Show;
            this.GameObject.SetActive(true);
            this.OnEnter(args);
            GameEventManager.Instance.RegistEvent(uiProxy);

            //播放进场动画
            if (this.UIContext.UIData.HasAnimation)
            {
                this.animator.enabled = true;
                this.animator.Play("Enter");
                this.animator.Update(0);
                await Enter();
                enterTask = null;
            }
        }

        public Task Enter()
        {
            this.enterTask = new TaskCompletionSource<bool>();
            return this.enterTask.Task;
        }

        public void OnEnter(params object[] args)
        {
            this.uiProxy?.OnEnter(args);
        }

        public async Task PauseAsync()
        {
            this.AnimationState = AnimationStateType.Pause;
            this.UIState = GameUI.UIStateType.Hide;

            //播放暂停动画
            if (this.UIContext.UIData.HasAnimation)
            {
                this.animator.enabled = true;
                this.animator.Play("Pause");
                this.animator.Update(0);
                await Pause();
                this.pauseTask = null;
            }

            GameEventManager.Instance.RemoveEvent(uiProxy);
            this.GameObject.SetActive(false);
        }

        public Task Pause()
        {
            this.pauseTask = new TaskCompletionSource<bool>();
            return pauseTask.Task;
        }

        public void OnPause()
        {
            this.uiProxy?.OnPause();
        }

        public async Task ResumeAsync()
        {
            this.AnimationState = AnimationStateType.Resume;
            this.UIState = GameUI.UIStateType.Show;
            this.GameObject.SetActive(true);
            this.OnResume();
            GameEventManager.Instance.RegistEvent(uiProxy);

            //播放Resume动画
            if (this.UIContext.UIData.HasAnimation)
            {
                this.animator.enabled = true;
                this.animator.Play("Resume");
                this.animator.Update(0);
                await this.Resume();
                resumeTask = null;
            }
        }

        public Task Resume()
        {
            this.resumeTask = new TaskCompletionSource<bool>();
            return this.resumeTask.Task;
        }

        public void OnResume()
        {
            this.uiProxy?.OnResume();
        }

        public async Task ExitAsync()
        {
            this.AnimationState = AnimationStateType.Exit;

            //播放退出动画
            if (this.UIContext.UIData.HasAnimation)
            {
                this.animator.enabled = true;
                this.animator.Play("Exit");
                this.animator.Update(0);
                await this.Exit();
                exitTask = null;
            }

            this.OnExit();
            GameEventManager.Instance.RemoveEvent(uiProxy);
            this.UIState = GameUI.UIStateType.Hide;
            this.GameObject.SetActive(false);

            if (this.UIContext.UIData.UICloseType == UICloseType.Destroy)
                Destroy();
        }

        public Task Exit()
        {
            this.exitTask = new TaskCompletionSource<bool>();
            return this.exitTask.Task;
        }

        public void OnExit()
        {
            this.uiProxy?.OnExit();
        }

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
        public void ExitImmediate()
        {
            OnExit();
            GameEventManager.Instance.RemoveEvent(uiProxy);
            this.UIState = GameUI.UIStateType.Hide;
            this.GameObject.SetActive(false);

            if (this.UIContext.UIData.UICloseType == UICloseType.Destroy)
                Destroy();
        }

        /// <summary>
        /// 销毁UI
        /// </summary>
        public void Destroy()
        {
            GameObject.Destroy(this.GameObject);
            this.GameObject = null;
            this.Transform = null;
            this.AnimationState = GameUI.AnimationStateType.Empty;

            this.enterTask = null;
            this.pauseTask = null;
            this.resumeTask = null;
            this.exitTask = null;
            InitState = false;
        }

        public void OnNotifyAnimationState()
        {
            switch (AnimationState)
            {
                case AnimationStateType.Enter:
                    this.enterTask.SetResult(true);
                    break;
                case AnimationStateType.Pause:
                    this.pauseTask.SetResult(true);
                    break;
                case AnimationStateType.Resume:
                    this.resumeTask.SetResult(true);
                    break;
                case AnimationStateType.Exit:
                    this.exitTask.SetResult(true);
                    break;
            }
        }
    }
}
