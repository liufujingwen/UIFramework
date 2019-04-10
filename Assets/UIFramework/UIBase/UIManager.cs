using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using XLua;

namespace UIFramework
{
    /// <summary>
    /// UI管理器
    /// </summary>
    public class UIManager : Singleton<UIManager>, IUINotify
    {
        //UI生命周期事件
        public const string EVENT_UI_AWAKE = "EVENT_UI_AWAKE";//UI执行Awake通知
        public const string EVENT_UI_START = "EVENT_UI_START";//UI执行Start通知
        public const string EVENT_UI_ENABLE = "EVENT_UI_ENABLE";//UI执行Enable通知
        public const string EVENT_UI_DISABLE = "EVENT_UI_DISABLE";//UI执行Disable通知
        public const string EVENT_UI_DESTROY = "EVENT_UI_DESTROY";//UI执行Destroy通知

        //所有一级UI必须注册后才能创建
        private Dictionary<string, UIData> uiRegisterDic = new Dictionary<string, UIData>();

        //C#的UI逻辑Type
        private Dictionary<string, Type> uiTypeDic = new Dictionary<string, Type>();

        //遮罩go，这个mask的作用是防止一切的点击
        private GameObject maskGo = null;
        //mask引用计数器
        private int maskCount = 0;

        /// <summary>
        /// 显示管理器
        /// </summary>
        private Dictionary<UIType, IUIContainer> showDic = new Dictionary<UIType, IUIContainer>();

        //HUD相机
        private Camera hudCamera = null;
        //UI相机
        private Camera uiCamera = null;
        //UIRoot
        private Transform uiRoot = null;
        //不销毁的UI存放位置
        private Transform poolCanvas = null;
        //保存UIType对应的界面父节点
        private Dictionary<UIType, Canvas> canvasDic = new Dictionary<UIType, Canvas>();

        /// <summary>
        /// 忽略Mask的类型
        /// </summary>
        public const UIType IgnoreMaskType = UIType.Tips | UIType.TopMask | UIType.Top;

        /// <summary>
        /// 堆栈管理的UI类型
        /// </summary>
        public const UIType StackType = UIType.Normal | UIType.Popup;

        /// <summary>
        /// 正在关闭全部（避免在关闭过程中有些UI写了打开某个ui在disable、destroy里面）
        /// </summary>
        public bool ClosingAll = false;

        /// <summary>
        /// 当前显示的UI,包含子UI
        /// </summary>
        List<UI> showList = new List<UI>();

        /// <summary>
        /// 已加载的所有UI
        /// </summary>
        List<UI> allList = new List<UI>();

        public void Init()
        {
            AddUiNotify(this);
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
            maskGo = GameObject.Instantiate(Resources.Load("UI/MaskUI")) as GameObject;
            Canvas topCanvas = canvasDic[UIType.Top];
            Canvas maskCanvas = maskGo.GetComponent<Canvas>();
            maskCanvas.sortingOrder = topCanvas.sortingOrder;
            maskGo.transform.SetParent(topCanvas.transform, false);
            maskGo.SetActive(false);
        }

        //控制mask的显隐
        public void SetMask(bool visible)
        {
            maskCount += visible ? 1 : -1;
            maskGo.SetActive(maskCount > 0);
        }

        //清除mask，计数器设置为0
        public void ClearMask()
        {
            maskCount = 0;
            maskGo.SetActive(maskCount > 0);
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
            Transform oldUIRoot = this.uiRoot;

            this.uiRoot = uiRoot;
            hudCamera = uiRoot.gameObject.FindComponent<Camera>("CameraHUD");
            uiCamera = uiRoot.gameObject.FindComponent<Camera>("Camera");
            poolCanvas = uiRoot.FindTransform("CanvasPool");

            foreach (UIType uiType in Enum.GetValues(typeof(UIType)))
            {
                if (uiType == UIType.Child)
                    continue;

                Canvas tempCanvas = uiRoot.gameObject.FindComponent<Canvas>($"Canvas{uiType}");
                canvasDic[uiType] = tempCanvas;

                if (!showDic.ContainsKey(uiType))
                {
                    if ((uiType & StackType) != 0)
                        showDic[uiType] = new UIStackContainer(uiType, tempCanvas.sortingOrder);
                    else
                        showDic[uiType] = new UIListContainer(uiType, tempCanvas.sortingOrder);
                }
            }

            //改变ui的父节点
            foreach (var kv in showDic)
                kv.Value.SetUiParent(canvasDic[kv.Key].transform, false);

            //改变mask的父节点
            if (maskGo != null)
            {
                Transform topCanvas = canvasDic[UIType.Top].transform;
                maskGo.transform.SetParent(topCanvas, false);
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
            uiTypeDic.Clear();
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
                        uiTypeDic[uiAttribute.UIName] = type;
                        Register(uiAttribute.UIName, uiAttribute.UIType, uiAttribute.UIResType, uiAttribute.HasAnimation, false);
                    }
                }

                object[] childAttrs = type.GetCustomAttributes(typeof(UIChildAttribute), false);
                foreach (object attr in childAttrs)
                {
                    UIChildAttribute childAttribute = (UIChildAttribute)attr;
                    if (childAttribute != null)
                    {
                        uiTypeDic[childAttribute.UIName] = type;
                        RegisterChild(childAttribute.UIName, childAttribute.ParentUIName, childAttribute.LoadWithParent, childAttribute.UIResType, childAttribute.HasAnimation, false);
                    }
                }
            }
        }

        /// <summary>
        /// 处理UI的父子关系，必须所有UI注册完后才能处理这个映射关系
        /// </summary>
        public void ProcessingUIDataMapping()
        {
            foreach (var kv in uiRegisterDic)
            {
                UIData uiData = null;
                if (!string.IsNullOrEmpty(kv.Value.ParentUIName))
                {
                    if (uiRegisterDic.TryGetValue(kv.Value.ParentUIName, out uiData))
                    {
                        if (uiData.ChildDic == null)
                            uiData.ChildDic = new Dictionary<string, UIData>(5);
                        uiData.ChildDic.Add(kv.Key, kv.Value);
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
            uiData.UiName = uiName;
            uiData.UiType = uiType;
            uiData.UIResType = uiResType;
            uiData.HasAnimation = hasAnimation;
            uiData.IsLuaUI = isLuaUI;
            uiData.ParentUIName = null;
            uiRegisterDic.Add(uiName, uiData);
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
            uiData.UiName = uiName;
            uiData.ParentUIName = parentUIName;
            uiData.LoadWithParent = loadWithParent;
            uiData.UiType = UIType.Child;
            uiData.UIResType = uiResType;
            uiData.HasAnimation = hasAnimation;
            uiData.IsLuaUI = isLuaUI;
            uiRegisterDic.Add(uiName, uiData);
        }

        public GameUI CreateUI(string uiName)
        {
            //加载新UI
            UIData uiData = null;
            uiRegisterDic.TryGetValue(uiName, out uiData);

            if (uiData == null)
            {
                Debug.LogError($"{uiName}:不存在");
                return null;
            }

            if (uiData.IsChildUI)
            {
                Debug.LogError($"子UI:{uiName}不能使用CreateUI创建");
                return null;
            }

            GameUI ui = new GameUI();
            ui.UiData = uiData;
            ui.Tcs = new TaskCompletionSource<bool>();

            //创建子UI
            if (ui.UiData.HasChildUI)
            {
                foreach (var kv in ui.UiData.ChildDic)
                {
                    ChildUI childUi = ui.FindChildUi(kv.Key);
                    if (childUi != null)
                        continue;

                    childUi = new ChildUI();
                    childUi.UiData = kv.Value;
                    childUi.Tcs = new TaskCompletionSource<bool>();
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
            if (!ui.AwakeState)
                ui.Awake();
        }

        /// <summary>
        /// 加载UI
        /// </summary>
        /// <param name="uiName">ui名字</param>
        /// <returns></returns>
        private Task LoadUiTask(UI ui)
        {
            if (ui.UIState == UIStateType.None)
                LoadAsset(ui);
            return ui.Tcs.Task;
        }

        /// <summary>
        /// 尝试加载LoadWithParent=true的子UI
        /// </summary>
        /// <returns></returns>
        public async Task TryLoadChildUI(UI ui)
        {
            if (ui.UiData.HasChildUI)
            {
                foreach (var kv in ui.UiData.ChildDic)
                {
                    if (kv.Value.LoadWithParent)
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

            if (!ui.Transform)
                return;

            Canvas tempCanvas = null;
            if (canvasDic.TryGetValue(ui.UiData.UiType, out tempCanvas))
            {
                if (!ui.Transform.IsChildOf(tempCanvas.transform))
                    ui.Transform.SetParent(tempCanvas.transform, worldPositionStays);
            }
            else if (ui.UiData.UiType == UIType.Child)
            {
                ChildUI childUi = ui as ChildUI;
                if (childUi.ParentUI != null && childUi.ParentUI.ChildParentNode)
                {
                    if (!ui.Transform.IsChildOf(childUi.ParentUI.ChildParentNode))
                        ui.Transform.SetParent(childUi.ParentUI.ChildParentNode, worldPositionStays);
                }
                else if (!ui.Transform.IsChildOf(childUi.ParentUI.Transform))
                {
                    ui.Transform.SetParent(childUi.ParentUI.Transform, worldPositionStays);
                }
            }

            if (worldPositionStays)
            {
                ui.Transform.localScale = Vector3.one;
                ui.Transform.localPosition = Vector3.zero;
                ui.Transform.localRotation = Quaternion.identity;
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
            if (canvasDic.TryGetValue(uiType, out tempCanvas))
            {
                if (transform.parent != tempCanvas.transform)
                    transform.SetParent(tempCanvas.transform, worldPositionStays);
            }
            else if (uiType == UIType.Child)
            {
                //子UI暂时放到poolCanvas
                if (transform.parent == null)
                    transform.SetParent(poolCanvas.transform, worldPositionStays);
            }
        }

        /// <summary>
        /// 通过名字查找UIData
        /// </summary>
        /// <returns></returns>
        private UIData FindUIData(string uiName)
        {
            UIData uiData = null;
            uiRegisterDic.TryGetValue(uiName, out uiData);
            return uiData;
        }

        /// <summary>
        /// 界面播放完动画通知
        /// </summary>
        /// <param name="animator"></param>
        public void NotifyAnimationFinish(Animator animator)
        {
            for (int i = 0; i < showList.Count; i++)
            {
                UI ui = showList[i];
                if (ui != null && ui.GameObject && ui.GameObject == animator.gameObject)
                    ui.OnNotifyAnimationState();
            }
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
            if (showDic.TryGetValue(uiData.UiType, out uiContainer))
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
            if (showDic.TryGetValue(ui.UiData.UiType, out uiContainer))
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
            if (showDic.TryGetValue(uiData.UiType, out uiContainer))
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
            if (showDic.TryGetValue(uiType, out uiContainer))
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
            if (showDic.TryGetValue(uiType, out uiContainer))
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
            if (showDic.TryGetValue(uiType, out uiContainer))
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
            if (showDic.TryGetValue(uiType, out uiContainer))
                uiContainer?.PopAllThenOpen(uiName, args);
        }

        /// <summary>
        /// 关闭指定UI
        /// </summary>
        /// <param name="uiName">UI名字</param>
        public void Close(string uiName)
        {
            UIData uiData = null;
            uiRegisterDic.TryGetValue(uiName, out uiData);
            if (uiData == null)
                return;

            IUIContainer uiContainer = null;
            if (showDic.TryGetValue(uiData.UiType, out uiContainer))
            {
                if (uiContainer is UIStackContainer)
                {
                    //当前UI在栈顶才能被关闭
                    UIStackContainer uiStackContainer = uiContainer as UIStackContainer;
                    UI ui = uiStackContainer.Peek();
                    if (ui != null && ui.UiData.UiName == uiName)
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
            uiRegisterDic.TryGetValue(uiName, out uiData);
            if (uiData == null)
                return;

            IUIContainer uiContainer = null;
            if (showDic.TryGetValue(uiData.UiType, out uiContainer))
            {
                if (uiContainer is UIStackContainer)
                {
                    //当前UI在栈顶才能被关闭
                    UIStackContainer uiStackContainer = uiContainer as UIStackContainer;
                    UI ui = uiStackContainer.Peek();
                    if (ui != null && ui.UiData.UiName == uiName)
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
            uiRegisterDic.TryGetValue(uiName, out uiData);
            if (uiData == null)
                return;

            IUIContainer uiContainer = null;
            if (showDic.TryGetValue(uiData.UiType, out uiContainer))
                uiContainer?.Remove(uiName);
        }

        /// <summary>
        /// 获取在C#端对应的脚本Type
        /// </summary>
        /// <param name="uiName">UI名字</param>
        /// <returns></returns>
        public Type GetType(string uiName)
        {
            Type type = null;
            uiTypeDic.TryGetValue(uiName, out type);
            return type;
        }

        /// <summary>
        /// 清除所有UI
        /// </summary>
        public void Clear()
        {
            ClosingAll = true;
            UIManager.Instance.ClearMask();
            foreach (var kv in showDic)
                kv.Value.Clear();
            ClosingAll = false;
        }

        //模拟异步加载资源
        void LoadAsset(UI ui)
        {
            if (ui.UiData.UIResType != UIResType.SetGameObject)
            {
                GameObject go = GameObject.Instantiate(Resources.Load<GameObject>(GetAssetUrl(ui.UiData.UiName))) as GameObject;

                ui.SetGameObject(go);

                if (ui.Tcs != null)
                    ui.Tcs.SetResult(true);
            }
            else
            {
                //处理SetGameObject的子UI
                ChildUI childUi = ui as ChildUI;

                if (childUi != null && childUi.ParentUI != null)
                {
                    GameObject childGameObject = childUi.ParentUI.GameObject.FindGameObject(childUi.UiData.UiName);
                    if (childGameObject == null)
                    {
                        Debug.LogErrorFormat("父UI:{0}不存在子UI节点:{1}", childUi.UiData.ParentUIName, childUi.UiData.UiName);
                    }
                    else
                    {
                        ui.SetGameObject(childGameObject);

                        if (ui.Tcs != null)
                            ui.Tcs.SetResult(true);
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

        public void NotifyAwake(UI ui)
        {
            for (int i = 0; i < nofifyList.Count; i++)
            {
                IUINotify notify = nofifyList[i];
                notify?.OnAwake(ui);
            }
        }

        public void NotifyStart(UI ui)
        {
            for (int i = 0; i < nofifyList.Count; i++)
            {
                IUINotify notify = nofifyList[i];
                notify?.OnStart(ui);
            }
        }

        public void NotifyEnable(UI ui)
        {
            for (int i = 0; i < nofifyList.Count; i++)
            {
                IUINotify notify = nofifyList[i];
                notify?.OnEnable(ui);
            }
        }

        public void NotifyDisable(UI ui)
        {
            for (int i = 0; i < nofifyList.Count; i++)
            {
                IUINotify notify = nofifyList[i];
                notify?.OnDisable(ui);
            }
        }

        public void NotifyDestroy(UI ui)
        {
            for (int i = 0; i < nofifyList.Count; i++)
            {
                IUINotify notify = nofifyList[i];
                notify?.OnDestroy(ui);
            }
        }

        public void OnAwake(UI ui)
        {
            if (ui == null)
                return;

            allList.Add(ui);
            GameEventManager.Instance.Notify(EVENT_UI_AWAKE, ui);
        }

        public void OnStart(UI ui)
        {
            if (ui == null)
                return;

            GameEventManager.Instance.Notify(EVENT_UI_START, ui);
        }

        public void OnEnable(UI ui)
        {
            if (ui == null)
                return;

            GameEventManager.Instance.Notify(EVENT_UI_ENABLE, ui);

            if (!showList.Contains(ui))
                showList.Add(ui);
        }

        public void OnDisable(UI ui)
        {
            if (ui == null)
                return;

            GameEventManager.Instance.Notify(EVENT_UI_DISABLE, ui);
            showList.Remove(ui);
        }

        public void OnDestroy(UI ui)
        {
            if (ui == null)
                return;

            allList.Remove(ui);
            GameEventManager.Instance.Notify(EVENT_UI_DESTROY, ui);
        }

        #endregion
    }
}
