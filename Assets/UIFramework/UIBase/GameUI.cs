using System.Threading.Tasks;

namespace UIFramework
{
    public class GameUI : UI
    {
        public GameUI(UIData uiData) : base(uiData) { }

        //保存子UI
        private readonly UIChildContainer m_ChildUIContainer = new UIChildContainer();

        /// <summary>
        /// 等待所有动画播放完成
        /// </summary>
        public override async Task WaitAnimationFinished()
        {
            //等待自己动画播放完成
            await base.WaitAnimationFinished();
            //等待子UI动画播放完成
            await m_ChildUIContainer.WaitAnimationFinished();
        }

        /// <summary>
        /// 通过名字查找子UI
        /// </summary>
        /// <param name="childUiName">子UI名字</param>
        /// <returns>子UI</returns>
        public ChildUI FindChildUi(string childUiName)
        {
            return m_ChildUIContainer.FindChildUi(childUiName);
        }

        /// <summary>
        /// 添加子UI
        /// </summary>
        /// <param name="childUiName">子UI名字</param>
        /// <param name="childUi">子UI实例</param>
        public void AddChildUI(string childUiName, ChildUI childUi)
        {
            if (childUi == null)
                return;
            childUi.parentUI = this;
            m_ChildUIContainer.AddChildUI(childUiName, childUi);
        }

        //通过名字打开子UI
        public void OpenChildUI(string childUiName, params object[] args)
        {
            m_ChildUIContainer.Open(childUiName, null, args);
        }

        //只打开一个子UI，已显示的UI会被关闭
        public void OpenOneChildUi(string childUiName, params object[] args)
        {
            m_ChildUIContainer.CloseAllThenOpen(childUiName, args);
        }

        public void CloseChildUI(string childUiName)
        {
            m_ChildUIContainer.Close(childUiName, null);
        }

        public override void Awake()
        {
            base.Awake();
            //已加载的子UI也需要执行Awake
            m_ChildUIContainer.TryAwake();
        }

        public override void PlayChildEnableAnimation()
        {
            m_ChildUIContainer.PlayEnableAnimation();
        }

        public override void PlayChildDisableAnimation()
        {
            //有动画的子UI退场
            m_ChildUIContainer.PlayDisableAnimation();
        }

        public override void PlayChildDestroyAnimation()
        {
            m_ChildUIContainer.PlayDestroyAnimation();
        }

        public override void Destroy()
        {
            m_ChildUIContainer.Destroy();
            base.Destroy();
        }
    }
}
