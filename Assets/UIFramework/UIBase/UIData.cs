using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIFramework
{
    public class UIData
    {
        /// <summary>
        /// UI名字
        /// </summary>
        public string UiName = null;

        /// <summary>
        /// UI类型
        /// </summary>
        public UIType UiType = UIType.Normal;

        /// <summary>
        /// UI加载方式
        /// </summary>
        public UIResType UIResType = UIResType.Resorces;

        /// <summary>
        /// UI是否有动画
        /// </summary>
        public bool HasAnimation = false;

        /// <summary>
        /// 是否在Lua处理逻辑
        /// </summary>
        public bool IsLuaUI = false;


        #region 子UI

        /// <summary>
        /// 是否有子UI
        /// </summary>
        public bool HasChildUI => ChildDic != null && ChildDic.Count > 0;

        //保存子UI信息
        public Dictionary<string, UIData> ChildDic = null;

        /// <summary>
        /// 是否子UI,有父窗口说明就是子UI
        /// </summary>
        public bool IsChildUI => !string.IsNullOrEmpty(ParentUIName);

        /// <summary>
        /// 父UI
        /// </summary>
        public string ParentUIName;

        /// <summary>
        /// 是否和父窗口一起加载
        /// </summary>
        public bool LoadWithParent = false;

        #endregion
    }
}
