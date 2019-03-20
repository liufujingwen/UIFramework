using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIFramework
{
    public class UIListContainer : IUIContainer
    {
        List<string> uiList = new List<string>();

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

        public void Open(string uiName, params object[] args)
        {
            if (UIManager.Instance.ClosingAll)
                return;
            OpenAsync(uiName, args).ConfigureAwait(true);
        }

        public async Task OpenAsync(string uiName, params object[] args)
        {
            await MaskManager.Instance.LoadMask();

            //保证播放动画期间不能操作
            if ((this.UIType & UIManager.IgnoreMaskType) == 0)
                MaskManager.Instance.SetActive(true);

            Task loadTask = UIManager.Instance.LoadUIAsync(uiName);
            {
                //容错处理，UI可能不存在
                if (loadTask == null)
                {
                    if ((this.UIType & UIManager.IgnoreMaskType) == 0)
                        MaskManager.Instance.SetActive(false);
                }
                await loadTask;
            }

            uiList.Add(uiName);
            GameUI newGameUI = UIManager.Instance.FindUI(uiName) as GameUI;
            if (newGameUI != null)
            {
                //先设置UI层级
                int order = (uiList.Count - 1) * ORDER_PER_PANEL + MinOrder;
                newGameUI.SetCavansOrder(order);
                //播放UI入场动画
                await newGameUI.StartAsync(args);
            }

            //释放mask
            if ((this.UIType & UIManager.IgnoreMaskType) == 0)
                MaskManager.Instance.SetActive(false);
        }


        public void Pop()
        {
            UnityEngine.Debug.LogErrorFormat("UIType:{0}不能使用Pop", this.UIType);
        }

        /// <summary>
        /// 关闭指定名字的UI
        /// </summary>
        /// <param name="uiName"></param>
        public void Close(string uiName)
        {
            CloseAsync(uiName).ConfigureAwait(true);
        }

        public async Task CloseAsync(string uiName)
        {
            if (!uiList.Contains(uiName))
                return;

            await MaskManager.Instance.LoadMask();

            //保证播放动画期间不能操作
            if ((this.UIType & UIManager.IgnoreMaskType) == 0)
                MaskManager.Instance.SetActive(true);

            uiList.Remove(uiName);

            GameUI closeUI = UIManager.Instance.FindUI(uiName) as GameUI;
            if (closeUI != null)
            {
                if (closeUI.UIContext.UIData.UICloseType == UICloseType.Destroy)
                    await closeUI.DestroyAsync();
                else
                    await closeUI.DisableAsync();

                UIManager.Instance.Remove(uiName);
            }

            //释放Mask
            if ((this.UIType & UIManager.IgnoreMaskType) == 0)
                MaskManager.Instance.SetActive(false);
        }

        /// <summary>
        /// 删除指定名字的UI
        /// </summary>
        /// <param name="uiName">UI名字</param>
        public void Remove(string uiName)
        {
            uiList.Remove(uiName);
        }

        /// <summary>
        /// 清除所有UI
        /// </summary>
        public void Clear()
        {
            if (uiList.Count > 0)
            {
                for (int i = 0; i < uiList.Count; i++)
                {
                    string tempUIName = uiList[i];
                    UIManager.Instance.Remove(tempUIName);
                }
            }
            uiList.Clear();
        }

    }
}
