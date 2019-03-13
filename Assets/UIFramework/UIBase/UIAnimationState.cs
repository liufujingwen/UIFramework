using UIFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIAnimationState : StateMachineBehaviour
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Singleton<UIManager>.Instance.NotifyAnimationFinish(animator);
    }

}
