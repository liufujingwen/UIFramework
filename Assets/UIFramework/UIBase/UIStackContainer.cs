using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace UIFramework
{
    /// <summary>
    /// 管理显示的堆栈
    /// </summary>
    public class UIStackContainer : IUIContainer
    {
        //避免push过快
        bool pushing = false;
        //避免pop过快
        bool poping = false;
        //正在关闭全部（避免在关闭过程中有些UI写了打开某个ui在disable、destroy里面）
        bool closingAll = false;

        public UIStackContainer(UIType uiType, int minOrder)
        {
            this.UIType = uiType;
            this.MinOrder = minOrder;
        }

        public UIType UIType;

        //保存当前入栈的UI
        private CustomStack<UI> showStack = new CustomStack<UI>();

        /// <summary>
        /// 该UI显示栈最小的order，起始order
        /// </summary>
        public int MinOrder = 0;

        /// <summary>
        /// 每个UI之间的order间隔
        /// </summary>
        private const int ORDER_PER_PANEL = 40;

        public UI Peek()
        {
            return showStack.Peek();
        }

        public void Open(string uiName, Action<UI> callback, params object[] args)
        {
            if (UIManager.Instance.ClosingAll)
                return;

            UI ui = UIManager.Instance.CreateUI(uiName);
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
            if (UIManager.Instance.ClosingAll)
                return;

            if (closingAll)
                return;

            if (pushing || poping)
                return;

            pushing = true;

            //保证播放动画期间不能操作
            if ((this.UIType & UIManager.IgnoreMaskType) == 0)
                UIManager.Instance.SetMask(true);

            //等待UI加载
            await UIManager.Instance.LoadUIAsync(ui);

            //播放UI出场动画
            if (showStack.Count != 0)
            {
                UI curUi = showStack.Peek();
                await curUi?.DisableAsync();
            }

            showStack.Push(ui);
            //先设置UI层级
            int order = (showStack.Count - 1) * ORDER_PER_PANEL + MinOrder;
            ui.SetCavansOrder(order);

            //播放UI入场动画
            await ui.StartAsync(args);

            //释放mask
            if ((this.UIType & UIManager.IgnoreMaskType) == 0)
                UIManager.Instance.SetMask(false);

            pushing = false;

            callback?.Invoke(ui);
        }

        public void Close(string uiName, Action callback)
        {
            UnityEngine.Debug.LogErrorFormat("UIType:{0}不能使用Close", this.UIType);
        }

        public void Pop(Action callback)
        {
            PopAsync(callback);
        }

        private async void PopAsync(Action callback)
        {
            if (poping || pushing)
                return;

            poping = true;

            //保证播放动画期间不能操作
            if ((this.UIType & UIManager.IgnoreMaskType) == 0)
                UIManager.Instance.SetMask(true);

            //最上层UI退栈
            if (showStack.Count != 0)
            {
                //先peek,如果pop了就没有动画通知
                UI curUi = showStack.Peek();
                await curUi.DisableAsync();
                showStack.Pop();
                curUi.Destroy();
            }

            //显示前一个界面
            if (showStack.Count != 0)
            {
                UI preUi = showStack.Peek();
                await preUi.EnableAsync();
            }

            //释放Mask
            if ((this.UIType & UIManager.IgnoreMaskType) == 0)
                UIManager.Instance.SetMask(false);

            poping = false;

            callback?.Invoke();
        }

        /// <summary>
        /// 最上面UI先退栈，并打开指定名字的UI
        /// </summary>
        /// <param name="uiName">打开的UI名字</param>
        /// <param name="args">传递到0nStart的参数</param>
        public void PopThenOpen(string uiName, params object[] args)
        {
            PopThenOpenAsync(uiName, args);
        }

        /// <summary>
        /// 最上面UI先退栈，并打开指定名字的UI
        /// </summary>
        /// <param name="uiName">打开的UI名字</param>
        /// <param name="args">传递到0nStart的参数</param>
        private async void PopThenOpenAsync(string uiName, params object[] args)
        {
            if (UIManager.Instance.ClosingAll)
                return;

            if (closingAll)
                return;

            if (pushing || poping)
                return;

            poping = true;

            UI newUi = UIManager.Instance.CreateUI(uiName);
            if (newUi == null)
            {
                poping = false;
                return;
            }

            //保证播放动画期间不能操作
            if ((this.UIType & UIManager.IgnoreMaskType) == 0)
                UIManager.Instance.SetMask(true);

            //等待加载新UI
            await UIManager.Instance.LoadUIAsync(newUi);

            //最上层UI退栈
            if (showStack.Count != 0)
            {
                //先peek,如果pop了就没有动画通知
                UI currentUi = showStack.Peek();
                await currentUi.DisableAsync();
                showStack.Pop();
                currentUi.Destroy();
            }

            poping = false;

            if ((this.UIType & UIManager.IgnoreMaskType) == 0)
                UIManager.Instance.SetMask(false);

            //打开新的UI
            UIManager.Instance.Open(newUi, args);
        }

        /// <summary>
        /// 最上面UI先退栈，并打开指定名字的UI
        /// </summary>
        /// <param name="uiName">打开的UI名字</param>
        /// <param name="args">传递到0nStart的参数</param>
        public void PopAllThenOpen(string uiName, params object[] args)
        {
            PopAllThenOpenAsync(uiName, args);
        }

        /// <summary>
        /// 最上面UI先退栈，并打开指定名字的UI
        /// </summary>
        /// <param name="uiName">打开的UI名字</param>
        /// <param name="args">传递到0nStart的参数</param>
        private async void PopAllThenOpenAsync(string uiName, params object[] args)
        {
            if (UIManager.Instance.ClosingAll)
                return;

            if (closingAll)
                return;

            if (pushing || poping)
                return;

            poping = true;

            UI newUi = UIManager.Instance.CreateUI(uiName);
            if (newUi == null)
            {
                poping = false;
                return;
            }

            //保证播放动画期间不能操作
            if ((this.UIType & UIManager.IgnoreMaskType) == 0)
                UIManager.Instance.SetMask(true);

            //先加载新UI
            await UIManager.Instance.LoadUIAsync(newUi);

            //最上层UI退栈
            while (showStack.Count != 0)
            {
                //先peek,如果pop了就没有动画通知
                UI curUi = showStack.Peek();
                await curUi.DisableAsync();
                showStack.Pop();
                curUi.Destroy();
            }

            poping = false;

            if ((this.UIType & UIManager.IgnoreMaskType) == 0)
                UIManager.Instance.SetMask(false);

            //打开新UI
            UIManager.Instance.Open(newUi, args);
        }

        /// <summary>
        /// 删除指定名字的UI
        /// </summary>
        /// <param name="uiName">UI名字</param>
        public void Remove(string uiName)
        {
            List<UI> uiList = showStack.GetList();
            for (int i = uiList.Count - 1; i >= 0; i--)
            {
                UI ui = uiList[i];
                if (ui.UiData.UiName == uiName)
                {
                    uiList.RemoveAt(i);
                    ui.Destroy();
                }
            }
        }

        //清除所有UI
        public void Clear()
        {
            closingAll = true;

            List<UI> uiList = showStack.GetList();
            if (uiList.Count > 0)
            {
                for (int i = uiList.Count - 1; i >= 0; i--)
                {
                    UI ui = uiList[i];
                    uiList.RemoveAt(i);
                    ui.Destroy();
                }
            }

            closingAll = false;

            showStack.Clear();
            pushing = false;
            poping = false;
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

            List<UI> uiList = showStack.GetList();
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
            List<UI> uiList = showStack.GetList();
            for (int i = 0; i < uiList.Count; i++)
            {
                UI ui = uiList[i];
                if (ui != null)
                    ui.NotifyAnimationState(animator);
            }
        }
    }
}
