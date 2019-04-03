using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UIFramework
{
    public class UIListContainer : IUIContainer
    {
        List<UI> uiList = new List<UI>();

        public UIListContainer(UIType uiType, int minOrder)
        {
            this.UIType = uiType;
            this.MinOrder = minOrder;
        }

        public UIType UIType;

        /// <summary>
        /// 该层级最小的order，起始order
        /// </summary>
        public int MinOrder = 0;

        /// <summary>
        /// 每个UI之间的order间隔
        /// </summary>
        private const int ORDER_PER_PANEL = 40;

        public void Open(string uiName, Action<UI> callback, params object[] args)
        {
            if (UIManager.Instance.ClosingAll)
                return;

            UI ui = FindUI(uiName);

            if (ui == null)
            {
                ui = UIManager.Instance.CreateUI(uiName);
                uiList.Add(ui);
            }

            if (ui == null)
                return;

            OpenAsync(ui, callback, args);
        }

        public void Open(UI ui, Action<UI> callback, params object[] args)
        {
            if (UIManager.Instance.ClosingAll)
                return;
            OpenAsync(ui, callback, args);
        }

        private async void OpenAsync(UI ui, Action<UI> callback, params object[] args)
        {
            //保证播放动画期间不能操作
            if ((this.UIType & UIManager.IgnoreMaskType) == 0)
                UIManager.Instance.SetMask(true);

            await UIManager.Instance.LoadUIAsync(ui);

            //先设置UI层级
            int order = (uiList.Count - 1) * ORDER_PER_PANEL + MinOrder;
            ui.SetCavansOrder(order);
            //播放UI入场动画
            await ui.StartAsync(args);

            //释放mask
            if ((this.UIType & UIManager.IgnoreMaskType) == 0)
                UIManager.Instance.SetMask(false);
        }


        public void Pop(Action actoin = null)
        {
            UnityEngine.Debug.LogErrorFormat("UIType:{0}不能使用Pop", this.UIType);
        }

        public void PopThenOpen(string uiName, params object[] args)
        {
            UnityEngine.Debug.LogErrorFormat("UIType:{0}不能使用PopThenOpen", this.UIType);
        }

        public void PopAllThenOpen(string uiName, params object[] args)
        {
            UnityEngine.Debug.LogErrorFormat("UIType:{0}不能使用PopAllThenOpen", this.UIType);
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
            if ((this.UIType & UIManager.IgnoreMaskType) == 0)
                UIManager.Instance.SetMask(true);

            //新播放退场动画
            await ui.DisableAsync();
            uiList.Remove(ui);
            ui.Destroy();

            //释放Mask
            if ((this.UIType & UIManager.IgnoreMaskType) == 0)
                UIManager.Instance.SetMask(false);

            callback?.Invoke();
        }

        public UI FindUI(string uiName)
        {
            for (int i = 0; i < uiList.Count; i++)
            {
                UI ui = uiList[i];
                if (ui != null && ui.UiData.UiName == uiName)
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
            for (int i = 0; i < uiList.Count; i++)
            {
                UI ui = uiList[i];
                if (ui != null && ui.UiData.UiName == uiName)
                {
                    uiList.RemoveAt(i);
                    ui.Destroy();
                }
            }
        }

        /// <summary>
        /// 清除所有UI
        /// </summary>
        public void Clear()
        {
            for (int i = uiList.Count - 1; i >= 0; i--)
            {
                UI ui = uiList[i];
                uiList.RemoveAt(i);
                ui.Destroy();
            }
            uiList.Clear();
        }

        /// <summary>
        /// 设置管理器所有UI父节点
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="worldPositionStays"></param>
        public void SetUiParent(Transform parent, bool worldPositionStays)
        {
            if (parent)
                return;

            for (int i = 0; i < uiList.Count; i++)
            {
                UI ui = uiList[i];
                if (ui != null && ui.Transform)
                {
                    if (!ui.Transform.IsChildOf(parent))
                        ui.Transform.SetParent(parent, worldPositionStays);
                }
            }
        }

        //通知动画播放完成
        public void OnNotifyAnimationFinish(Animator animator)
        {
            for (int i = 0; i < uiList.Count; i++)
            {
                UI ui = uiList[i];
                if (ui != null)
                    ui.NotifyAnimationState(animator);
            }
        }
    }
}
