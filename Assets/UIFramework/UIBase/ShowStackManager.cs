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
    public class ShowStackManager
    {
        //避免push过快
        bool pushing = false;
        //避免pop过快
        bool poping = false;

        /// <summary>
        /// Push和Pop忽略Mask的类型
        /// </summary>
        public const UIType IgnoreMaskType = UIType.Tips | UIType.TopMask | UIType.Top;

        public ShowStackManager(UIType uiType, int minOrder)
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

        public void Push(string uiName, params object[] args)
        {
            PushAsync(uiName, args).ConfigureAwait(true);
        }

        public async Task PushAsync(string uiName, params object[] args)
        {
            if (pushing)
                return;

            pushing = true;

            await MaskManager.Instance.LoadMask();

            //保证播放动画期间不能操作
            if ((this.UIType & IgnoreMaskType) == 0)
                MaskManager.Instance.SetActive(true);

            Task loadTask = UIManager.Instance.LoadUIAsync(uiName);
            {
                //容错处理，UI可能不存在
                if (loadTask == null)
                {
                    pushing = false;
                    if ((this.UIType & IgnoreMaskType) == 0)
                        MaskManager.Instance.SetActive(false);
                }
                await loadTask;
            }

            //播放UI出场动画
            if (showStack.Count != 0)
            {
                string curUIName = showStack.Peek();
                GameUI curUI = UIManager.Instance.FindUI(curUIName);

                if (curUI != null && curUI.UIState > GameUI.UIStateType.None)
                    await curUI.PauseAsync();
            }

            showStack.Push(uiName);
            GameUI newGameUI = UIManager.Instance.FindUI(uiName);
            if (newGameUI != null)
            {
                //先设置UI层级
                int order = (showStack.Count - 1) * ORDER_PER_PANEL + MinOrder;
                newGameUI.SetCavansOrder(order);
                //播放UI入场动画
                await newGameUI.EnterAsync(args);
            }

            //释放mask
            if ((this.UIType & IgnoreMaskType) == 0)
                MaskManager.Instance.SetActive(false);

            pushing = false;
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
            if ((this.UIType & IgnoreMaskType) == 0)
                MaskManager.Instance.SetActive(true);

            //最上层UI退栈
            if (showStack.Count != 0)
            {
                string curUIName = showStack.Pop();
                GameUI curUI = UIManager.Instance.FindUI(curUIName);
                if (curUI != null)
                {
                    await curUI.ExitAsync();
                    UIManager.Instance.RemoveUI(curUIName);
                }
            }

            //显示前一个界面
            if (showStack.Count != 0)
            {
                string preUIName = showStack.Peek();
                GameUI preUI = UIManager.Instance.FindUI(preUIName);
                if (preUI != null)
                {
                    if (preUI.UIState > GameUI.UIStateType.None)
                        await preUI.ResumeAsync();
                }
            }

            //释放Mask
            if ((this.UIType & IgnoreMaskType) == 0)
                MaskManager.Instance.SetActive(false);

            poping = false;
        }

        //删除UI
        public void Remove(string uiName)
        {
            List<string> uiList = showStack.GetList();
            if (uiList.Count > 0)
            {
                for (int i = 0; i < uiList.Count; i++)
                {
                    string tempUIName = uiList[i];
                    if (uiName == tempUIName)
                        UIManager.Instance.RemoveUI(uiName);
                }
            }
            uiList.Remove(uiName);
        }

        public void Clear()
        {
            List<string> uiList = showStack.GetList();
            if (uiList.Count > 0)
            {
                for (int i = 0; i < uiList.Count; i++)
                {
                    string tempUIName = uiList[i];
                    UIManager.Instance.RemoveUI(tempUIName);
                }
            }

            uiList.Clear();
            pushing = false;
            poping = false;
        }
    }
}
