using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIFramework
{
    public class UIContext
    {
        /// <summary>
        /// UI数据
        /// </summary>
        public UIData UIData = null;

        /// <summary>
        /// UI资源加载状态
        /// </summary>
        public TaskCompletionSource<bool> TCS = null;

        /// <summary>
        /// UI
        /// </summary>
        public UI UI = null;
    }
}
