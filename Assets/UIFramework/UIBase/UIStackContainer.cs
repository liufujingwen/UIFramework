using UnityEngine;
using System.Collections.Generic;
using System;

namespace UIFramework
{
    /// <summary>
    /// 管理显示的堆栈
    /// </summary>
    public class UIStackContainer : IUIContainer
    {
        //避免push过快
        private bool m_Pushing = false;
        //避免pop过快
        private bool m_Poping = false;
        //正在关闭全部（避免在关闭过程中有些UI写了打开某个ui在disable、destroy里面）
        private bool m_ClosingAll = false;
        //临时记录连续状态为ShowHistory的ui
        private static List<UI> ms_TempList = new List<UI>();

        public UIStackContainer(UIType uiType, int minOrder)
        {
            this.uiType = uiType;
            this.minOrder = minOrder;
        }

        public UIType uiType { get; set; }

        //保存当前入栈的UI
        private readonly CustomStack<UI> m_ShowStack = new CustomStack<UI>();

        /// <summary>
        /// 该UI显示栈最小的order，起始order
        /// </summary>
        public int minOrder { get; set; }

        /// <summary>
        /// 每个UI之间的order间隔
        /// </summary>
        private const int ORDER_PER_PANEL = 40;

        public UI Peek()
        {
            return m_ShowStack.Peek();
        }

        public void Open(string uiName, Action<UI> callback, params object[] args)
        {
            if (UIManager.instance.closingAll)
                return;

            UI ui = UIManager.instance.CreateUI(uiName);
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
            if (UIManager.instance.closingAll)
                return;

            if (m_ClosingAll)
                return;

            if (m_Pushing || m_Poping)
                return;

            m_Pushing = true;

            //保证播放动画期间不能操作
            if ((this.uiType & UIManager.IGNORE_MASK_TYPE) == 0)
                UIManager.instance.SetMask(true);

            //等待UI加载
            await UIManager.instance.LoadUIAsync(ui);

            //播放UI推场动画
            if (ui.uiData.uiType == UIType.NormalPopup)
            {
                UI curUi = m_ShowStack.Peek();
            }
            else
            {
                //播放UI退场动画
                if (m_ShowStack.Count != 0)
                {
                    List<UI> uiList = m_ShowStack.GetList();
                    for (int i = uiList.Count - 1; i >= 0; i--)
                    {
                        UI tempUi = uiList[i];
                        if (tempUi != null && tempUi.uiState == UIStateType.Enable)
                        {
                            await tempUi?.DisableAsync();
                        }
                    }
                }
            }

            m_ShowStack.Push(ui);
            //先设置UI层级
            int order = (m_ShowStack.Count - 1) * ORDER_PER_PANEL + minOrder;
            ui.SetCavansOrder(order);

            //播放UI入场动画
            await ui.StartAsync(args);

            //释放mask
            if ((this.uiType & UIManager.IGNORE_MASK_TYPE) == 0)
                UIManager.instance.SetMask(false);

            m_Pushing = false;

            callback?.Invoke(ui);
        }

        public void Close(string uiName, Action callback)
        {
            Debug.LogErrorFormat("UIType:{0}不能使用Close", this.uiType);
        }

        public void Pop(Action callback)
        {
            PopAsync(callback);
        }

        private async void PopAsync(Action callback)
        {
            if (m_Poping || m_Pushing)
                return;

            m_Poping = true;

            //保证播放动画期间不能操作
            if ((this.uiType & UIManager.IGNORE_MASK_TYPE) == 0)
                UIManager.instance.SetMask(true);

            //最上层UI退栈
            if (m_ShowStack.Count != 0)
            {
                //先peek,如果pop了就没有动画通知
                UI curUi = m_ShowStack.Peek();
                await curUi.DestroyAsync();
                m_ShowStack.Pop();
                curUi.Destroy();
                UIManager.instance.RealseUi(curUi);
            }

            m_Poping = false;

            //显示前一个界面
            if (m_ShowStack.Count != 0)
            {
                List<UI> uiList = m_ShowStack.GetList();
                UI preUi = m_ShowStack.Peek();
                if (preUi != null && preUi.uiData.uiType == UIType.NormalPopup && preUi.uiState == UIStateType.Disable)
                {
                    ms_TempList.Clear();

                    for (int i = uiList.Count - 2; i >= 0; i--)
                    {
                        UI tempUi = uiList[i];
                        if (tempUi == null)
                        {
                            break;
                        }

                        ms_TempList.Add(tempUi);

                        if (tempUi.uiData.uiType != UIType.NormalPopup)
                        {
                            break;
                        }
                    }

                    //直接显示连续为ShowHistory的ui
                    for (int i = ms_TempList.Count - 1; i >= 0; i--)
                    {
                        UI tempUi = ms_TempList[i];
                        tempUi?.Enable();
                    }
                }
                else
                {
                    await preUi?.EnableAsync();
                }
            }

            //释放Mask
            if ((this.uiType & UIManager.IGNORE_MASK_TYPE) == 0)
                UIManager.instance.SetMask(false);

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
            if (UIManager.instance.closingAll)
                return;

            if (m_ClosingAll)
                return;

            if (m_Pushing || m_Poping)
                return;

            m_Poping = true;

            UI newUi = UIManager.instance.CreateUI(uiName);
            if (newUi == null)
            {
                m_Poping = false;
                return;
            }

            //保证播放动画期间不能操作
            if ((this.uiType & UIManager.IGNORE_MASK_TYPE) == 0)
                UIManager.instance.SetMask(true);

            //等待加载新UI
            await UIManager.instance.LoadUIAsync(newUi);

            //最上层UI退栈
            if (m_ShowStack.Count != 0)
            {
                //先peek,如果pop了就没有动画通知
                UI currentUi = m_ShowStack.Peek();
                await currentUi.DestroyAsync();
                m_ShowStack.Pop();
                currentUi.Destroy();
                UIManager.instance.RealseUi(currentUi);
            }

            m_Poping = false;

            if ((this.uiType & UIManager.IGNORE_MASK_TYPE) == 0)
                UIManager.instance.SetMask(false);

            //打开新的UI
            UIManager.instance.Open(newUi, args);
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
            if (UIManager.instance.closingAll)
                return;

            if (m_ClosingAll)
                return;

            if (m_Pushing || m_Poping)
                return;

            m_Poping = true;

            UI newUi = UIManager.instance.CreateUI(uiName);
            if (newUi == null)
            {
                m_Poping = false;
                return;
            }

            //保证播放动画期间不能操作
            if ((this.uiType & UIManager.IGNORE_MASK_TYPE) == 0)
                UIManager.instance.SetMask(true);

            //先加载新UI
            await UIManager.instance.LoadUIAsync(newUi);

            //最上层UI退栈
            while (m_ShowStack.Count != 0)
            {
                //先peek,如果pop了就没有动画通知
                UI curUi = m_ShowStack.Peek();
                await curUi.DestroyAsync();
                m_ShowStack.Pop();
                curUi.Destroy();
                UIManager.instance.RealseUi(curUi);
            }

            m_Poping = false;

            if ((this.uiType & UIManager.IGNORE_MASK_TYPE) == 0)
                UIManager.instance.SetMask(false);

            //打开新UI
            UIManager.instance.Open(newUi, args);
        }

        /// <summary>
        /// 删除指定名字的UI
        /// </summary>
        /// <param name="uiName">UI名字</param>
        public void Remove(string uiName)
        {
            List<UI> uiList = m_ShowStack.GetList();
            for (int i = uiList.Count - 1; i >= 0; i--)
            {
                UI ui = uiList[i];
                if (ui.uiData.uiName == uiName)
                {
                    uiList.RemoveAt(i);
                    ui.Destroy();
                    UIManager.instance.RealseUi(ui);
                }
            }
        }


        /// <summary>
        /// 从上往下删除指定名字的一个一个UI
        /// </summary>
        /// <param name="uiName">要删除的UI</param>
        public void RemoveOne(string uiName)
        {
            List<UI> uiList = m_ShowStack.GetList();
            for (int i = uiList.Count - 1; i >= 0; i--)
            {
                UI ui = uiList[i];
                if (ui.uiData.uiName == uiName)
                {
                    uiList.RemoveAt(i);
                    ui.Destroy();
                    UIManager.instance.RealseUi(ui);
                    break;
                }
            }
        }

        //清除所有UI
        public void Clear()
        {
            m_ClosingAll = true;

            List<UI> uiList = m_ShowStack.GetList();
            if (uiList.Count > 0)
            {
                for (int i = uiList.Count - 1; i >= 0; i--)
                {
                    UI ui = uiList[i];
                    uiList.RemoveAt(i);
                    ui.Destroy();
                    UIManager.instance.RealseUi(ui);
                }
            }

            m_ClosingAll = false;

            m_ShowStack.Clear();
            m_Pushing = false;
            m_Poping = false;
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

            List<UI> uiList = m_ShowStack.GetList();
            for (int i = 0; i < uiList.Count; i++)
            {
                UI ui = uiList[i];
                if (ui != null && ui.transform)
                {
                    if (!ui.transform.IsChildOf(parent))
                        ui.transform.SetParent(parent, worldPositionStays);
                }
            }
        }
    }
}
