using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UIFramework
{
    public class GameUI : UI
    {
        //保存子UI
        Dictionary<string, ChildUI> ChildDic = null;

        /// <summary>
        /// 等待所有动画播放完成
        /// </summary>
        public override async Task WaitAnimationFinished()
        {
            //等待自己动画播放完成
            await base.WaitAnimationFinished();

            //等待子UI动画播放完成
            if (ChildDic != null)
            {
                foreach (var kv in ChildDic)
                    await kv.Value.WaitAnimationFinished();
            }
        }

        /// <summary>
        /// 添加子窗口
        /// </summary>
        /// <param name="childUIName">子窗口名字</param>
        /// <param name="childUI">子窗口实例</param>
        public void AddChildUI(string childUIName, ChildUI childUI)
        {
            if (string.IsNullOrEmpty(childUIName) || childUI == null)
                return;

            if (ChildDic == null)
                ChildDic = new Dictionary<string, ChildUI>(5);

            if (!ChildDic.ContainsKey(childUIName))
                ChildDic[childUIName] = childUI;
        }

        public override void Awake()
        {
            base.Awake();

            //已加载的子UI也需要执行Awake
            if (this.UIContext.UIData.HasChildUI)
            {
                foreach (var kv in this.UIContext.UIData.ChildDic)
                {
                    ChildUI childUI = UIManager.Instance.FindUI(kv.Key) as ChildUI;
                    if (childUI != null)
                        childUI.Awake();
                }
            }
        }


        public override async Task StartAsync(params object[] args)
        {

            await base.StartAsync(args);
        }


        public override void Start(params object[] args)
        {
            base.Start(args);

            //if (ChildDic != null)
            //{
            //    foreach (var kv in ChildDic)
            //    {
            //        if (kv.Value != null && kv.Value.UIState == UIStateType.Awake)
            //            childUI.Awake();
            //    }
            //}

        }

        public override void Enable()
        {
            base.Enable();
        }

        public override void Disable()
        {
            base.Disable();
        }

        public override void Destroy()
        {

            base.Destroy();
        }

    }
}
