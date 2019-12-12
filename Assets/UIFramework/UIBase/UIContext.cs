using System.Threading.Tasks;

namespace UIFramework
{
    public class UIContext
    {
        /// <summary>
        /// UI数据
        /// </summary>
        public UIData uiData { get; set; }

        /// <summary>
        /// UI资源加载状态
        /// </summary>
        public TaskCompletionSource<bool> tcs = null;

        /// <summary>
        /// UI
        /// </summary>
        public UI ui = null;

        /// <summary>
        /// 销毁UI
        /// </summary>
        public void Destroy()
        {
            tcs = null;
            ui?.Destroy();
        }

    }
}
