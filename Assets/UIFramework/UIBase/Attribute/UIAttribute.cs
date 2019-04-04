using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            this.UIName = uiName;
            this.UIType = uiType;
            this.UIResType = uiResType;
            this.HasAnimation = hasAnimation;
        }

        /// <summary>
        /// UI名字
        /// </summary>
        public string UIName;

        /// <summary>
        /// UI类型
        /// </summary>
        public UIType UIType;

        /// <summary>
        /// UI加载方式
        /// </summary>
        public UIResType UIResType;

        /// <summary>
        /// UI是否有动画
        /// </summary>
        public bool HasAnimation = false;

    }
}
