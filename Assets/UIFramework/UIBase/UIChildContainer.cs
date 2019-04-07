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
            ChildUI childUi = FindChildUi(uiName);

            if (childUi == null)
                return;

            OpenAsync(childUi, callback, args);
        }

        public void Open(UI ui, Action<UI> callback, params object[] args)
        {
            OpenAsync(ui as ChildUI, callback, args);
        }

        private async void OpenAsync(ChildUI childUi, Action<UI> callback, params object[] args)
        {
            if (childUi == null)
                return;

            await WaitAnimationFinished();

            UIManager.Instance.SetMask(true);

            await UIManager.Instance.LoadUIAsync(childUi);

            if (!showList.Contains(childUi))
                showList.Add(childUi);

            //设置子UI层级
            childUi.SetCavansOrder(childUi.ParentUI.SortingOrder + 1);

            if (childUi.UIState == UIStateType.Awake)
                await childUi.StartAsync(args);
            else if (childUi.UIState == UIStateType.Start || childUi.UIState == UIStateType.Disable)
                await childUi.EnableAsync();

            UIManager.Instance.SetMask(false);

            callback?.Invoke(childUi);
        }

        public void Close(string uiName, Action callback)
        {
            if (!IsShow(uiName))
                return;

            ChildUI childUI = null;
            if (!childDic.TryGetValue(uiName, out childUI))
                return;

            CloseAsync(uiName, callback);
        }

        private async void CloseAsync(string uiName, Action callback)
        {
            await WaitAnimationFinished();

            UIManager.Instance.SetMask(true);

            ChildUI childUI = null;
            if (childDic.TryGetValue(uiName, out childUI))
            {
                await childUI.DisableAsync();
            }

            for (int i = 0; i < showList.Count; i++)
            {
                ChildUI tempChildUI = showList[i];
                if (tempChildUI != null && tempChildUI.UiData.UiName == uiName)
                {
                    showList.RemoveAt(i);
                    break;
                }
            }

            UIManager.Instance.SetMask(false);

            callback?.Invoke();
        }

        //通过名字查找子UI
        public ChildUI FindChildUi(string childUiName)
        {
            ChildUI childUi = null;
            childDic.TryGetValue(childUiName, out childUi);
            return childUi;
        }

        //该子UI是否显示
        public bool IsShow(string childUiName)
        {
            for (int i = 0; i < showList.Count; i++)
            {
                ChildUI childUi = showList[i];
                if (childUi != null && childUi.UiData.UiName == childUiName)
                    return true;
            }
            return false;
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
                if (showChild != null && showChild.UiData.UiName == uiName)
                {
                    childUI = showChild;
                    showList.RemoveAt(i);
                }
            }

            childUI?.Destroy();
            childDic?.Remove(uiName);
        }

        public void Destroy()
        {
            showList.Clear();
            if (childDic != null)
            {
                foreach (var kv in childDic)
                {
                    ChildUI childUi = kv.Value;
                    if (childUi != null)
                        childUi.Destroy();
                }
                childDic.Clear();
            }
        }

        //删除所有子UI
        public void Clear()
        {
            Destroy();
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
                    if (kv.Value.UiData.LoadWithParent && !kv.Value.AwakeState)
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
                if (childUI != null && childUI.UiData.HasAnimation)
                    childUI.DisableAsync().ConfigureAwait(true);
            }
        }

        public void Disable()
        {
            for (int i = 0; i < showList.Count; i++)
            {
                ChildUI childUI = showList[i];
                if (childUI != null && !childUI.UiData.HasAnimation)
                    childUI.DisableAsync().ConfigureAwait(true);
            }
        }

        public void CloseAllThenOpen(string uiName, params object[] args)
        {
            CloseAllThenOpenAsync(uiName, args);
        }

        private async void CloseAllThenOpenAsync(string uiName, params object[] args)
        {
            ChildUI childUi = FindChildUi(uiName);
            if (childUi == null)
                return;

            await WaitAnimationFinished();

            UIManager.Instance.SetMask(true);

            await UIManager.Instance.LoadUIAsync(childUi);

            //要打开的窗口是否已经显示
            bool showFlag = false;
            for (int i = showList.Count - 1; i >= 0; i--)
            {
                ChildUI tempChildUi = showList[i];
                if (tempChildUi.UiData.UiName != childUi.UiData.UiName)
                    Close(tempChildUi.UiData.UiName, null);
                else
                    showFlag = true;
            }

            await WaitAnimationFinished();

            if (!showFlag)
                OpenAsync(childUi, null, args);

            UIManager.Instance.SetMask(false);
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

        public void SetUiParent(UnityEngine.Transform parent, bool worldPositionStays)
        {
            UnityEngine.Debug.LogErrorFormat("Todo...");
        }

        //通知动画播放完成
        public void OnNotifyAnimationFinish(UnityEngine.Animator animator)
        {
            for (int i = 0; i < showList.Count; i++)
            {
                UI ui = showList[i];
                if (ui != null && ui.GameObject == animator.gameObject)
                    ui.OnNotifyAnimationState();
            }
        }
    }
}
