using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UIFramework
{
    /// <summary>
    /// 子UI管理器
    /// </summary>
    public class UIChildContainer : IUIContainer
    {
        //保存已加载的子UI
        private readonly Dictionary<string, ChildUI> m_ChildDic = new Dictionary<string, ChildUI>();
        //保存已显示的子UI
        private readonly List<ChildUI> m_ShowList = new List<ChildUI>();

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

            UIManager.instance.SetMask(true);

            await UIManager.instance.LoadUIAsync(childUi);

            if (!m_ShowList.Contains(childUi))
                m_ShowList.Add(childUi);

            //设置子UI层级
            childUi.SetCavansOrder(childUi.parentUI.sortingOrder + 1);

            if (childUi.uiState == UIStateType.Awake)
                await childUi.StartAsync(args);
            else if (childUi.uiState == UIStateType.Start || childUi.uiState == UIStateType.Disable)
                await childUi.EnableAsync();

            UIManager.instance.SetMask(false);

            callback?.Invoke(childUi);
        }

        public void Close(string uiName, Action callback)
        {
            if (!IsShow(uiName))
                return;

            ChildUI childUI = null;
            if (!m_ChildDic.TryGetValue(uiName, out childUI))
                return;

            CloseAsync(uiName, callback);
        }

        private async void CloseAsync(string uiName, Action callback)
        {
            await WaitAnimationFinished();

            UIManager.instance.SetMask(true);

            ChildUI childUI = null;
            if (m_ChildDic.TryGetValue(uiName, out childUI))
            {
                await childUI.DisableAsync();
            }

            for (int i = 0; i < m_ShowList.Count; i++)
            {
                ChildUI tempChildUI = m_ShowList[i];
                if (tempChildUI != null && tempChildUI.uiData.uiName == uiName)
                {
                    m_ShowList.RemoveAt(i);
                    break;
                }
            }

            UIManager.instance.SetMask(false);

            callback?.Invoke();
        }

        //通过名字查找子UI
        public ChildUI FindChildUi(string childUiName)
        {
            ChildUI childUi = null;
            m_ChildDic.TryGetValue(childUiName, out childUi);
            return childUi;
        }

        //该子UI是否显示
        public bool IsShow(string childUiName)
        {
            for (int i = 0; i < m_ShowList.Count; i++)
            {
                ChildUI childUi = m_ShowList[i];
                if (childUi != null && childUi.uiData.uiName == childUiName)
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
            for (int i = m_ShowList.Count - 1; i >= 0; i--)
            {
                ChildUI showChild = m_ShowList[i];
                if (showChild != null && showChild.uiData.uiName == uiName)
                    m_ShowList.RemoveAt(i);
            }

            ChildUI childUI = null;
            m_ChildDic.TryGetValue(uiName, out childUI);
            childUI?.Destroy();
            UIManager.instance.RealseUi(childUI);
            m_ChildDic?.Remove(uiName);
        }

        /// <summary>
        /// 从上往下删除指定名字的一个一个UI
        /// </summary>
        /// <param name="uiName">要删除的UI</param>
        public void RemoveOne(string uiName)
        {
            for (int i = m_ShowList.Count - 1; i >= 0; i--)
            {
                ChildUI showChild = m_ShowList[i];
                if (showChild != null && showChild.uiData.uiName == uiName)
                    m_ShowList.RemoveAt(i);
            }

            ChildUI childUI = null;
            m_ChildDic.TryGetValue(uiName, out childUI);
            childUI?.Destroy();
            UIManager.instance.RealseUi(childUI);
            m_ChildDic?.Remove(uiName);
        }

        public void Destroy()
        {
            m_ShowList.Clear();
            if (m_ChildDic != null)
            {
                foreach (var kv in m_ChildDic)
                {
                    ChildUI childUi = kv.Value;
                    if (childUi != null)
                    {
                        childUi.Destroy();
                        UIManager.instance.RealseUi(childUi);
                    }
                }
                m_ChildDic.Clear();
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
            if (m_ChildDic != null)
            {
                foreach (var kv in m_ChildDic)
                {
                    if (kv.Value.uiData.loadWithParent && !kv.Value.awakeState)
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

            m_ChildDic.Add(uiName, childUI);
        }

        public async Task WaitAnimationFinished()
        {
            //等待子UI动画播放完成
            if (m_ChildDic != null)
            {
                foreach (var kv in m_ChildDic)
                    await kv.Value.WaitAnimationFinished();
            }
        }

        public void PlayEnableAnimation()
        {
            //父UI执行Enable之前，需要把显示列表的UI执行Enable
            for (int i = 0; i < m_ShowList.Count; i++)
            {
                ChildUI childUI = m_ShowList[i];
                if (childUI != null)
                    childUI.EnableAsync().ConfigureAwait(true);
            }
        }

        public void PlayDisableAnimation()
        {
            //父UI执行Disable之前，有动画的子UI需要播放退场动画
            for (int i = 0; i < m_ShowList.Count; i++)
            {
                ChildUI childUI = m_ShowList[i];
                if (childUI != null)
                    childUI.DisableAsync().ConfigureAwait(true);
            }
        }

        public void PlayDestroyAnimation()
        {
            for (int i = 0; i < m_ShowList.Count; i++)
            {
                ChildUI childUI = m_ShowList[i];
                if (childUI != null)
                    childUI.DestroyAsync().ConfigureAwait(true);
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

            UIManager.instance.SetMask(true);

            await UIManager.instance.LoadUIAsync(childUi);

            //要打开的窗口是否已经显示
            bool showFlag = false;
            for (int i = m_ShowList.Count - 1; i >= 0; i--)
            {
                ChildUI tempChildUi = m_ShowList[i];
                if (tempChildUi.uiData.uiName != childUi.uiData.uiName)
                    Close(tempChildUi.uiData.uiName, null);
                else
                    showFlag = true;
            }

            await WaitAnimationFinished();

            if (!showFlag)
                OpenAsync(childUi, null, args);

            UIManager.instance.SetMask(false);
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
    }
}
