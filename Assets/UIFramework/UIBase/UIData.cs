using System.Collections.Generic;

namespace UIFramework
{
    public class UIData
    {
        /// <summary>
        /// UI名字
        /// </summary>
        public string uiName { get; set; }

        /// <summary>
        /// UI类型
        /// </summary>
        public UIType uiType { get; set; } = UIType.Normal;

        /// <summary>
        /// UI加载方式
        /// </summary>
        public UIResType uiResType { get; set; } = UIResType.Resorces;

        /// <summary>
        /// UI是否有动画
        /// </summary>
        public bool hasAnimation { get; set; } = false;

        /// <summary>
        /// 是否在Lua处理逻辑
        /// </summary>
        public bool isLuaUI { get; set; } = false;


        #region 子UI

        /// <summary>
        /// 是否有子UI
        /// </summary>
        public bool hasChildUI => childDic != null && childDic.Count > 0;

        //保存子UI信息
        public Dictionary<string, UIData> childDic { get; set; }

        /// <summary>
        /// 是否子UI,有父窗口说明就是子UI
        /// </summary>
        public bool isChildUI => !string.IsNullOrEmpty(parentUIName);

        /// <summary>
        /// 父UI
        /// </summary>
        public string parentUIName { get; set; }

        /// <summary>
        /// 是否和父窗口一起加载
        /// </summary>
        public bool loadWithParent { get; set; }

        #endregion
    }
}
