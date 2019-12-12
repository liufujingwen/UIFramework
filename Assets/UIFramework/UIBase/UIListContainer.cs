using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIFramework
{
    public class UIListContainer : IUIContainer
    {
        private readonly List<UI> m_UIList = new List<UI>();

        public UIListContainer(UIType uiType, int minOrder)
        {
            this.uiType = uiType;
            this.minOrder = minOrder;
        }

        public UIType uiType { get; private set; }

        /// <summary>
        /// 该层级最小的order，起始order
        /// </summary>
        public int minOrder { get; set; }

        /// <summary>
        /// 每个UI之间的order间隔
        /// </summary>
        private const int ORDER_PER_PANEL = 40;

        public void Open(string uiName, Action<UI> callback, params object[] args)
        {
            if (UIManager.instance.closingAll)
                return;

            UI ui = FindUI(uiName);

            if (ui == null)
            {
                ui = UIManager.instance.CreateUI(uiName);
                m_UIList.Add(ui);
            }

            if (ui == null)
                return;

            OpenAsync(ui, callback, args);
        }

        public void Open(UI ui, Action<UI> callback, params object[] args)
        {
            if (UIManager.instance.closingAll)
                return;
            OpenAsync(ui, callback, args);
        }

        private async void OpenAsync(UI ui, Action<UI> callback, params object[] args)
        {
            //保证播放动画期间不能操作
            if ((this.uiType & UIManager.IGNORE_MASK_TYPE) == 0)
                UIManager.instance.SetMask(true);

            await UIManager.instance.LoadUIAsync(ui);

            //先设置UI层级
            int order = (m_UIList.Count - 1) * ORDER_PER_PANEL + minOrder;
            ui.SetCavansOrder(order);
            //播放UI入场动画
            await ui.StartAsync(args);

            //释放mask
            if ((this.uiType & UIManager.IGNORE_MASK_TYPE) == 0)
                UIManager.instance.SetMask(false);

            callback?.Invoke(ui);
        }


        public void Pop(Action actoin = null)
        {
            UnityEngine.Debug.LogErrorFormat("UIType:{0}不能使用Pop", this.uiType);
        }

        public void PopThenOpen(string uiName, params object[] args)
        {
            UnityEngine.Debug.LogErrorFormat("UIType:{0}不能使用PopThenOpen", this.uiType);
        }

        public void PopAllThenOpen(string uiName, params object[] args)
        {
            UnityEngine.Debug.LogErrorFormat("UIType:{0}不能使用PopAllThenOpen", this.uiType);
        }

        /// <summary>
        /// 关闭指定名字的UI
        /// </summary>
        /// <param name="uiName"></param>
        public void Close(string uiName, Action callback)
        {
            CloseAsync(uiName, callback);
        }

        private async void CloseAsync(string uiName, Action callback)
        {
            UI ui = FindUI(uiName);
            if (ui == null)
                return;

            //保证播放动画期间不能操作
            if ((this.uiType & UIManager.IGNORE_MASK_TYPE) == 0)
                UIManager.instance.SetMask(true);

            //新播放退场动画
            await ui.DestroyAsync();
            m_UIList.Remove(ui);
            ui.Destroy();
            UIManager.instance.RealseUi(ui);

            //释放Mask
            if ((this.uiType & UIManager.IGNORE_MASK_TYPE) == 0)
                UIManager.instance.SetMask(false);

            callback?.Invoke();
        }

        /// <summary>
        /// 通过名字查找UI
        /// </summary>
        /// <param name="uiName">查找的UI名字</param>
        /// <returns></returns>
        public UI FindUI(string uiName)
        {
            for (int i = 0; i < m_UIList.Count; i++)
            {
                UI ui = m_UIList[i];
                if (ui != null && ui.uiData.uiName == uiName)
                    return ui;
            }

            return null;
        }

        /// <summary>
        /// 删除指定名字的UI
        /// </summary>
        /// <param name="uiName">UI名字</param>
        public void Remove(string uiName)
        {
            for (int i = 0; i < m_UIList.Count; i++)
            {
                UI ui = m_UIList[i];
                if (ui != null && ui.uiData.uiName == uiName)
                {
                    m_UIList.RemoveAt(i);
                    ui.Destroy();
                    UIManager.instance.RealseUi(ui);
                    break;
                }
            }
        }

        /// <summary>
        /// 从上往下删除指定名字的一个一个UI
        /// </summary>
        /// <param name="uiName">要删除的UI</param>
        public void RemoveOne(string uiName)
        {
            for (int i = 0; i < m_UIList.Count; i++)
            {
                UI ui = m_UIList[i];
                if (ui != null && ui.uiData.uiName == uiName)
                {
                    m_UIList.RemoveAt(i);
                    ui.Destroy();
                    UIManager.instance.RealseUi(ui);
                    break;
                }
            }
        }

        /// <summary>
        /// 清除所有UI
        /// </summary>
        public void Clear()
        {
            for (int i = m_UIList.Count - 1; i >= 0; i--)
            {
                UI ui = m_UIList[i];
                m_UIList.RemoveAt(i);
                ui.Destroy();
                UIManager.instance.RealseUi(ui);
            }
            m_UIList.Clear();
        }

        /// <summary>
        /// 设置管理器所有UI父节点
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="worldPositionStays"></param>
        public void SetUiParent(Transform parent, bool worldPositionStays)
        {
            if (!parent)
                return;

            for (int i = 0; i < m_UIList.Count; i++)
            {
                UI ui = m_UIList[i];
                if (ui != null && ui.transform)
                {
                    if (!ui.transform.IsChildOf(parent))
                        ui.transform.SetParent(parent, worldPositionStays);
                }
            }
        }
    }
}
