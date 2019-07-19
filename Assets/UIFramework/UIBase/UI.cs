using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;

namespace UIFramework
{
    /// <summary>
    /// UI类型
    /// </summary>
    public enum UIType
    {
        Hud,
        Normal = 1,
        NormalPopup = 2,
        Popup = 4,
        Dialog = 8,
        Guide = 16,
        Tips = 32,
        TopMask = 64,
        Top = 128,

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
        Loading,//资源正在加载
        Awake,
        Start,
        Enable,
        Disable,
        Destroy,
        Release,//资源已释放
    }

    public abstract class UI
    {
        public TaskCompletionSource<bool> Tcs = null;
        public UIData UiData = null;
        public GameObject GameObject = null;
        public Transform Transform = null;
        public Transform ChildParentNode = null;
        public UIStateType UIState = UIStateType.None;
        public AnimationStateType AnimationState = AnimationStateType.None;
        public bool AwakeState = false;
        public int SortingOrder = 0;
        public UIProxy UIProxy = null;

        protected Dictionary<Canvas, int> CanvasDic = null;

        Dictionary<string, PlayableDirector> PlayableDirectors = new Dictionary<string, PlayableDirector>();

        //当前是否播放动画
        protected TaskCompletionSource<bool> StartPlayingAnimationTask;
        protected TaskCompletionSource<bool> EnablePlayingAnimationTask;
        protected TaskCompletionSource<bool> DisablePlayingAnimationTask;
        protected TaskCompletionSource<bool> DestroyPlayingAnimationTask;

        protected UI(UIData uiData)
        {
            this.UiData = uiData;
            Tcs = new TaskCompletionSource<bool>();

            SortingOrder = 0;

            if (UiData.IsLuaUI)
            {
                UIProxy = new UILuaProxy();
            }
            else
            {
                Type type = UIManager.Instance.GetType(UiData.UiName);
                UIProxy = Activator.CreateInstance(type) as UIProxy;
            }

            UIProxy?.SetUi(this);
        }

        public void SetGameObject(GameObject gameObject)
        {
            this.GameObject = gameObject;
            this.GameObject.name = this.UiData.UiName;
            this.Transform = this.GameObject.transform;
            this.ChildParentNode = this.Transform.FindTransform("ChildParentNode");

            UIManager.Instance.SetUIParent(this, false);

            //记录所有PlayableDirector
            if (this.UiData.HasAnimation)
            {
                Transform animationTrans = Transform.Find("Animation");
                if (animationTrans != null)
                {
                    PlayableDirector[] directors = animationTrans.GetComponentsInChildren<PlayableDirector>(true);
                    PlayableDirectors.Clear();
                    for (int i = 0; i < directors.Length; i++)
                    {
                        PlayableDirector director = directors[i];

                        if (!director)
                            continue;

                        string animName = director.gameObject.name;

                        if (PlayableDirectors.ContainsKey(animName))
                        {
                            Debug.LogError($"{UiData.UiName}存在同名动画:{animName}");
                            return;
                        }

                        director.playOnAwake = false;
                        PlayableDirectors.Add(animName, director);
                    }
                }
            }

            //记录所有Canvas初始化的sortingOrder
            Canvas[] tempCanvases = this.GameObject.GetComponentsInChildren<Canvas>(true);
            CanvasDic = new Dictionary<Canvas, int>(tempCanvases.Length);
            for (int i = 0; i < tempCanvases.Length; i++)
            {
                Canvas tempCanvas = tempCanvases[i];
                CanvasDic[tempCanvas] = tempCanvas.sortingOrder;
            }

            this.GameObject.SetActive(false);

            UIProxy.SetGameObejct();
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
            if (this.UIState <= UIStateType.Loading)
            {
                this.UIState = UIStateType.Awake;
                AwakeState = true;
                UIManager.Instance.NotifyBeforeAwake(this);
                this.OnAwake();
                UIManager.Instance.NotifyAfterAwake(this);
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
            if (this.UIState <= UIStateType.Loading)
                return;

            if (this.UIState == UIStateType.Awake)
            {
                Start(args);
                PlayChildEnableAnimation();
                Enable();

                this.AnimationState = AnimationStateType.Start;

                //播放进场动画
                if (this.UiData.HasAnimation)
                {
                    if (!PlayAnimation(AnimationStateType.Start))
                        PlayAnimation(AnimationStateType.Enable);
                }

                await this.WaitAnimationFinished();
            }
        }

        public virtual void Start(params object[] args)
        {
            if (this.UIState == UIStateType.Awake)
            {
                this.UIState = UIStateType.Start;
                this.GameObject?.SetActive(true);
                UIManager.Instance.NotifyBeforeStart(this);
                this.OnStart(args);
                UIManager.Instance.NotifyAfterStart(this);
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
            if (this.UIState <= UIStateType.Loading)
                return;

            if (this.UIState == UIStateType.Start || this.UIState == UIStateType.Disable)
            {
                Enable();
                PlayChildEnableAnimation();
                this.AnimationState = AnimationStateType.Disable;

                //播放Resume动画
                PlayAnimation(AnimationStateType.Enable);

                await this.WaitAnimationFinished();
            }
        }

        public virtual void PlayChildEnableAnimation()
        {
        }

        public virtual void Enable()
        {
            if (this.UIState == UIStateType.Start || this.UIState == UIStateType.Disable)
            {
                this.UIState = UIStateType.Enable;
                this.GameObject?.SetActive(true);

                UIManager.Instance.NotifyBeforeEnable(this);

                this.OnEnable();
                //注册事件监听
                if (UIProxy.Events != null)
                {
                    for (int i = 0; i < UIProxy.Events.Length; i++)
                    {
                        string evt = UIProxy.Events[i];
                        GameEventManager.Instance.RegisterEvent(evt, UIProxy.OnNotify);
                    }
                }

                UIManager.Instance.NotifyAfterEnable(this);
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
            if (this.UIState <= UIStateType.Loading)
                return;

            if (this.UIState == UIStateType.Start || this.UIState == UIStateType.Enable)
            {
                PlayChildDisableAnimation();
                this.AnimationState = AnimationStateType.Disable;

                //播放暂停动画
                PlayAnimation(AnimationStateType.Disable);

                await this.WaitAnimationFinished();

                Disable();
            }
        }

        public virtual void PlayChildDisableAnimation()
        {
        }

        public virtual void Disable()
        {
            if (this.UIState <= UIStateType.Loading)
                return;

            //只有状态为Start、Enable才能执行disable
            if (this.UIState == UIStateType.Start || this.UIState == UIStateType.Enable)
            {
                this.UIState = UIStateType.Disable;

                if (UIProxy.Events != null)
                {
                    for (int i = 0; i < UIProxy.Events.Length; i++)
                    {
                        string evt = UIProxy.Events[i];
                        GameEventManager.Instance.RemoveEvent(evt, UIProxy.OnNotify);
                    }
                }

                UIManager.Instance.NotifyBeforeDisable(this);

                OnDisable();
                this.GameObject?.SetActive(false);

                UIManager.Instance.NotifyAfterDisable(this);
            }
        }

        public void OnDisable()
        {
            this.UIProxy?.OnDisable();
        }

        #endregion


        #region Destroy

        public virtual void PlayChildDestroyAnimation() { }

        public async Task DestroyAsync()
        {
            if (UIState <= UIStateType.Loading)
            {
                return;
            }

            if (UIState == UIStateType.Start || UIState == UIStateType.Enable)
            {
                PlayChildDestroyAnimation();
                AnimationState = AnimationStateType.Destroy;

                //播放暂停动画
                if (!PlayAnimation(AnimationStateType.Destroy))
                {
                    PlayAnimation(AnimationStateType.Disable);
                }

                await WaitAnimationFinished();

                Disable();
            }
        }

        public virtual void Destroy()
        {
            if (this.UIState <= UIStateType.Loading)
                return;

            Disable();
            if (this.UIState != UIStateType.Destroy)
            {
                UIManager.Instance.NotifyBeforeDestroy(this);

                this.OnDestroy();

                if (this.GameObject)
                    GameObject.Destroy(this.GameObject);
                GameObject = null;
                Transform = null;
                UIState = UIStateType.Destroy;
                AnimationState = AnimationStateType.Destroy;
                AwakeState = false;
                StartPlayingAnimationTask = null;
                EnablePlayingAnimationTask = null;
                DisablePlayingAnimationTask = null;
                DestroyPlayingAnimationTask = null;
                Tcs = null;

                UIManager.Instance.NotifyAfterDestroy(this);
            }
        }

        public void OnDestroy()
        {
            this.UIProxy?.OnDestroy();
        }

        #endregion

        public void PlayAnimation(string animName, Action finishedCallback = null)
        {
            PlayableDirector playableDirector = null;
            if (!PlayableDirectors.TryGetValue(animName, out playableDirector))
            {
                Debug.LogError($"{UiData.UiName}不存在动画:{animName}");
                return;
            }

            if (!playableDirector)
                return;

            playableDirector.gameObject.PlayTimelineAnimation(animName, () =>
            {
                finishedCallback?.Invoke();
            });
        }

        private bool PlayAnimation(AnimationStateType state)
        {
            string animName = null;

            switch (state)
            {
                case AnimationStateType.Start:
                    animName = "AnimStart";
                    break;
                case AnimationStateType.Enable:
                    animName = "AnimEnable";
                    break;
                case AnimationStateType.Disable:
                    animName = "AnimDisable";
                    break;
                case AnimationStateType.Destroy:
                    animName = "AnimDestroy";
                    break;
            }

            PlayableDirector director = null;
            if (!PlayableDirectors.TryGetValue(animName, out director))
            {
                return false;
            }

            if (!director.playableAsset)
            {
                return false;
            }

            TaskCompletionSource<bool> task = null;
            //创建Task
            switch (state)
            {
                case AnimationStateType.Start:
                    StartPlayingAnimationTask = new TaskCompletionSource<bool>();
                    task = StartPlayingAnimationTask;
                    break;
                case AnimationStateType.Enable:
                    EnablePlayingAnimationTask = new TaskCompletionSource<bool>();
                    task = EnablePlayingAnimationTask;
                    break;
                case AnimationStateType.Disable:
                    DisablePlayingAnimationTask = new TaskCompletionSource<bool>();
                    task = DisablePlayingAnimationTask;
                    break;
                case AnimationStateType.Destroy:
                    DestroyPlayingAnimationTask = new TaskCompletionSource<bool>();
                    task = DestroyPlayingAnimationTask;
                    break;
            }

            PlayAnimation(animName, () =>
            {
                if (task != null)
                    task.SetResult(true);
            });

            return true;
        }

        /// <summary>
        /// 等待动画播放完成Task
        /// </summary>
        /// <returns></returns>
        public virtual async Task WaitAnimationFinished()
        {
            if (StartPlayingAnimationTask != null)
                await StartPlayingAnimationTask.Task;

            if (EnablePlayingAnimationTask != null)
                await EnablePlayingAnimationTask.Task;

            if (DisablePlayingAnimationTask != null)
                await DisablePlayingAnimationTask.Task;

            if (DestroyPlayingAnimationTask != null)
                await DestroyPlayingAnimationTask.Task;
        }

        /// <summary>
        /// 当前是否播放动画
        /// </summary>
        /// <returns></returns>
        //public Task GetPlayingAniamtionTask()
        //{
        //    return IsPlayingAnimationTask.Task;
        //}
    }
}