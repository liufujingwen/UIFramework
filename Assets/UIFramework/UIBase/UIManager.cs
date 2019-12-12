using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace UIFramework
{
    /// <summary>
    /// UI管理器
    /// </summary>
    public class UIManager
    {
        public static UIManager instance { get; } = new UIManager();

        //UI生命周期事件
        public const string EVENT_UI_AWAKE = "EVENT_UI_AWAKE";//UI执行Awake通知
        public const string EVENT_UI_START = "EVENT_UI_START";//UI执行Start通知
        public const string EVENT_UI_ENABLE = "EVENT_UI_ENABLE";//UI执行Enable通知
        public const string EVENT_UI_DISABLE = "EVENT_UI_DISABLE";//UI执行Disable通知
        public const string EVENT_UI_DESTROY = "EVENT_UI_DESTROY";//UI执行Destroy通知

        /// <summary>
        /// 忽略Mask的类型
        /// </summary>
        public const UIType IGNORE_MASK_TYPE = UIType.Tips | UIType.TopMask | UIType.Top;

        /// <summary>
        /// 堆栈管理的UI类型
        /// </summary>
        public const UIType STACK_TYPE = UIType.Normal | UIType.NormalPopup | UIType.Popup;

        //共享Normal栈的类型
        public const UIType SHARE_NORMAL_TYPE = UIType.Normal | UIType.NormalPopup;

        //所有一级UI必须注册后才能创建
        private readonly Dictionary<string, UIData> m_UIRegisterDic = new Dictionary<string, UIData>();

        //C#的UI逻辑Type
        private readonly Dictionary<string, Type> m_UITypeDic = new Dictionary<string, Type>();

        //遮罩go，这个mask的作用是防止一切的点击
        private GameObject m_MaskGO = null;
        //mask引用计数器
        private int m_MaskCount = 0;

        /// <summary>
        /// 显示管理器
        /// </summary>
        private readonly Dictionary<UIType, IUIContainer> m_ShowDic = new Dictionary<UIType, IUIContainer>();

        //HUD相机
        private Camera m_HUDCamera = null;
        //UI相机
        private Camera m_UICamera = null;
        //UIRoot
        private Transform m_UIRoot = null;
        //不销毁的UI存放位置
        private Transform m_PoolCanvas = null;
        //保存UIType对应的界面父节点
        private readonly Dictionary<UIType, Canvas> m_CanvasDic = new Dictionary<UIType, Canvas>();

        /// <summary>
        /// 正在关闭全部（避免在关闭过程中有些UI写了打开某个ui在disable、destroy里面）
        /// </summary>
        public bool closingAll { get; private set; } = false;

        /// <summary>
        /// 当前显示的UI,包含子UI
        /// </summary>
        private readonly List<UI> m_ShowList = new List<UI>();

        /// <summary>
        /// 已加载的所有UI
        /// </summary>
        private readonly List<UI> m_AllLoadList = new List<UI>();

        public void Init()
        {
            InitTypes();
            ProcessingUIDataMapping();
            InitUIRoot(UIResType.Resorces);
            CreateMask();
        }

        /// <summary>
        /// 创建UIMask,这个mask的作用是防止一切的点击
        /// </summary>
        public void CreateMask()
        {
            m_MaskGO = UnityEngine.Object.Instantiate(Resources.Load("UI/MaskUI")) as GameObject;
            Canvas topCanvas = m_CanvasDic[UIType.Top];
            Canvas maskCanvas = m_MaskGO.GetComponent<Canvas>();
            maskCanvas.sortingOrder = topCanvas.sortingOrder;
            m_MaskGO.transform.SetParent(topCanvas.transform, false);
            m_MaskGO.SetActive(false);
        }

        //控制mask的显隐
        public void SetMask(bool visible)
        {
            m_MaskCount += visible ? 1 : -1;
            m_MaskGO.SetActive(m_MaskCount > 0);
        }

        //清除mask，计数器设置为0
        public void ClearMask()
        {
            m_MaskCount = 0;
            m_MaskGO.SetActive(m_MaskCount > 0);
        }

        public void InitUIRoot(UIResType uiResType)
        {
            //把所有UI先清空

            GameObject uiRoot = null;
            if (uiResType == UIResType.Resorces)
            {
                uiRoot = GameObject.Instantiate(Resources.Load("UI/UIRoot")) as GameObject;
            }
            else
            {
                //从AssetBundle加载
                //uiRoot = GameObject.Instantiate(Resources.Load("UI/UIRoot")) as GameObject;
            }
            ChangeUIRoot(uiRoot.transform);
        }

        /// <summary>
        /// 切换UIRoot
        /// </summary>
        /// <param name="uiRoot"></param>
        void ChangeUIRoot(Transform uiRoot)
        {
            if (uiRoot == null)
                return;

            //先记录旧的UIRoot
            Transform oldUIRoot = this.m_UIRoot;

            m_UIRoot = uiRoot;
            m_HUDCamera = uiRoot.gameObject.FindComponent<Camera>("CameraHUD");
            m_UICamera = uiRoot.gameObject.FindComponent<Camera>("Camera");
            m_PoolCanvas = uiRoot.FindTransform("CanvasPool");

            Array uiTypeArray = Enum.GetValues(typeof(UIType));
            foreach (UIType uiType in uiTypeArray)
            {
                if (uiType == UIType.Child)
                    continue;

                if (uiType != UIType.Normal && (uiType & SHARE_NORMAL_TYPE) != 0)
                    continue;

                Canvas tempCanvas = uiRoot.gameObject.FindComponent<Canvas>($"Canvas{uiType}");
                m_CanvasDic[uiType] = tempCanvas;

                if (!m_ShowDic.ContainsKey(uiType))
                {
                    if ((uiType & STACK_TYPE) != 0)
                        m_ShowDic[uiType] = new UIStackContainer(uiType, tempCanvas.sortingOrder);
                    else
                        m_ShowDic[uiType] = new UIListContainer(uiType, tempCanvas.sortingOrder);
                }
            }

            //设置共享Normal层级的类型
            foreach (UIType uiType in uiTypeArray)
            {
                if ((uiType & SHARE_NORMAL_TYPE) == 0)
                    continue;
                m_ShowDic[uiType] = m_ShowDic[UIType.Normal];
            }

            //改变ui的父节点
            foreach (var kv in m_ShowDic)
            {
                if (kv.Key != UIType.Normal && (kv.Key & SHARE_NORMAL_TYPE) != 0)
                    continue;
                kv.Value.SetUiParent(m_CanvasDic[kv.Key].transform, false);
            }

            //改变mask的父节点
            if (m_MaskGO != null)
            {
                Transform topCanvas = m_CanvasDic[UIType.Top].transform;
                m_MaskGO.transform.SetParent(topCanvas, false);
            }

            if (oldUIRoot != null)
            {
                GameObject.Destroy(oldUIRoot.gameObject);
                oldUIRoot = null;
            }
        }

        void InitTypes()
        {
            //获取所有Mono的UIType
            m_UITypeDic.Clear();
            Type[] types = DllHelper.GetMonoTypes();
            for (int i = 0; i < types.Length; i++)
            {
                Type type = types[i];
                object[] attrs = type.GetCustomAttributes(typeof(UIAttribute), false);
                foreach (object attr in attrs)
                {
                    UIAttribute uiAttribute = (UIAttribute)attr;
                    if (uiAttribute != null)
                    {
                        m_UITypeDic[uiAttribute.uiName] = type;
                        Register(uiAttribute.uiName, uiAttribute.uiType, uiAttribute.uiResType, uiAttribute.hasAnimation, false);
                    }
                }

                object[] childAttrs = type.GetCustomAttributes(typeof(UIChildAttribute), false);
                foreach (object attr in childAttrs)
                {
                    UIChildAttribute childAttribute = (UIChildAttribute)attr;
                    if (childAttribute != null)
                    {
                        m_UITypeDic[childAttribute.uiName] = type;
                        RegisterChild(childAttribute.uiName, childAttribute.parentUIName, childAttribute.loadWithParent, childAttribute.uiResType, childAttribute.hasAnimation, false);
                    }
                }
            }
        }

        /// <summary>
        /// 处理UI的父子关系，必须所有UI注册完后才能处理这个映射关系
        /// </summary>
        public void ProcessingUIDataMapping()
        {
            foreach (var kv in m_UIRegisterDic)
            {
                UIData uiData = null;
                if (!string.IsNullOrEmpty(kv.Value.parentUIName))
                {
                    if (m_UIRegisterDic.TryGetValue(kv.Value.parentUIName, out uiData))
                    {
                        if (uiData.childDic == null)
                            uiData.childDic = new Dictionary<string, UIData>(5);
                        uiData.childDic.Add(kv.Key, kv.Value);
                    }
                }
            }
        }

        /// <summary>
        /// 注册UI
        /// </summary>
        /// <param name="uiName">UI名字</param>
        /// <param name="uiType">UI类型</param>
        /// <param name="uiResType">UI加载方式</param>
        /// <param name="hasAnimation">UI是否有动画</param>
        public void Register(string uiName, UIType uiType, UIResType uiResType, bool hasAnimation, bool isLuaUI)
        {
            UIData uiData = new UIData();
            uiData.uiName = uiName;
            uiData.uiType = uiType;
            uiData.uiResType = uiResType;
            uiData.hasAnimation = hasAnimation;
            uiData.isLuaUI = isLuaUI;
            uiData.parentUIName = null;
            m_UIRegisterDic.Add(uiName, uiData);
        }

        /// <summary>
        /// 注册子UI
        /// </summary>
        /// <param name="uiName">UI名字</param>
        /// <param name="parentUIName">父窗口UI名字</param>
        /// <param name="loadWithParent">是否和父窗口一起加载</param>
        /// <param name="uiType">UI类型</param>
        /// <param name="uiResType">UI加载方式</param>
        /// <param name="uiCloseType">UI关闭方式</param>
        /// <param name="hasAnimation">UI是否有动画</param>
        public void RegisterChild(string uiName, string parentUIName, bool loadWithParent, UIResType uiResType, bool hasAnimation, bool isLuaUI)
        {
            UIData uiData = new UIData();
            uiData.uiName = uiName;
            uiData.parentUIName = parentUIName;
            uiData.loadWithParent = loadWithParent;
            uiData.uiType = UIType.Child;
            uiData.uiResType = uiResType;
            uiData.hasAnimation = hasAnimation;
            uiData.isLuaUI = isLuaUI;
            m_UIRegisterDic.Add(uiName, uiData);
        }

        public GameUI CreateUI(string uiName)
        {
            //加载新UI
            UIData uiData = null;
            m_UIRegisterDic.TryGetValue(uiName, out uiData);

            if (uiData == null)
            {
                Debug.LogError($"{uiName}:不存在");
                return null;
            }

            if (uiData.isChildUI)
            {
                Debug.LogError($"子UI:{uiName}不能使用CreateUI创建");
                return null;
            }

            GameUI ui = new GameUI(uiData);

            //创建子UI
            if (ui.uiData.hasChildUI)
            {
                foreach (var kv in ui.uiData.childDic)
                {
                    ChildUI childUi = ui.FindChildUi(kv.Key);
                    if (childUi != null)
                        continue;

                    childUi = new ChildUI(kv.Value);
                    ui.AddChildUI(kv.Key, childUi);
                }
            }

            return ui;
        }

        /// <summary>
        /// 加载UI
        /// </summary>
        /// <param name="ui">UI</param>
        /// <returns></returns>
        public async Task LoadUIAsync(UI ui)
        {
            await LoadUiTask(ui);
            //尝试加载加载LoadWithParent=true的子UI
            await TryLoadChildUI(ui);

            //保证所有UI只执行一次Awake
            if (!ui.awakeState)
                ui.Awake();
        }

        /// <summary>
        /// 加载UI
        /// </summary>
        /// <param name="uiName">ui名字</param>
        /// <returns></returns>
        private Task LoadUiTask(UI ui)
        {
            if (ui.uiState == UIStateType.None)
                LoadAsset(ui);
            return ui.tcs.Task;
        }

        /// <summary>
        /// 尝试加载LoadWithParent=true的子UI
        /// </summary>
        /// <returns></returns>
        public async Task TryLoadChildUI(UI ui)
        {
            if (ui.uiData.hasChildUI)
            {
                foreach (var kv in ui.uiData.childDic)
                {
                    if (kv.Value.loadWithParent)
                    {
                        GameUI gameUI = ui as GameUI;
                        ChildUI childUi = gameUI.FindChildUi(kv.Key);
                        await LoadUiTask(childUi);
                    }
                }
            }
        }

        /// <summary>
        /// 通过UIType设置界面的父节点
        /// </summary>
        /// <param name="ui">UI界面</param>
        /// <param name="worldPositionStays">If true, the parent-relative position, scale and rotation are modified such that</param>
        public void SetUIParent(UI ui, bool worldPositionStays)
        {
            if (ui == null)
                return;

            if (!ui.transform)
                return;

            Canvas tempCanvas = null;

            UIType uiType = ui.uiData.uiType;

            if (uiType != UIType.Child && (ui.uiData.uiType & SHARE_NORMAL_TYPE) != 0)
            {
                uiType = UIType.Normal;
            }

            if (m_CanvasDic.TryGetValue(uiType, out tempCanvas))
            {
                if (!ui.transform.IsChildOf(tempCanvas.transform))
                    ui.transform.SetParent(tempCanvas.transform, worldPositionStays);
            }
            else if (uiType == UIType.Child)
            {
                ChildUI childUi = ui as ChildUI;
                if (childUi.parentUI != null && childUi.parentUI.childParentNode)
                {
                    if (!ui.transform.IsChildOf(childUi.parentUI.childParentNode))
                        ui.transform.SetParent(childUi.parentUI.childParentNode, worldPositionStays);
                }
                else if (!ui.transform.IsChildOf(childUi.parentUI.transform))
                {
                    ui.transform.SetParent(childUi.parentUI.transform, worldPositionStays);
                }
            }

            if (worldPositionStays)
            {
                ui.transform.localScale = Vector3.one;
                ui.transform.localPosition = Vector3.zero;
                ui.transform.localRotation = Quaternion.identity;
            }
        }

        /// <summary>
        /// 通过UIType设置界面的父节点
        /// </summary>
        /// <param name="transform">界面transform</param>
        /// <param name="uiType">ui类型</param>
        /// <param name="worldPositionStays">If true, the parent-relative position, scale and rotation are modified such that</param>
        public void SetUIParent(Transform transform, UIType uiType, bool worldPositionStays)
        {
            if (!transform)
                return;

            Canvas tempCanvas = null;
            if (m_CanvasDic.TryGetValue(uiType, out tempCanvas))
            {
                if (transform.parent != tempCanvas.transform)
                    transform.SetParent(tempCanvas.transform, worldPositionStays);
            }
            else if (uiType == UIType.Child)
            {
                //子UI暂时放到poolCanvas
                if (transform.parent == null)
                    transform.SetParent(m_PoolCanvas.transform, worldPositionStays);
            }
        }

        /// <summary>
        /// 通过名字查找UIData
        /// </summary>
        /// <returns></returns>
        private UIData FindUIData(string uiName)
        {
            UIData uiData = null;
            m_UIRegisterDic.TryGetValue(uiName, out uiData);
            return uiData;
        }

        /// <summary>
        /// 打开UI
        /// </summary>
        /// <param name="uiName">UI名字</param>
        /// <param name="args">传递到0nStart的参数</param>
        public void Open(string uiName, params object[] args)
        {
            UIData uiData = FindUIData(uiName);
            if (uiData == null)
            {
                Debug.LogError($"{uiName}未注册");
                return;
            }

            IUIContainer uiContainer = null;
            if (m_ShowDic.TryGetValue(uiData.uiType, out uiContainer))
                uiContainer?.Open(uiName, null, args);
        }

        /// <summary>
        /// 打开UI
        /// </summary>
        /// <param name="ui">UI</param>
        /// <param name="args">传递到0nStart的参数</param>
        public void Open(UI ui, params object[] args)
        {
            IUIContainer uiContainer = null;
            if (m_ShowDic.TryGetValue(ui.uiData.uiType, out uiContainer))
                uiContainer?.Open(ui, null, args);
        }

        /// <summary>
        /// 打开UI，完成后执行回调
        /// </summary>
        /// <param name="uiName">UI名字</param>
        /// <param name="callback">回调</param>
        /// <param name="args">传递到0nStart的参数</param>
        public void OpenWithCallback(string uiName, Action<UI> callback, params object[] args)
        {
            UIData uiData = FindUIData(uiName);
            if (uiData == null)
            {
                Debug.LogError($"OpenWithCallbackAsync {uiName}未注册");
                return;
            }

            IUIContainer uiContainer = null;
            if (m_ShowDic.TryGetValue(uiData.uiType, out uiContainer))
                uiContainer?.Open(uiName, callback, args);
        }

        /// <summary>
        /// 关闭Normal栈顶界面(对noraml出栈的会计方式，不需要传uitype)
        /// </summary>
        /// <param name="uiType">ui类型</param>
        public void Pop()
        {
            Pop(UIType.Normal);
        }

        /// <summary>
        /// 关闭栈顶界面
        /// </summary>
        /// <param name="uiType">ui类型</param>
        public void Pop(UIType uiType)
        {
            IUIContainer uiContainer = null;
            if (m_ShowDic.TryGetValue(uiType, out uiContainer))
                uiContainer?.Pop(null);
        }

        /// <summary>
        /// Normal类型的UI退栈并执行回调
        /// </summary>
        /// <param name="callback">回调</param>
        public void PopWithCallback(Action callback)
        {
            PopWithCallback(UIType.Normal, callback);
        }

        /// <summary>
        /// UI退栈并执行回调
        /// </summary>
        /// <param name="uiType">UI类型</param>
        /// <param name="callback">回调</param>
        public void PopWithCallback(UIType uiType, Action callback)
        {
            IUIContainer uiContainer = null;
            if (m_ShowDic.TryGetValue(uiType, out uiContainer))
                uiContainer?.Pop(callback);
        }

        /// <summary>
        /// 针对Normal类型的管理，关闭上一个界面，然后打开下一个界面（无缝切换）
        /// </summary>
        /// <param name="uiName">UI名字</param>
        /// <param name="args">传递到0nStart的参数</param>
        public void PopThenOpen(string uiName, params object[] args)
        {
            PopThenOpen(UIType.Normal, uiName, args);
        }

        /// <summary>
        /// 指定类型UI退栈后，并打开指定名字的UI
        /// </summary>
        /// <param name="uiType">UI类型</param>
        /// <param name="uiName">打开的UI名字</param>
        /// <param name="args">传递到0nStart的参数</param>
        public void PopThenOpen(UIType uiType, string uiName, params object[] args)
        {
            IUIContainer uiContainer = null;
            if (m_ShowDic.TryGetValue(uiType, out uiContainer))
                uiContainer?.PopThenOpen(uiName, args);
        }

        /// <summary>
        /// 针对Normal类型的管理，关闭栈中所有界面，然后打开下一个界面（无缝切换）
        /// </summary>
        /// <param name="uiName">UI名字</param>
        /// <param name="args">传递到0nStart的参数</param>
        public void PopAllThenOpen(string uiName, params object[] args)
        {
            PopAllThenOpen(UIType.Normal, uiName, args);
        }

        /// <summary>
        /// 关闭指定类型的栈所有界面，然后打开下一个界面（无缝切换）
        /// </summary>
        /// <param name="uiType">关闭的栈类型</param>
        /// <param name="uiName">UI名字</param>
        /// <param name="args">传递到0nStart的参数</param>
        public void PopAllThenOpen(UIType uiType, string uiName, params object[] args)
        {
            IUIContainer uiContainer = null;
            if (m_ShowDic.TryGetValue(uiType, out uiContainer))
                uiContainer?.PopAllThenOpen(uiName, args);
        }

        /// <summary>
        /// 关闭指定UI
        /// </summary>
        /// <param name="uiName">UI名字</param>
        public void Close(string uiName)
        {
            UIData uiData = null;
            m_UIRegisterDic.TryGetValue(uiName, out uiData);
            if (uiData == null)
                return;

            IUIContainer uiContainer = null;
            if (m_ShowDic.TryGetValue(uiData.uiType, out uiContainer))
            {
                if (uiContainer is UIStackContainer)
                {
                    //当前UI在栈顶才能被关闭
                    UIStackContainer uiStackContainer = uiContainer as UIStackContainer;
                    UI ui = uiStackContainer.Peek();
                    if (ui != null && ui.uiData.uiName == uiName)
                        uiContainer?.Pop(null);
                }
                else
                {
                    uiContainer?.Close(uiName, null);
                }
            }
        }

        /// <summary>
        /// 关闭UI后执行一个回调
        /// </summary>
        /// <param name="uiName">UI名字</param>
        /// <param name="callback">关闭回调</param>
        public void CloseWithCallback(string uiName, Action callback)
        {
            UIData uiData = null;
            m_UIRegisterDic.TryGetValue(uiName, out uiData);
            if (uiData == null)
                return;

            IUIContainer uiContainer = null;
            if (m_ShowDic.TryGetValue(uiData.uiType, out uiContainer))
            {
                if (uiContainer is UIStackContainer)
                {
                    //当前UI在栈顶才能被关闭
                    UIStackContainer uiStackContainer = uiContainer as UIStackContainer;
                    UI ui = uiStackContainer.Peek();
                    if (ui != null && ui.uiData.uiName == uiName)
                        uiContainer?.Pop(callback);
                }
                else
                {
                    uiContainer?.Close(uiName, callback);
                }
            }
        }

        /// <summary>
        /// 删除指定UI
        /// </summary>
        /// <param name="uiName">UI名字</param>
        public void Remove(string uiName)
        {
            UIData uiData = null;
            m_UIRegisterDic.TryGetValue(uiName, out uiData);
            if (uiData == null)
                return;

            IUIContainer uiContainer = null;
            if (m_ShowDic.TryGetValue(uiData.uiType, out uiContainer))
                uiContainer?.Remove(uiName);
        }

        /// <summary>
        /// 删除一个Ui，如果容器里面有多个，那么只会删除最上面那个
        /// </summary>
        /// <param name="uiName"></param>
        public void RemoveOne(string uiName)
        {
            UIData uiData = null;
            m_UIRegisterDic.TryGetValue(uiName, out uiData);
            if (uiData == null)
                return;

            IUIContainer uiContainer = null;
            if (m_ShowDic.TryGetValue(uiData.uiType, out uiContainer))
                uiContainer?.RemoveOne(uiName);
        }

        /// <summary>
        /// 获取在C#端对应的脚本Type
        /// </summary>
        /// <param name="uiName">UI名字</param>
        /// <returns></returns>
        public Type GetType(string uiName)
        {
            Type type = null;
            m_UITypeDic.TryGetValue(uiName, out type);
            return type;
        }

        /// <summary>
        /// 清除所有UI
        /// </summary>
        public void Clear()
        {
            closingAll = true;
            UIManager.instance.ClearMask();
            foreach (var kv in m_ShowDic)
                kv.Value.Clear();
            closingAll = false;
        }

        /// <summary>
        /// 清除指定类型容器的UI
        /// </summary>
        public void ClearByType(UIType uiType)
        {
            closingAll = true;
            UIManager.instance.ClearMask();
            foreach (var kv in m_ShowDic)
            {
                if ((kv.Key & uiType) != 0)
                    kv.Value.Clear();
            }
            closingAll = false;
        }

        //模拟异步加载资源
        void LoadAsset(UI ui)
        {
            if (ui.uiData.uiResType != UIResType.SetGameObject)
            {
                ui.uiState = UIStateType.Loading;
                GameObject orignal = Resources.Load<GameObject>(GetAssetUrl(ui.uiData.uiName));
                if (orignal.activeSelf)
                    orignal.SetActive(true);

                GameObject go = UnityEngine.Object.Instantiate(orignal) as GameObject;

                ui.SetGameObject(go);

                if (ui.tcs != null)
                    ui.tcs.SetResult(true);
            }
            else
            {
                //处理SetGameObject的子UI
                ChildUI childUi = ui as ChildUI;

                if (childUi != null && childUi.parentUI != null)
                {
                    ui.uiState = UIStateType.Loading;
                    GameObject childGameObject = childUi.parentUI.gameObject.FindGameObject(childUi.uiData.uiName);
                    if (childGameObject == null)
                    {
                        Debug.LogErrorFormat("父UI:{0}不存在子UI节点:{1}", childUi.uiData.parentUIName, childUi.uiData.uiName);
                    }
                    else
                    {
                        ui.SetGameObject(childGameObject);

                        if (ui.tcs != null)
                            ui.tcs.SetResult(true);
                    }
                }
            }
        }

        public string GetAssetUrl(string uiName)
        {
            return $"UI/{uiName}";
        }

        #region IUINotify

        List<IUINotify> nofifyList = new List<IUINotify>();

        public void AddUiNotify(IUINotify notify)
        {
            if (notify == null)
                return;
            if (!nofifyList.Contains(notify))
                nofifyList.Add(notify);
        }

        public void RemoveUiNotify(IUINotify notify)
        {
            if (notify == null)
                return;
            nofifyList.Remove(notify);
        }

        public void NotifyBeforeLoad(UI ui)
        {
            if (ui == null)
                return;
        }

        public void NotifyAfterLoad(UI ui)
        {
            if (ui == null)
                return;
        }

        public void NotifyBeforeAwake(UI ui)
        {
            if (ui == null)
                return;

            m_AllLoadList.Add(ui);
            EventManager.instance.Notify(EVENT_UI_AWAKE, ui);

            for (int i = 0; i < nofifyList.Count; i++)
            {
                IUINotify notify = nofifyList[i];
                notify?.OnAwake(ui);
            }
        }

        public void NotifyAfterAwake(UI ui)
        {
            if (ui == null)
                return;
        }

        public void NotifyBeforeStart(UI ui)
        {
            if (ui == null)
                return;
        }

        public void NotifyAfterStart(UI ui)
        {
            if (ui == null)
                return;

            if (!m_ShowList.Contains(ui))
            {
                m_ShowList.Add(ui);
            }

            EventManager.instance.Notify(EVENT_UI_START, ui);

            for (int i = 0; i < nofifyList.Count; i++)
            {
                IUINotify notify = nofifyList[i];
                notify?.OnStart(ui);
            }
        }

        public void NotifyBeforeEnable(UI ui)
        {
            if (ui == null)
                return;

        }

        public void NotifyAfterEnable(UI ui)
        {
            if (ui == null)
                return;

            EventManager.instance.Notify(EVENT_UI_ENABLE, ui);

            if (!m_ShowList.Contains(ui))
                m_ShowList.Add(ui);

            for (int i = 0; i < nofifyList.Count; i++)
            {
                IUINotify notify = nofifyList[i];
                notify?.OnEnable(ui);
            }
        }

        public void NotifyBeforeDisable(UI ui)
        {
            if (ui == null)
                return;

            EventManager.instance.Notify(EVENT_UI_DISABLE, ui);
            m_ShowList.Remove(ui);

            for (int i = 0; i < nofifyList.Count; i++)
            {
                IUINotify notify = nofifyList[i];
                notify?.OnDisable(ui);
            }
        }

        public void NotifyAfterDisable(UI ui)
        {
            if (ui == null)
                return;
        }

        public void NotifyBeforeDestroy(UI ui)
        {
            if (ui == null)
                return;

            m_AllLoadList.Remove(ui);
            EventManager.instance.Notify(EVENT_UI_DESTROY, ui);

            for (int i = 0; i < nofifyList.Count; i++)
            {
                IUINotify notify = nofifyList[i];
                notify?.OnDestroy(ui);
            }
        }

        public void NotifyAfterDestroy(UI ui)
        {
        }

        #endregion

        #region Release

        /// <summary>
        /// 释放ui资源
        /// </summary>
        /// <param name="ui"></param>
        public void RealseUi(UI ui)
        {
            if (ui == null)
            {
                return;
            }

            //没有加载、或者已释放直接返回
            if (ui.uiState == UIStateType.None || ui.uiState == UIStateType.Release)
            {
                return;
            }

            ui.uiState = UIStateType.Release;

            if (ui.uiData.uiResType == UIResType.Bundle)
            {
                if (ui.gameObject)
                {
                    UnityEngine.Object.Destroy(ui.gameObject);
                }
            }
        }

        #endregion
    }
}
