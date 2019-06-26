using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

[RequireComponent(typeof(Animator))]
public class UIPlayAnimatorAnimation : MonoBehaviour
{
    public string AnimName;
    public AnimationClip[] Clips;
    private Animator Animator;
    private bool IsPlaying;
    private Action FinishedCallback;//动画播放完成回调
    private float ElapsedTime;//动画经过的时间
    private float Length;//动画时长

    private PlayableGraph PlayableGraph;
    private AnimationPlayableOutput AnimationPlayableOutput;
    private AnimationClipPlayable CurrentPlayable;

    private readonly Dictionary<string, AnimationClipPlayable> PlayableDic = new Dictionary<string, AnimationClipPlayable>();
    private static readonly Dictionary<int, UIPlayAnimatorAnimation> AnimatorAnimDic = new Dictionary<int, UIPlayAnimatorAnimation>();

    private void OnDisable()
    {
        Stop();
    }

    private void Update()
    {
        if (!IsPlaying)
        {
            return;
        }

        if (!Animator)
        {
            return;
        }

        ElapsedTime += Time.deltaTime;

        if (ElapsedTime >= Length)
        {
            CurrentPlayable.SetTime(Length);
            OnFinished();
        }
        else
        {
            CurrentPlayable.SetTime(ElapsedTime);
        }
    }

    //动画播放完成
    private void OnFinished()
    {
        IsPlaying = false;
        ElapsedTime = 0;
        PlayableGraph.Stop();
        Action tempHandle = FinishedCallback;
        FinishedCallback = null;
        tempHandle?.Invoke();
    }

    //播放动画
    public void Play(string animName, Action finishedCallback)
    {
        AnimName = animName;
        FinishedCallback = finishedCallback;

        if (!gameObject.activeInHierarchy)
        {
            Debug.LogError($"UIPlayAnimatorAnimation.Play Error:[{gameObject.name}:隐藏的gameObject不能播放动画]");
            return;
        }

        if (Animator == null)
        {
            Animator = gameObject.GetComponent<Animator>();
        }

        if (!Animator)
        {
            Debug.LogError($"UIPlayAnimatorAnimation.Play Error:[{gameObject.name}:不存在Animation组件]");
            return;
        }

        if (!PlayableGraph.IsValid())
        {
            PlayableGraph = PlayableGraph.Create();
            AnimationPlayableOutput = AnimationPlayableOutput.Create(PlayableGraph, "Animation", Animator);
        }

        AnimationClipPlayable playable;
        if (!PlayableDic.TryGetValue(animName, out playable))
        {
            bool Exist = false;

            for (int i = 0; i < Clips.Length; i++)
            {
                AnimationClip tempClip = Clips[i];
                if (tempClip != null && tempClip.name == animName)
                {
                    CurrentPlayable = AnimationClipPlayable.Create(PlayableGraph, tempClip);
                    PlayableDic[tempClip.name] = playable;
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

        AnimationClip clip = CurrentPlayable.GetAnimationClip();
        if (!clip)
        {
            Debug.LogError($"UIPlayAnimatorAnimation.Play Error:[{gameObject.name}:不存在Clip:{animName}]");
            return;
        }

        IsPlaying = true;
        ElapsedTime = 0;
        Length = clip.length;
        AnimationPlayableOutput.SetSourcePlayable(playable);
        PlayableGraph.Play();
        playable.SetTime(ElapsedTime);
    }

    //停止播放动画
    public void Stop()
    {
        ElapsedTime = 0;
        IsPlaying = false;
        PlayableGraph.Stop();
    }

    private void OnDestroy()
    {
        if (AnimatorAnimDic.ContainsKey(gameObject.GetHashCode()))
        {
            AnimatorAnimDic.Remove(gameObject.GetHashCode());
        }

        CurrentPlayable.Destroy();
        PlayableGraph.Destroy();
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
        if (!AnimatorAnimDic.TryGetValue(gameObject.GetHashCode(), out playAnimatorAnimation))
        {
            playAnimatorAnimation = gameObject.AddComponent<UIPlayAnimatorAnimation>();
            AnimatorAnimDic.Add(gameObject.GetHashCode(), playAnimatorAnimation);
        }

        if (!playAnimatorAnimation)
        {
            Debug.LogError($"UIPlayAnimatorAnimation.Play Error:[playAnimatorAnimation = null]");
            return;
        }

        playAnimatorAnimation.Play(animName, finishedCallback);
    }
}

