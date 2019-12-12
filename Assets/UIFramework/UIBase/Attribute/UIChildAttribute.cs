using System;

namespace UIFramework
{
    public class UIChildAttribute : Attribute
    {
        /// <summary>
        /// 注册UI
        /// </summary>
        /// <param name="uiName">UI名字</param>
        /// <param name="parentUIName">父窗口UI名字</param>
        /// <param name="uiResType">UI加载方式</param>
        public UIChildAttribute(string uiName, string parentUIName, bool loadWithParent, UIResType uiResType, bool hasAnimation)
        {
            this.uiName = uiName;
            this.parentUIName = parentUIName;
            this.loadWithParent = loadWithParent;
            this.uiResType = uiResType;
            this.hasAnimation = hasAnimation;
        }

        /// <summary>
        /// UI名字
        /// </summary>
        public string uiName { get; set; }

        /// <summary>
        /// 父窗口UI名字
        /// </summary>
        public string parentUIName { get; set; }

        /// <summary>
        /// 是否和父窗口一起加载
        /// </summary>
        public bool loadWithParent { get; set; }

        /// <summary>
        /// UI加载方式
        /// </summary>
        public UIResType uiResType { get; set; }

        /// <summary>
        /// UI是否有动画
        /// </summary>
        public bool hasAnimation { get; set; }
    }
}
