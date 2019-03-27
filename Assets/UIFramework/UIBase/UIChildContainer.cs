using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIFramework
{
    /// <summary>
    /// 子UI管理器
    /// </summary>
    public class UIChildContainer : IUIContainer
    {
        //保存已加载的子UI
        private Dictionary<string, ChildUI> childDic = new Dictionary<string, ChildUI>();
        //保存已显示的子UI
        List<ChildUI> showList = new List<ChildUI>();

        public void Open(string uiName, Action<UI> callback, params object[] args)
        {
            OpenAsync(uiName, callback, args);
        }

        private async void OpenAsync(string uiName, Action<UI> callback, params object[] args)
        {
            await WaitAnimationFinished();

            MaskManager.Instance.SetActive(true);

            Task loadTask = UIManager.Instance.LoadUIAsync(uiName);
            {
                //容错处理，UI可能不存在
                if (loadTask == null)
                    MaskManager.Instance.SetActive(false);
                await loadTask;
            }

            ChildUI childUI = null;

            if (childDic != null)
            {
                if (!childDic.TryGetValue(uiName, out childUI))
                {
                    MaskManager.Instance.SetActive(false);
                }
                else
                {
                    if (!showList.Contains(childUI))
                        showList.Add(childUI);

                    //设置子UI层级
                    childUI.SetCavansOrder(childUI.ParentUI.SortingOrder + 1);

                    if (childUI.UIState == UIStateType.Awake)
                        await childUI.StartAsync(args);
                    else if (childUI.UIState == UIStateType.Start || childUI.UIState == UIStateType.Disable)
                        await childUI.EnableAsync();
                }
            }

            MaskManager.Instance.SetActive(false);

            callback?.Invoke(childUI);
        }


        public void Close(string uiName, Action callback)
        {
            ChildUI childUI = null;
            if (!childDic.TryGetValue(uiName, out childUI))
                return;
            CloseAsync(uiName, callback);
        }

        private async void CloseAsync(string uiName, Action callback)
        {
            await WaitAnimationFinished();

            MaskManager.Instance.SetActive(true);

            for (int i = 0; i < showList.Count; i++)
            {
                ChildUI tempChildUI = showList[i];
                if (tempChildUI != null && tempChildUI.UIContext.UIData.UIName == uiName)
                {
                    showList.RemoveAt(i);
                    break;
                }
            }

            ChildUI childUI = null;
            if (childDic.TryGetValue(uiName, out childUI))
            {
                await childUI.DisableAsync();
            }

            MaskManager.Instance.SetActive(false);

            callback?.Invoke();
        }

        /// <summary>
        /// 移除子UI，不播放动画
        /// </summary>
        /// <param name="uiName"></param>
        public void Remove(string uiName)
        {
            ChildUI childUI = null;
            for (int i = showList.Count - 1; i >= 0; i--)
            {
                ChildUI showChild = showList[i];
                if (showChild != null && showChild.UIContext.UIData.UIName == uiName)
                {
                    childUI = showChild;
                    showList.RemoveAt(i);
                }
            }

            childDic?.Remove(uiName);
            UIManager.Instance.Remove(uiName);
        }

        //删除所有子UI
        public void Clear()
        {
            showList.Clear();
            if (childDic != null)
            {
                foreach (var kv in childDic)
                {
                    string uiName = kv.Value.UIContext.UIData.UIName;
                    UIManager.Instance.Remove(uiName);
                }
                childDic.Clear();
            }
        }

        /// <summary>
        /// 父UI加载完后尝试帮已加载的子UI执行Awake
        /// </summary>
        public void TryAwake()
        {
            if (childDic != null)
            {
                foreach (var kv in childDic)
                {
                    if (kv.Value.UIContext.UIData.IsChildUI)
                        kv.Value.Awake();
                }
            }
        }

        /// <summary>
        /// 添加子UI
        /// </summary>
        /// <param name="uiName">子UI名字</param>
        /// <param name="childUI">子UI实例</param>
        public void AddChildUI(string uiName, ChildUI childUI)
        {
            if (string.IsNullOrEmpty(uiName) || childUI == null)
                return;

            if (childDic == null)
                childDic = new Dictionary<string, ChildUI>();

            childDic.Add(uiName, childUI);
        }

        public async Task WaitAnimationFinished()
        {
            //等待子UI动画播放完成
            if (childDic != null)
            {
                foreach (var kv in childDic)
                    await kv.Value.WaitAnimationFinished();
            }
        }

        public void Enable()
        {
            //父UI执行Enable之前，需要把显示列表的UI执行Enable
            for (int i = 0; i < showList.Count; i++)
            {
                ChildUI childUI = showList[i];
                if (childUI != null)
                    childUI.EnableAsync().ConfigureAwait(true);
            }
        }


        public void BeforeDisable()
        {
            //父UI执行Disable之前，有动画的子UI需要播放退场动画
            for (int i = 0; i < showList.Count; i++)
            {
                ChildUI childUI = showList[i];
                if (childUI != null && childUI.UIContext.UIData.HasAnimation)
                    childUI.DisableAsync().ConfigureAwait(true);
            }
        }

        public void Disable()
        {
            for (int i = 0; i < showList.Count; i++)
            {
                ChildUI childUI = showList[i];
                if (childUI != null && !childUI.UIContext.UIData.HasAnimation)
                    childUI.DisableAsync().ConfigureAwait(true);
            }
        }

        /// <summary>
        /// 父UI回池，子UI重置状态
        /// </summary>
        public void InPool()
        {
            showList.Clear();
            foreach (var kv in childDic)
            {
                if (kv.Value.UIState > UIStateType.Awake)
                    kv.Value.UIState = UIStateType.Awake;
            }

        }

        public void Pop(Action callback)
        {
            UnityEngine.Debug.LogError("子UI管理器不能使用Pop");
        }

        public void PopThenOpen(string uiName, params object[] args)
        {
            UnityEngine.Debug.LogErrorFormat("子UI管理器不能使用PopThenOpen");
        }

        public void PopAllThenOpen(string uiName, params object[] args)
        {
            UnityEngine.Debug.LogErrorFormat("子UI管理器不能使用PopAllThenOpen");
        }
    }
}
