using System;
using System.Collections.Generic;
using UnityEngine;

public class UIPlayLegacyAnimation : MonoBehaviour
{
    public string AnimName;
    public Animation Animation;
    private bool IsPlaying;
    private Action FinishedCallback;//动画播放完成回调
    private float ElapsedTime;//动画经过的时间
    private float Length;//动画时长
    private AnimationState AnimationState;
    private static readonly Dictionary<int, UIPlayLegacyAnimation> LegacyAnimDic = new Dictionary<int, UIPlayLegacyAnimation>();
    private bool CheckPlayState = false;
    private float Speed = 0;
    private int WaitFrame = 0;

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

        if (!Animation)
        {
            return;
        }

        if (WaitFrame > 0)
        {
            WaitFrame--;
            return;
        }

        if (CheckPlayState)
        {
            CheckPlayState = false;
            AnimationState.speed = Speed;
        }

        ElapsedTime += Time.deltaTime;
        if (!Animation.isPlaying && ElapsedTime >= Length)
        {
            OnFinished();
        }
    }

    //动画播放完成
    private void OnFinished()
    {
        IsPlaying = false;
        ElapsedTime = 0;
        //保证动画最后一帧执行
        AnimationState.normalizedTime = 1;
        Animation.Play(AnimName);
        Action tempHandle = FinishedCallback;
        FinishedCallback = null;
        tempHandle?.Invoke();
    }

    public void Play(string animName, Action finishedCallback)
    {
        if (IsPlaying)
        {
            Debug.LogWarning($"上一个动画:{AnimName}还没有播放完成，然后就直接播放:{animName}");
        }

        //先停止
        Stop();

        AnimName = animName;
        FinishedCallback = finishedCallback;

        if (!gameObject.activeInHierarchy)
        {
            Debug.LogError($"UIPlayLegacyAnimation.Play Error:[{gameObject.name}:隐藏的gameObject不能播放动画]");
            return;
        }

        if (Animation == null)
        {
            Animation = gameObject.GetComponent<Animation>();
        }

        if (!Animation)
        {
            Debug.LogError($"UIPlayLegacyAnimation.Play Error:[{gameObject.name}:没有Animation组件]");
            return;
        }

        AnimationClip clip = Animation.GetClip(animName);
        if (!clip)
        {
            Debug.LogError($"UIPlayLegacyAnimation.Play Error:[{gameObject.name}:不存在Clip:{animName}]");
            return;
        }

        IsPlaying = true;
        ElapsedTime = 0;
        Length = clip.length;
        Animation.Play(animName);
        AnimationState = Animation[animName];
        Speed = AnimationState.speed;
        Speed = Speed == 0 ? 1 : Speed;
        AnimationState.speed = 0;
        CheckPlayState = true;
        WaitFrame = 2;
    }

    //停止播放动画
    public void Stop()
    {
        if (FinishedCallback != null)
        {
            Action tempCallback = FinishedCallback;
            FinishedCallback = null;
            tempCallback.Invoke();
        }

        ElapsedTime = 0;
        IsPlaying = false;
        if (AnimationState)
        {
            Speed = Speed == 0 ? 1 : Speed;
            AnimationState.speed = Speed;
        }
        Animation?.Stop();
        CheckPlayState = false;
        WaitFrame = 0;
    }

    private void OnDestroy()
    {
        if (LegacyAnimDic.ContainsKey(gameObject.GetHashCode()))
        {
            LegacyAnimDic.Remove(gameObject.GetHashCode());
        }
    }

    //播放动画
    public static void Play(Transform transform, string animName, Action finishedCallback)
    {
        if (!transform)
        {
            Debug.LogError($"UIPlayLegacyAnimation.Play Error:[transform = null]");
            return;
        }

        Play(transform.gameObject, animName, finishedCallback);
    }

    //播放动画
    public static void Play(GameObject gameObject, string animName, Action finishedCallback)
    {
        if (!gameObject)
        {
            Debug.LogError($"UIPlayLegacyAnimation.Play Error:[gameObject = null]");
            return;
        }

        UIPlayLegacyAnimation playLegacyAnim = null;
        if (!LegacyAnimDic.TryGetValue(gameObject.GetHashCode(), out playLegacyAnim))
        {
            playLegacyAnim = gameObject.AddComponent<UIPlayLegacyAnimation>();
            LegacyAnimDic.Add(gameObject.GetHashCode(), playLegacyAnim);
        }

        if (!playLegacyAnim)
        {
            Debug.LogError($"UIPlayLegacyAnimation.Play Error:[playLegacyAnim = null]");
            return;
        }

        playLegacyAnim.Play(animName, finishedCallback);
    }
}

