using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            this.UIName = uiName;
            this.ParentUIName = parentUIName;
            this.LoadWithParent = loadWithParent;
            this.UIResType = uiResType;
            this.HasAnimation = hasAnimation;
        }

        /// <summary>
        /// UI名字
        /// </summary>
        public string UIName;

        /// <summary>
        /// 父窗口UI名字
        /// </summary>
        public string ParentUIName;

        /// <summary>
        /// 是否和父窗口一起加载
        /// </summary>
        public bool LoadWithParent;

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
