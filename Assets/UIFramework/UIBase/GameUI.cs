using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

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
        /// 通过名字查找子UI
        /// </summary>
        /// <param name="childUiName">子UI名字</param>
        /// <returns>子UI</returns>
        public ChildUI FindChildUi(string childUiName)
        {
            return childUIContainer.FindChildUi(childUiName);
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
            childUi.ParentUI = this;
            childUIContainer.AddChildUI(childUiName, childUi);
        }

        //通过名字打开子UI
        public void OpenChildUI(string childUiName, params object[] args)
        {
            ChildUI childUi = childUIContainer.FindChildUi(childUiName);
            if (childUi == null)
            {
                childUi = UIManager.Instance.CreateUI(childUiName) as ChildUI;
                this.AddChildUI(childUiName, childUi);
            }

            childUIContainer.Open(childUi, null, args);
        }

        //只打开一个子UI，已显示的UI会被关闭
        public void OpenOneChildUi(string childUiName, params object[] args)
        {
            childUIContainer.CloseAllThenOpen(childUiName, args);
        }

        public void CloseChildUI(string childUiName)
        {
            childUIContainer.Close(childUiName, null);
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

        public override void Destroy()
        {
            childUIContainer.Destroy();
            base.Destroy();
        }

        //ui动画播放完成通知
        public override void NotifyAnimationState(Animator animator)
        {
            if (this.GameObject == animator.gameObject)
                this.OnNotifyAnimationState();
            else
                childUIContainer.OnNotifyAnimationFinish(animator);
        }
    }
}
