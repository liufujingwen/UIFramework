using System;

namespace UIFramework
{
    public class UIAttribute : Attribute
    {
        /// <summary>
        /// 注册UI
        /// </summary>
        /// <param name="uiName">UI名字</param>
        /// <param name="uiType">UI类型</param>
        /// <param name="uiResType">UI加载方式</param>
        public UIAttribute(string uiName, UIType uiType, UIResType uiResType, bool hasAnimation)
        {
            this.uiName = uiName;
            this.uiType = uiType;
            this.uiResType = uiResType;
            this.hasAnimation = hasAnimation;
        }

        /// <summary>
        /// UI名字
        /// </summary>
        public string uiName { get; set; }

        /// <summary>
        /// UI类型
        /// </summary>
        public UIType uiType { get; set; }

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
