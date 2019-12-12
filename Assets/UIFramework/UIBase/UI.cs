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
        public TaskCompletionSource<bool> tcs { get; private set; }
        public UIData uiData { get; set; } = null;
        public GameObject gameObject { get; private set; } = null;
        public Transform transform { get; private set; } = null;
        public Transform childParentNode { get; set; } = null;
        public UIStateType uiState { get; set; } = UIStateType.None;
        public AnimationStateType animationState { get; private set; } = AnimationStateType.None;
        public bool awakeState { get; set; } = false;
        public int sortingOrder { get; set; } = 0;
        public UIProxy uiProxy { get; set; } = null;

        private Dictionary<Canvas, int> m_CanvasDic = null;

        private readonly Dictionary<string, PlayableDirector> m_PlayableDirectors = new Dictionary<string, PlayableDirector>();

        //当前是否播放动画
        private TaskCompletionSource<bool> m_StartPlayingAnimationTask;
        private TaskCompletionSource<bool> m_EnablePlayingAnimationTask;
        private TaskCompletionSource<bool> m_DisablePlayingAnimationTask;
        private TaskCompletionSource<bool> m_DestroyPlayingAnimationTask;

        protected UI(UIData uiData)
        {
            this.uiData = uiData;
            tcs = new TaskCompletionSource<bool>();

            sortingOrder = 0;

            if (this.uiData.isLuaUI)
            {
#if XLUA
                uiProxy = new UILuaProxy();
#endif
            }
            else
            {
                Type type = UIManager.instance.GetType(this.uiData.uiName);
                uiProxy = Activator.CreateInstance(type) as UIProxy;
            }

            uiProxy?.SetUi(this);
        }

        public void SetGameObject(GameObject gameObject)
        {
            this.gameObject = gameObject;
            this.gameObject.name = uiData.uiName;
            transform = this.gameObject.transform;
            childParentNode = transform.FindTransform("ChildParentNode");

            UIManager.instance.SetUIParent(this, false);

            //记录所有PlayableDirector
            if (uiData.hasAnimation)
            {
                Transform animationTrans = transform.Find("Animation");
                if (animationTrans != null)
                {
                    PlayableDirector[] directors = animationTrans.GetComponentsInChildren<PlayableDirector>(true);
                    m_PlayableDirectors.Clear();
                    for (int i = 0; i < directors.Length; i++)
                    {
                        PlayableDirector director = directors[i];

                        if (!director)
                            continue;

                        string animName = director.gameObject.name;

                        if (m_PlayableDirectors.ContainsKey(animName))
                        {
                            Debug.LogError($"{uiData.uiName}存在同名动画:{animName}");
                            return;
                        }

                        director.playOnAwake = false;
                        m_PlayableDirectors.Add(animName, director);
                    }
                }
            }

            //记录所有Canvas初始化的sortingOrder
            Canvas[] tempCanvases = this.gameObject.GetComponentsInChildren<Canvas>(true);
            m_CanvasDic = new Dictionary<Canvas, int>(tempCanvases.Length);
            for (int i = 0; i < tempCanvases.Length; i++)
            {
                Canvas tempCanvas = tempCanvases[i];
                m_CanvasDic[tempCanvas] = tempCanvas.sortingOrder;
            }

            this.gameObject.SetActive(false);

            uiProxy.SetGameObejct();
        }

        /// <summary>
        /// 设置界面层级
        /// </summary>
        /// <param name="order"></param>
        public void SetCavansOrder(int order)
        {
            sortingOrder = order;

            if (m_CanvasDic != null)
            {
                foreach (var kv in m_CanvasDic)
                    kv.Key.sortingOrder = kv.Value + order;
            }
        }

#region Awake

        public virtual void Awake()
        {
            if (uiState <= UIStateType.Loading)
            {
                uiState = UIStateType.Awake;
                awakeState = true;
                UIManager.instance.NotifyBeforeAwake(this);
                OnAwake();
                UIManager.instance.NotifyAfterAwake(this);
                gameObject.SetActive(false);
            }
        }

        public void OnAwake()
        {
            uiProxy?.OnAwake();
        }

#endregion


#region Start

        public virtual async Task StartAsync(params object[] args)
        {
            if (uiState <= UIStateType.Loading)
                return;

            if (uiState == UIStateType.Awake)
            {
                Start(args);
                PlayChildEnableAnimation();
                Enable();

                animationState = AnimationStateType.Start;

                //播放进场动画
                if (uiData.hasAnimation)
                {
                    if (!PlayAnimation(AnimationStateType.Start))
                        PlayAnimation(AnimationStateType.Enable);
                }

                await WaitAnimationFinished();
            }
        }

        public virtual void Start(params object[] args)
        {
            if (uiState == UIStateType.Awake)
            {
                uiState = UIStateType.Start;
                gameObject?.SetActive(true);
                UIManager.instance.NotifyBeforeStart(this);
                OnStart(args);
                UIManager.instance.NotifyAfterStart(this);
            }
        }

        public void OnStart(params object[] args)
        {
            uiProxy?.OnStart(args);
        }

#endregion


#region Enable

        public async Task EnableAsync()
        {
            if (uiState <= UIStateType.Loading)
                return;

            if (uiState == UIStateType.Start || uiState == UIStateType.Disable)
            {
                Enable();
                PlayChildEnableAnimation();
                animationState = AnimationStateType.Disable;

                //播放Resume动画
                PlayAnimation(AnimationStateType.Enable);

                await WaitAnimationFinished();
            }
        }

        public virtual void PlayChildEnableAnimation()
        {
        }

        public virtual void Enable()
        {
            if (uiState == UIStateType.Start || uiState == UIStateType.Disable)
            {
                uiState = UIStateType.Enable;
                gameObject?.SetActive(true);

                UIManager.instance.NotifyBeforeEnable(this);

                OnEnable();
                //注册事件监听
                if (uiProxy.events != null)
                {
                    for (int i = 0; i < uiProxy.events.Length; i++)
                    {
                        string evt = uiProxy.events[i];
                        EventManager.instance.RegisterEvent(evt, uiProxy.OnNotify);
                    }
                }

                UIManager.instance.NotifyAfterEnable(this);
            }
        }

        public void OnEnable()
        {
            uiProxy?.OnEnable();
        }

#endregion


#region Disable

        public async Task DisableAsync()
        {
            if (uiState <= UIStateType.Loading)
                return;

            if (uiState == UIStateType.Start || uiState == UIStateType.Enable)
            {
                PlayChildDisableAnimation();
                animationState = AnimationStateType.Disable;

                //播放暂停动画
                PlayAnimation(AnimationStateType.Disable);

                await WaitAnimationFinished();

                Disable();
            }
        }

        public virtual void PlayChildDisableAnimation()
        {
        }

        public virtual void Disable()
        {
            if (uiState <= UIStateType.Loading)
                return;

            //只有状态为Start、Enable才能执行disable
            if (uiState == UIStateType.Start || uiState == UIStateType.Enable)
            {
                uiState = UIStateType.Disable;

                if (uiProxy.events != null)
                {
                    for (int i = 0; i < uiProxy.events.Length; i++)
                    {
                        string evt = uiProxy.events[i];
                        EventManager.instance.RemoveEvent(evt, uiProxy.OnNotify);
                    }
                }

                UIManager.instance.NotifyBeforeDisable(this);

                OnDisable();
                gameObject?.SetActive(false);

                UIManager.instance.NotifyAfterDisable(this);
            }
        }

        public void OnDisable()
        {
            uiProxy?.OnDisable();
        }

#endregion


#region Destroy

        public virtual void PlayChildDestroyAnimation() { }

        public async Task DestroyAsync()
        {
            if (uiState <= UIStateType.Loading)
            {
                return;
            }

            if (uiState == UIStateType.Start || uiState == UIStateType.Enable)
            {
                PlayChildDestroyAnimation();
                animationState = AnimationStateType.Destroy;

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
            if (uiState <= UIStateType.Loading)
                return;

            Disable();
            if (uiState != UIStateType.Destroy)
            {
                UIManager.instance.NotifyBeforeDestroy(this);

                OnDestroy();

                if (gameObject)
                    GameObject.Destroy(gameObject);
                gameObject = null;
                transform = null;
                uiState = UIStateType.Destroy;
                animationState = AnimationStateType.Destroy;
                awakeState = false;
                m_StartPlayingAnimationTask = null;
                m_EnablePlayingAnimationTask = null;
                m_DisablePlayingAnimationTask = null;
                m_DestroyPlayingAnimationTask = null;
                tcs = null;

                UIManager.instance.NotifyAfterDestroy(this);
            }
        }

        public void OnDestroy()
        {
            uiProxy?.OnDestroy();
        }

#endregion

        public void PlayAnimation(string animName, Action finishedCallback = null)
        {
            PlayableDirector playableDirector = null;
            if (!m_PlayableDirectors.TryGetValue(animName, out playableDirector))
            {
                Debug.LogError($"{uiData.uiName}不存在动画:{animName}");
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
            if (!m_PlayableDirectors.TryGetValue(animName, out director))
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
                    m_StartPlayingAnimationTask = new TaskCompletionSource<bool>();
                    task = m_StartPlayingAnimationTask;
                    break;
                case AnimationStateType.Enable:
                    m_EnablePlayingAnimationTask = new TaskCompletionSource<bool>();
                    task = m_EnablePlayingAnimationTask;
                    break;
                case AnimationStateType.Disable:
                    m_DisablePlayingAnimationTask = new TaskCompletionSource<bool>();
                    task = m_DisablePlayingAnimationTask;
                    break;
                case AnimationStateType.Destroy:
                    m_DestroyPlayingAnimationTask = new TaskCompletionSource<bool>();
                    task = m_DestroyPlayingAnimationTask;
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
            if (m_StartPlayingAnimationTask != null)
                await m_StartPlayingAnimationTask.Task;

            if (m_EnablePlayingAnimationTask != null)
                await m_EnablePlayingAnimationTask.Task;

            if (m_DisablePlayingAnimationTask != null)
                await m_DisablePlayingAnimationTask.Task;

            if (m_DestroyPlayingAnimationTask != null)
                await m_DestroyPlayingAnimationTask.Task;
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