using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using XLua;

namespace UIFramework
{
    public enum UIType
    {
        Normal = 1,
        Dialog = 2,
        Guide = 4,
        Tips = 8,
        TopMask = 16,
        Top = 32,
    }

    /// <summary>
    /// UI加载方式
    /// </summary>
    public enum UIResType
    {
        Resorces,//使用Resource.Load()
        Bundle,//使用AssetBundle
    }

    /// <summary>
    /// UI关闭方式
    /// </summary>
    public enum UICloseType
    {
        Hide,//隐藏不销毁，放回poolCanvas
        Destroy,//直接销毁
    }

    public class UIData
    {
        /// <summary>
        /// UI名字
        /// </summary>
        public string UIName = null;

        /// <summary>
        /// UI类型
        /// </summary>
        public UIType UIType = UIType.Normal;

        /// <summary>
        /// UI加载方式
        /// </summary>
        public UIResType UIResType = UIResType.Resorces;

        /// <summary>
        /// UI关闭方式
        /// </summary>
        public UICloseType UICloseType = UICloseType.Destroy;

        /// <summary>
        /// UI是否有动画
        /// </summary>
        public bool HasAnimation = false;

        /// <summary>
        /// 是否在Lua处理逻辑
        /// </summary>
        public bool IsLuaUI = false;

    }

    public class UIContex
    {
        /// <summary>
        /// UI数据
        /// </summary>
        public UIData UIData = null;

        /// <summary>
        /// UI资源加载状态
        /// </summary>
        public TaskCompletionSource<bool> TCS = null;

        /// <summary>
        /// UI
        /// </summary>
        public GameUI UI = null;
    }

    public class UIManager : Singleton<UIManager>
    {
        private Dictionary<string, UIData> uiRegisterDic = new Dictionary<string, UIData>();//所有UI必须注册后才能创建
        private List<UIContex> uiList = new List<UIContex>();//所有加载的UI
        private Dictionary<string, UIContex> poolDic = new Dictionary<string, UIContex>();//保存所有不销毁的UI
        private Dictionary<string, Type> uiTypeDic = new Dictionary<string, Type>();

        /// <summary>
        /// 显示堆栈，每个UIType对应一个栈
        /// </summary>
        private Dictionary<UIType, ShowStackManager> showDic = new Dictionary<UIType, ShowStackManager>();
        //UI相机
        private Camera uiCamera = null;
        //UIRoot
        private Transform uiRoot = null;
        //不销毁的UI存放位置
        private Transform poolCanvas = null;
        //保存UIType对应的界面父节点
        private Dictionary<UIType, Canvas> canvasDic = new Dictionary<UIType, Canvas>();

        public void Init()
        {
            InitTypes();
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
                Canvas tempCanvas = uiRoot.FindComponent<Canvas>($"{uiType}Canvas");
                canvasDic[uiType] = tempCanvas;
                showDic[uiType] = new ShowStackManager(uiType, tempCanvas.sortingOrder);
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
            //Debug.Log($"uiName:{uiName} uiType:{uiType} uiResType:{uiResType} uiCloseType:{uiCloseType} hasAnimation:{hasAnimation} isLuaUI:{isLuaUI}");
            UIData uiData = new UIData();
            uiData.UIName = uiName;
            uiData.UIType = uiType;
            uiData.UIResType = uiResType;
            uiData.UICloseType = uiCloseType;
            uiData.HasAnimation = hasAnimation;
            uiData.IsLuaUI = isLuaUI;
            uiRegisterDic.Add(uiName, uiData);
        }

        /// <summary>
        /// 加载UI
        /// </summary>
        /// <param name="uiName">ui名字</param>
        /// <returns></returns>
        public async Task LoadUIAsync(string uiName)
        {
            await LoadUI(uiName);
            GameUI tempGameUI = FindUI(uiName);
            if (tempGameUI != null)
            {
                UIManager.Instance.SetUIParent(tempGameUI.Transform, tempGameUI.UIContext.UIData.UIType);

                //保证所有UI只执行一次Init
                if (!tempGameUI.AwakeState)
                    tempGameUI.Awake();
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
                if (uiRegisterDic.TryGetValue(uiName, out uiData))
                {
                    tempUIContext = new UIContex();
                    tempUIContext.UIData = uiData;
                    tempUIContext.TCS = new TaskCompletionSource<bool>();
                    uiList.Add(tempUIContext);

                    Main.Instance.StartCoroutine(LoadAsset(GetAssetUrl(uiName), go =>
                    {
                        tempUIContext.UI = new GameUI();
                        tempUIContext.UI.SetContext(go, tempUIContext);

                        if (tempUIContext.TCS != null)
                            tempUIContext.TCS.SetResult(true);

                    }));

                    return tempUIContext.TCS.Task;
                }

                Debug.LogError($"{uiName}:不存在");
                return null;
            }
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
            if (!canvasDic.TryGetValue(uiType, out tempCanvas))
                return;

            if (transform.parent != tempCanvas.transform)
            {
                transform.SetParent(tempCanvas.transform, false);
                transform.transform.localPosition = Vector3.zero;
                transform.transform.localScale = Vector3.one;
                transform.transform.localRotation = Quaternion.identity;
            }
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

                    if (tempUIContext.UIData.UICloseType == UICloseType.Destroy)
                    {
                        tempUIContext.TCS = null;
                        if (tempUIContext.UI != null && tempUIContext.UI.UIState != GameUI.UIStateType.Destroy)
                            tempUIContext.UI.Destroy();
                    }
                    else
                    {
                        //UI不销毁，直接回池
                        if (tempUIContext.UI.Transform)
                            tempUIContext.UI.Transform.SetParent(poolCanvas, false);
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
        public GameUI FindUI(string uiName)
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
            GameUI tempGameUI = null;

            for (int i = 0, max = uiList.Count; i < max; i++)
            {
                UIContex tempUIContext = uiList[i];
                if (tempUIContext != null && tempUIContext.TCS != null && tempUIContext.TCS.Task.Result)
                {
                    if (tempUIContext.UI.GameObject == animator.gameObject)
                    {
                        tempGameUI = tempUIContext.UI;
                        break;
                    }
                }
            }

            if (tempGameUI != null)
                tempGameUI.OnNotifyAnimationState();
        }


        public void Push(string uiName, params object[] args)
        {
            UIData uiData = FindUIData(uiName);
            if (uiData == null)
            {
                Debug.LogError($"{uiName}未注册");
                return;
            }

            ShowStackManager showStack = null;
            if (showDic.TryGetValue(uiData.UIType, out showStack))
                showStack.Push(uiName, args);
        }

        /// <summary>
        /// 关闭Normal栈顶界面(对noraml出栈的会计方式，不需要传uitype)
        /// </summary>
        /// <param name="uiType">ui类型</param>
        public void Pop()
        {
            ShowStackManager tempShowStack = null;
            if (showDic.TryGetValue(UIType.Normal, out tempShowStack))
                tempShowStack.Pop();
        }

        /// <summary>
        /// 关闭栈顶界面
        /// </summary>
        /// <param name="uiType">ui类型</param>
        public void Pop(UIType uiType)
        {
            ShowStackManager tempShowStack = null;
            if (showDic.TryGetValue(uiType, out tempShowStack))
                tempShowStack.Pop();
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
            MaskManager.Instance.Clear();
            foreach (var kv in showDic)
                kv.Value.Clear();
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
