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
        private CustomStack<string> showStack = new CustomStack<string>();

        /// <summary>
        /// 该UI显示栈最小的order，起始order
        /// </summary>
        public int MinOrder = 0;

        /// <summary>
        /// 每个UI之间的order间隔
        /// </summary>
        private const int ORDER_PER_PANEL = 40;

        public void Open(string uiName, params object[] args)
        {
            if (UIManager.Instance.ClosingAll)
                return;
            if (closingAll)
                return;
            OpenAsync(uiName, args).ConfigureAwait(true);
        }

        public async Task OpenAsync(string uiName, params object[] args)
        {
            if (pushing)
                return;

            pushing = true;

            await MaskManager.Instance.LoadMask();

            //保证播放动画期间不能操作
            if ((this.UIType & UIManager.IgnoreMaskType) == 0)
                MaskManager.Instance.SetActive(true);

            Task loadTask = UIManager.Instance.LoadUIAsync(uiName);
            {
                //容错处理，UI可能不存在
                if (loadTask == null)
                {
                    pushing = false;
                    if ((this.UIType & UIManager.IgnoreMaskType) == 0)
                        MaskManager.Instance.SetActive(false);
                }
                await loadTask;
            }

            //播放UI出场动画
            if (showStack.Count != 0)
            {
                string curUIName = showStack.Peek();
                GameUI curUI = UIManager.Instance.FindUI(curUIName) as GameUI;
                await curUI?.DisableAsync();
            }

            showStack.Push(uiName);
            GameUI newGameUI = UIManager.Instance.FindUI(uiName) as GameUI;
            if (newGameUI != null)
            {
                //先设置UI层级
                int order = (showStack.Count - 1) * ORDER_PER_PANEL + MinOrder;
                newGameUI.SetCavansOrder(order);

                bool contains = showStack.Contains(uiName);
                //栈底如果存在则直接修改状态，否者无法执行StartAsync里面的逻辑（循环栈）
                if (contains && newGameUI.UIState > UIStateType.Awake)
                    newGameUI.UIState = UIStateType.Awake;
                //播放UI入场动画
                await newGameUI.StartAsync(args);
            }

            //释放mask
            if ((this.UIType & UIManager.IgnoreMaskType) == 0)
                MaskManager.Instance.SetActive(false);

            pushing = false;
        }

        public void Close(string uiName)
        {
            UnityEngine.Debug.LogErrorFormat("UIType:{0}不能使用Close", this.UIType);
        }

        public void Pop()
        {
            PopAsync().ConfigureAwait(true);
        }

        public async Task PopAsync()
        {
            if (poping)
                return;

            poping = true;

            await MaskManager.Instance.LoadMask();

            //保证播放动画期间不能操作
            if ((this.UIType & UIManager.IgnoreMaskType) == 0)
                MaskManager.Instance.SetActive(true);

            //最上层UI退栈
            if (showStack.Count != 0)
            {
                string curUIName = showStack.Pop();
                GameUI curUI = UIManager.Instance.FindUI(curUIName) as GameUI;
                if (curUI != null)
                {
                    bool contains = showStack.Contains(curUIName);

                    //栈底没有才能Destroy(循环栈)
                    if (contains || curUI.UIContext.UIData.UICloseType != UICloseType.Destroy)
                        await curUI.DisableAsync();
                    else
                        await curUI.DestroyAsync();

                    //栈底没有才能移除UI(循环栈)
                    if (!contains)
                        UIManager.Instance.Remove(curUIName);
                }
            }

            //显示前一个界面
            if (showStack.Count != 0)
            {
                string preUIName = showStack.Peek();
                GameUI preUI = UIManager.Instance.FindUI(preUIName) as GameUI;
                if (preUI != null)
                {
                    await preUI.EnableAsync();
                }
            }

            //释放Mask
            if ((this.UIType & UIManager.IgnoreMaskType) == 0)
                MaskManager.Instance.SetActive(false);

            poping = false;
        }

        /// <summary>
        /// 删除指定名字的UI
        /// </summary>
        /// <param name="uiName">UI名字</param>
        public void Remove(string uiName)
        {
            showStack.Remove(uiName);
        }

        public void Clear()
        {
            closingAll = true;

            List<string> uiList = showStack.GetList();
            if (uiList.Count > 0)
            {
                for (int i = 0; i < uiList.Count; i++)
                {
                    string tempUIName = uiList[i];
                    UIManager.Instance.Remove(tempUIName);
                }
            }

            closingAll = false;

            showStack.Clear();
            pushing = false;
            poping = false;
        }
    }
}
