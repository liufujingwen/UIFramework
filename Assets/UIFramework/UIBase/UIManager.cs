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
    public class UIManager : Singleton<UIManager>
    {
        //所有UI必须注册后才能创建
        private Dictionary<string, UIData> uiRegisterDic = new Dictionary<string, UIData>();
        //保存子UI信息
        private Dictionary<string, UIData> uiChildRegisterDic = new Dictionary<string, UIData>();

        private List<UIContex> uiList = new List<UIContex>();//所有加载的UI
        private Dictionary<string, UIContex> poolDic = new Dictionary<string, UIContex>();//保存所有不销毁的UI
        private Dictionary<string, Type> uiTypeDic = new Dictionary<string, Type>();

        /// <summary>
        /// 显示管理器
        /// </summary>
        private Dictionary<UIType, IUIContainer> showDic = new Dictionary<UIType, IUIContainer>();

        //UI相机
        private Camera uiCamera = null;
        //UIRoot
        private Transform uiRoot = null;
        //不销毁的UI存放位置
        private Transform poolCanvas = null;
        //保存UIType对应的界面父节点
        private Dictionary<UIType, Canvas> canvasDic = new Dictionary<UIType, Canvas>();

        /// <summary>
        /// Push和Pop忽略Mask的类型
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


        public void Init()
        {
            InitTypes();
            ProcessingUIDataMapping();
            InitUIRoot(UIResType.Resorces);
        }

        public void InitUIRoot(UIResType uiResType)
        {
            //把所有UI先清空
            this.Clear();
            this.canvasDic.Clear();
            if (this.uiRoot)
                GameObject.Destroy(this.uiRoot.gameObject);

            GameObject uiRoot = null;

            if (uiResType == UIResType.Resorces)
                uiRoot = GameObject.Instantiate(Resources.Load("UI/UIRoot")) as GameObject;
            else
            {
                //从AssetBundle加载
                //uiRoot = GameObject.Instantiate(Resources.Load("UI/UIRoot")) as GameObject;
            }

            uiCamera = uiRoot.FindComponent<Camera>("Camera");
            poolCanvas = uiRoot.transform.FindTransform("PoolCanvas");
            foreach (UIType uiType in Enum.GetValues(typeof(UIType)))
            {
                if (uiType == UIType.Child)
                    continue;

                Canvas tempCanvas = uiRoot.FindComponent<Canvas>($"{uiType}Canvas");
                canvasDic[uiType] = tempCanvas;

                if ((uiType & StackType) != 0)
                    showDic[uiType] = new UIStackContainer(uiType, tempCanvas.sortingOrder);
                else
                    showDic[uiType] = new UIListContainer(uiType, tempCanvas.sortingOrder);
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
                        Register(uiAttribute.UIName, uiAttribute.UIType, uiAttribute.UIResType, uiAttribute.UICloseType, uiAttribute.HasAnimation, false);
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
            foreach (var kv in uiChildRegisterDic)
            {
                UIData uiData = null;
                if (uiRegisterDic.TryGetValue(kv.Value.ParentUIName, out uiData))
                {
                    if (uiData.ChildDic == null)
                        uiData.ChildDic = new Dictionary<string, UIData>(5);
                    uiData.ChildDic.Add(kv.Key, kv.Value);
                }
            }
        }

        /// <summary>
        /// 注册UI
        /// </summary>
        /// <param name="uiName">UI名字</param>
        /// <param name="uiType">UI类型</param>
        /// <param name="uiResType">UI加载方式</param>
        /// <param name="uiCloseType">UI关闭方式</param>
        /// <param name="hasAnimation">UI是否有动画</param>
        public void Register(string uiName, UIType uiType, UIResType uiResType, UICloseType uiCloseType, bool hasAnimation, bool isLuaUI)
        {
            UIData uiData = new UIData();
            uiData.UIName = uiName;
            uiData.UIType = uiType;
            uiData.UIResType = uiResType;
            uiData.UICloseType = uiCloseType;
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
            uiData.UIName = uiName;
            uiData.ParentUIName = parentUIName;
            uiData.LoadWithParent = loadWithParent;
            uiData.UIType = UIType.Child;
            uiData.UIResType = uiResType;
            uiData.HasAnimation = hasAnimation;
            uiData.IsLuaUI = isLuaUI;
            uiChildRegisterDic.Add(uiName, uiData);
        }

        /// <summary>
        /// 加载UI
        /// </summary>
        /// <param name="uiName">ui名字</param>
        /// <returns></returns>
        public async Task LoadUIAsync(string uiName)
        {
            await LoadUI(uiName);
            //尝试加载加载LoadWithParent=true的子UI
            await TryLoadChildUI(uiName);

            UI tempUI = FindUI(uiName);
            if (tempUI != null)
            {
                //保证从池中出来的父节点也是正确的
                SetUIParent(tempUI.Transform, tempUI.UIContext.UIData.UIType);
                //保证所有UI只执行一次Init
                if (!tempUI.AwakeState)
                    tempUI.Awake();
            }
        }

        /// <summary>
        /// 加载UI
        /// </summary>
        /// <param name="uiName">ui名字</param>
        /// <returns></returns>
        private Task LoadUI(string uiName)
        {
            UIContex tempUIContext = FindUIContext(uiName);

            if (tempUIContext != null)
            {
                //加载完成
                return tempUIContext.TCS.Task;
            }
            else
            {
                //从池中找
                if (poolDic.TryGetValue(uiName, out tempUIContext))
                {
                    poolDic.Remove(uiName);
                    uiList.Add(tempUIContext);
                    return tempUIContext.TCS.Task;
                }

                //加载新UI
                UIData uiData = null;
                uiRegisterDic.TryGetValue(uiName, out uiData);

                //是否子UI
                if (uiData == null)
                    uiChildRegisterDic.TryGetValue(uiName, out uiData);

                if (uiData != null)
                {
                    tempUIContext = new UIContex();
                    tempUIContext.UIData = uiData;
                    tempUIContext.TCS = new TaskCompletionSource<bool>();
                    uiList.Add(tempUIContext);

                    if (tempUIContext.UIData.UIResType != UIResType.SetGameObject)
                    {
                        Main.Instance.StartCoroutine(LoadAsset(GetAssetUrl(uiName), go =>
                        {
                            if (tempUIContext.UIData.IsChildUI)
                            {
                                GameUI parentUI = FindUI(tempUIContext.UIData.ParentUIName) as GameUI;
                                if (parentUI != null)
                                    tempUIContext.UI = new ChildUI(tempUIContext.UIData.UIName, parentUI);
                            }
                            else
                            {
                                tempUIContext.UI = new GameUI();
                            }

                            tempUIContext.UI.SetContext(go, tempUIContext);

                            if (tempUIContext.TCS != null)
                                tempUIContext.TCS.SetResult(true);
                        }));
                    }
                    else
                    {
                        //处理SetGameObject的子UI
                        GameUI parentUI = FindUI(tempUIContext.UIData.ParentUIName) as GameUI;
                        if (parentUI != null)
                        {
                            if (tempUIContext.UIData.IsChildUI)
                            {
                                tempUIContext.UI = new ChildUI(tempUIContext.UIData.UIName, parentUI);

                                if (parentUI != null)
                                {
                                    GameObject childGameObject = parentUI.ChildParentNode.FindGameObject(tempUIContext.UIData.UIName);
                                    if (childGameObject == null)
                                    {
                                        Debug.LogErrorFormat("父UI:{0}不存在子UI节点:{1}", tempUIContext.UIData.ParentUIName, tempUIContext.UIData.UIName);
                                    }
                                    else
                                    {
                                        tempUIContext.UI.SetContext(childGameObject, tempUIContext);
                                        tempUIContext.TCS.SetResult(true);
                                    }
                                }
                            }
                        }
                    }

                    return tempUIContext.TCS.Task;
                }

                Debug.LogError($"{uiName}:不存在");
                return null;
            }
        }

        /// <summary>
        /// 尝试加载LoadWithParent=true的子UI
        /// </summary>
        /// <returns></returns>
        public async Task TryLoadChildUI(string uiName)
        {
            UIData parentUIData = null;
            if (uiRegisterDic.TryGetValue(uiName, out parentUIData))
            {
                if (parentUIData.HasChildUI)
                {
                    foreach (var kv in parentUIData.ChildDic)
                    {
                        if (kv.Value.LoadWithParent)// && kv.Value.UIResType != UIResType.SetGameObject
                        {
                            await LoadChildUI(kv.Key);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 加载子UI
        /// </summary>
        /// <param name="childUIName"></param>
        /// <returns></returns>
        private Task LoadChildUI(string childUIName)
        {
            return LoadUI(childUIName);
        }

        /// <summary>
        /// 通过UIType设置界面的父节点
        /// </summary>
        /// <param name="transform">界面transform</param>
        /// <param name="uiType">ui类型</param>
        public void SetUIParent(Transform transform, UIType uiType)
        {
            if (!transform)
                return;

            Canvas tempCanvas = null;
            if (canvasDic.TryGetValue(uiType, out tempCanvas))
            {
                if (transform.parent != tempCanvas.transform)
                    transform.SetParent(tempCanvas.transform, false);
            }
            else if (uiType == UIType.Child)
            {
                //子UI暂时放到poolCanvas
                if (transform.parent == null)
                    transform.SetParent(poolCanvas.transform, false);
            }
            transform.transform.localPosition = Vector3.zero;
            transform.transform.localScale = Vector3.one;
            transform.transform.localRotation = Quaternion.identity;
        }

        /// <summary>
        /// 通过名字删除UI
        /// </summary>
        /// <param name="uiName">UI名字</param>
        public void RemoveUI(string uiName)
        {
            for (int i = 0, max = uiList.Count; i < max; i++)
            {
                UIContex tempUIContext = uiList[i];
                if (tempUIContext != null && tempUIContext.UIData.UIName == uiName)
                {
                    uiList.RemoveAt(i);

                    //子UI一律销毁
                    if (tempUIContext.UIData.UIType == UIType.Child || tempUIContext.UIData.UICloseType == UICloseType.Destroy)
                    {
                        tempUIContext.TCS = null;
                        if (tempUIContext.UI != null && tempUIContext.UI.UIState != UIStateType.Destroy)
                            tempUIContext.UI.Destroy();
                    }
                    else
                    {
                        //UI不销毁，直接回池
                        if (tempUIContext.UI.Transform)
                            tempUIContext.UI.Transform.SetParent(poolCanvas, false);

                        GameUI gameUI = tempUIContext.UI as GameUI;
                        gameUI?.InPool();
                        poolDic.Add(tempUIContext.UIData.UIName, tempUIContext);
                    }

                    break;
                }
            }
        }

        /// <summary>
        /// 通过名字查找UI
        /// </summary>
        /// <param name="uiName">UI名字</param>
        /// <returns></returns>
        public UI FindUI(string uiName)
        {
            UIContex tempUIContext = FindUIContext(uiName);
            if (tempUIContext != null)
                return tempUIContext.UI;
            return null;
        }


        /// <summary>
        /// 通过名字查找UIData
        /// </summary>
        /// <param name="uiName">UI名字</param>
        /// <returns></returns>
        private UIContex FindUIContext(string uiName)
        {
            for (int i = 0; i < uiList.Count; i++)
            {
                UIContex tempUIContext = uiList[i];
                if (tempUIContext != null && tempUIContext.UIData.UIName == uiName)
                    return tempUIContext;
            }
            return null;
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
            UI tempUI = null;

            for (int i = 0, max = uiList.Count; i < max; i++)
            {
                UIContex tempUIContext = uiList[i];
                if (tempUIContext != null && tempUIContext.TCS != null && tempUIContext.TCS.Task.Result)
                {
                    if (tempUIContext.UI.GameObject == animator.gameObject)
                    {
                        tempUI = tempUIContext.UI;
                        break;
                    }
                }
            }

            if (tempUI != null)
                tempUI.OnNotifyAnimationState();
        }


        public void Open(string uiName, params object[] args)
        {
            UIData uiData = FindUIData(uiName);
            if (uiData == null)
            {
                Debug.LogError($"{uiName}未注册");
                return;
            }

            IUIContainer uiContainer = null;
            if (showDic.TryGetValue(uiData.UIType, out uiContainer))
            {
                if (uiContainer is UIStackContainer)
                    (uiContainer as UIStackContainer).Open(uiName, args);
                else
                    (uiContainer as UIListContainer).Open(uiName, args);
            }
        }

        /// <summary>
        /// 关闭Normal栈顶界面(对noraml出栈的会计方式，不需要传uitype)
        /// </summary>
        /// <param name="uiType">ui类型</param>
        public void Pop()
        {
            IUIContainer uiContainer = null;
            if (showDic.TryGetValue(UIType.Normal, out uiContainer))
            {
                if (uiContainer is UIStackContainer)
                    (uiContainer as UIStackContainer).Pop();
            }
        }

        /// <summary>
        /// 关闭指定UI
        /// </summary>
        /// <param name="uiName">UI名字</param>
        public void Close(string uiName)
        {
            UIData uiData = FindUIData(uiName);
            if (uiData == null)
            {
                Debug.LogError($"Close的UI:{uiName}未注册");
                return;
            }

            IUIContainer uiContainer = null;
            if (showDic.TryGetValue(uiData.UIType, out uiContainer))
            {
                if (uiContainer is UIListContainer)
                    (uiContainer as UIListContainer).Close(uiName);
            }
        }

        /// <summary>
        /// 删除指定名字的UI
        /// </summary>
        /// <param name="uiName">UI名字</param>
        public void Remove(string uiName)
        {
            UIData uiData = FindUIData(uiName);
            if (uiData == null)
            {
                Debug.LogError($"Remove的UI:{uiName}未注册");
                return;
            }

            IUIContainer uiContainer = null;
            if (showDic.TryGetValue(uiData.UIType, out uiContainer))
            {
                uiContainer.Remove(uiName);
            }
        }

        /// <summary>
        /// 关闭栈顶界面
        /// </summary>
        /// <param name="uiType">ui类型</param>
        public void Pop(UIType uiType)
        {
            IUIContainer uiContainer = null;
            if (showDic.TryGetValue(uiType, out uiContainer))
            {
                if (uiContainer is UIStackContainer)
                    (uiContainer as UIStackContainer).Pop();
            }
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
            MaskManager.Instance.Close();
            foreach (var kv in showDic)
                kv.Value.Clear();
            ClosingAll = false;
        }

        //模拟异步加载资源
        IEnumerator LoadAsset(string url, Action<GameObject> loadHandle)
        {
            //随机延迟时间
            yield return UnityEngine.Random.Range(1, 10);
            GameObject go = GameObject.Instantiate(Resources.Load<GameObject>(url)) as GameObject;
            if (loadHandle != null)
                loadHandle(go);
        }

        public string GetAssetUrl(string uiName)
        {
            return $"UI/{uiName}";
        }
    }
}
