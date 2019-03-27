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

        public void Open(string uiName, Action<UI> callback, params object[] args)
        {
            OpenAsync(uiName, callback, args);
        }

        private async void OpenAsync(string uiName, Action<UI> callback, params object[] args)
        {
            if (UIManager.Instance.ClosingAll)
                return;

            if (closingAll)
                return;

            if (pushing || poping)
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

            callback?.Invoke(newGameUI);
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

            //保证播放动画期间不能操作
            if ((this.UIType & UIManager.IgnoreMaskType) == 0)
                MaskManager.Instance.SetActive(true);

            //先加载需要展示的UI
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

            poping = true;

            //最上层UI退栈
            if (showStack.Count != 0)
            {
                string curUiName = showStack.Pop();
                GameUI curUI = UIManager.Instance.FindUI(curUiName) as GameUI;
                if (curUI != null)
                {
                    bool contains = showStack.Contains(curUiName);

                    //栈底没有才能Destroy(循环栈),且不是打开的最新UI
                    if (contains || curUI.UIContext.UIData.UICloseType != UICloseType.Destroy || uiName == curUiName)
                        await curUI.DisableAsync();
                    else
                        await curUI.DestroyAsync();

                    //栈底没有才能移除UI(循环栈),且不是需要打开的UI
                    if (!contains && uiName != curUiName)
                        UIManager.Instance.Remove(curUiName);

                    //保证打开的UI能执行OnStart
                    if (uiName == curUiName)
                    {
                        if (curUI.UIState > UIStateType.Awake)
                            curUI.UIState = UIStateType.Awake;
                    }
                }
            }

            poping = false;

            if ((this.UIType & UIManager.IgnoreMaskType) == 0)
                MaskManager.Instance.SetActive(false);

            //打开新UI
            UIManager.Instance.Open(uiName, args);
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

            //保证播放动画期间不能操作
            if ((this.UIType & UIManager.IgnoreMaskType) == 0)
                MaskManager.Instance.SetActive(true);

            string topName = showStack.Peek();
            //需要打开的新UI刚好在栈顶
            //先加载需要展示的UI
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

            //最上层UI退栈
            if (showStack.Count != 0)
            {
                string curUiName = showStack.Pop();
                GameUI curUI = UIManager.Instance.FindUI(curUiName) as GameUI;
                if (curUI != null)
                {
                    //退栈UI如果等于新打开的UI,不能Destroy
                    if (curUI.UIContext.UIData.UICloseType != UICloseType.Destroy || uiName == curUiName)
                        await curUI.DisableAsync();
                    else
                        await curUI.DestroyAsync();

                    if (uiName == curUiName)
                    {
                        //确保新打开的UI还能执行OnStart
                        if (curUI.UIState > UIStateType.Awake)
                            curUI.UIState = UIStateType.Awake;
                    }
                }
            }

            //栈底可能存在新打开的UI，所以不能全不删除,直接调用Clear也许会清掉已经加载的新UI(循环栈)
            List<string> uiNameList = showStack.GetList();
            for (int i = uiNameList.Count - 1; i >= 0; i--)
            {
                string tempName = uiNameList[i];
                if (tempName != uiName)
                    UIManager.Instance.Remove(tempName);
            }
            showStack.Clear();

            poping = false;

            if ((this.UIType & UIManager.IgnoreMaskType) == 0)
                MaskManager.Instance.SetActive(false);

            //打开新UI
            UIManager.Instance.Open(uiName, args);
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
