using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

[RequireComponent(typeof(Animator))]
public class UIPlayAnimatorAnimation : MonoBehaviour
{
    public string animName;
    public AnimationClip[] clips;
    private Animator m_Animator;
    private bool m_IsPlaying;
    private Action m_FinishedCallback;//动画播放完成回调
    private float m_ElapsedTime;//动画经过的时间
    private float m_Length;//动画时长

    private PlayableGraph m_PlayableGraph;
    private AnimationPlayableOutput m_AnimationPlayableOutput;
    private AnimationClipPlayable m_CurrentPlayable;

    private readonly Dictionary<string, AnimationClipPlayable> m_PlayableDic = new Dictionary<string, AnimationClipPlayable>();
    private static readonly Dictionary<int, UIPlayAnimatorAnimation> m_AnimatorAnimDic = new Dictionary<int, UIPlayAnimatorAnimation>();

    private void OnDisable()
    {
        Stop();
    }

    private void Update()
    {
        if (!m_IsPlaying)
        {
            return;
        }

        if (!m_Animator)
        {
            return;
        }

        m_ElapsedTime += Time.deltaTime;

        if (m_ElapsedTime >= m_Length)
        {
            m_CurrentPlayable.SetTime(m_Length);
            OnFinished();
        }
        else
        {
            m_CurrentPlayable.SetTime(m_ElapsedTime);
        }
    }

    //动画播放完成
    private void OnFinished()
    {
        m_IsPlaying = false;
        m_ElapsedTime = 0;
        m_PlayableGraph.Stop();
        Action tempHandle = m_FinishedCallback;
        m_FinishedCallback = null;
        tempHandle?.Invoke();
    }

    //播放动画
    public void Play(string animName, Action finishedCallback)
    {
        this.animName = animName;
        m_FinishedCallback = finishedCallback;

        if (!gameObject.activeInHierarchy)
        {
            Debug.LogError($"UIPlayAnimatorAnimation.Play Error:[{gameObject.name}:隐藏的gameObject不能播放动画]");
            return;
        }

        if (m_Animator == null)
        {
            m_Animator = gameObject.GetComponent<Animator>();
        }

        if (!m_Animator)
        {
            Debug.LogError($"UIPlayAnimatorAnimation.Play Error:[{gameObject.name}:不存在Animation组件]");
            return;
        }

        if (!m_PlayableGraph.IsValid())
        {
            m_PlayableGraph = PlayableGraph.Create();
            m_AnimationPlayableOutput = AnimationPlayableOutput.Create(m_PlayableGraph, "Animation", m_Animator);
        }

        AnimationClipPlayable playable;
        if (!m_PlayableDic.TryGetValue(animName, out playable))
        {
            bool Exist = false;

            for (int i = 0; i < clips.Length; i++)
            {
                AnimationClip tempClip = clips[i];
                if (tempClip != null && tempClip.name == animName)
                {
                    m_CurrentPlayable = AnimationClipPlayable.Create(m_PlayableGraph, tempClip);
                    m_PlayableDic[tempClip.name] = playable;
                    Exist = true;
                    break;
                }
            }

            if (!Exist)
            {
                Debug.LogError($"UIPlayAnimatorAnimation.Play Error:[{gameObject.name}:不存在Clip:{animName}]");
                return;
            }
        }

        AnimationClip clip = m_CurrentPlayable.GetAnimationClip();
        if (!clip)
        {
            Debug.LogError($"UIPlayAnimatorAnimation.Play Error:[{gameObject.name}:不存在Clip:{animName}]");
            return;
        }

        m_IsPlaying = true;
        m_ElapsedTime = 0;
        m_Length = clip.length;
        m_AnimationPlayableOutput.SetSourcePlayable(playable);
        m_PlayableGraph.Play();
        playable.SetTime(m_ElapsedTime);
    }

    //停止播放动画
    public void Stop()
    {
        m_ElapsedTime = 0;
        m_IsPlaying = false;
        m_PlayableGraph.Stop();
    }

    private void OnDestroy()
    {
        if (m_AnimatorAnimDic.ContainsKey(gameObject.GetHashCode()))
        {
            m_AnimatorAnimDic.Remove(gameObject.GetHashCode());
        }

        m_CurrentPlayable.Destroy();
        m_PlayableGraph.Destroy();
    }

    //播放动画
    public static void Play(Transform transform, string animName, Action finishedCallback)
    {
        if (!transform)
        {
            Debug.LogError($"UIPlayAnimatorAnimation.Play Error:[transform = null]");
            return;
        }

        Play(transform.gameObject, animName, finishedCallback);
    }

    //播放动画
    public static void Play(GameObject gameObject, string animName, Action finishedCallback)
    {
        if (!gameObject)
        {
            Debug.LogError($"UIPlayAnimatorAnimation.Play Error:[gameObject = null]");
            return;
        }

        UIPlayAnimatorAnimation playAnimatorAnimation = null;
        if (!m_AnimatorAnimDic.TryGetValue(gameObject.GetHashCode(), out playAnimatorAnimation))
        {
            playAnimatorAnimation = gameObject.AddComponent<UIPlayAnimatorAnimation>();
            m_AnimatorAnimDic.Add(gameObject.GetHashCode(), playAnimatorAnimation);
        }

        if (!playAnimatorAnimation)
        {
            Debug.LogError($"UIPlayAnimatorAnimation.Play Error:[playAnimatorAnimation = null]");
            return;
        }

        playAnimatorAnimation.Play(animName, finishedCallback);
    }
}

