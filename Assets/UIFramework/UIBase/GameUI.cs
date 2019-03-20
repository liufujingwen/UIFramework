using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UIFramework
{
    public class GameUI : UI
    {
        //保存子UI
        private UIChildContainer childUIContainer = new UIChildContainer();

        /// <summary>
        /// 等待所有动画播放完成
        /// </summary>
        public override async Task WaitAnimationFinished()
        {
            //等待自己动画播放完成
            await base.WaitAnimationFinished();
            //等待子UI动画播放完成
            await childUIContainer.WaitAnimationFinished();
        }

        /// <summary>
        /// 添加子UI
        /// </summary>
        /// <param name="childUIName">子UI名字</param>
        /// <param name="childUI">子UI实例</param>
        public void AddChildUI(string childUIName, ChildUI childUI)
        {
            childUIContainer.AddChildUI(childUIName, childUI);
        }

        public void OpenChildUI(string childUIName, params object[] args)
        {
            childUIContainer.Open(childUIName, args);
        }

        public void CloseChildUI(string childUIName)
        {
            childUIContainer.Close(childUIName);
        }

        public override void Awake()
        {
            base.Awake();
            //已加载的子UI也需要执行Awake
            childUIContainer.TryAwake();
        }

        /// <summary>
        /// 记录在已显示列表的UI需要执行Enable
        /// </summary>
        public override void BeforeEnable()
        {
            base.BeforeEnable();
            childUIContainer.Enable();
        }

        /// <summary>
        /// 所有已显示的子UI播放退场动画
        /// </summary>

        public override void BeforeDisable()
        {
            base.BeforeDisable();
            //有动画的子UI退场
            childUIContainer.BeforeDisable();
        }

        public override void Disable()
        {
            //没有动画的子UI退场
            childUIContainer.Disable();
            base.Disable();
        }

        public override void BeforeDestroy()
        {
            base.BeforeDestroy();
            childUIContainer.Disable();
        }

        public override void Destroy()
        {
            childUIContainer.Clear();
            base.Destroy();
        }

        //UI回池
        public void InPool()
        {
            if (this.UIState > UIStateType.Awake)
                this.UIState = UIStateType.Awake;
            childUIContainer.InPool();
        }

    }
}
