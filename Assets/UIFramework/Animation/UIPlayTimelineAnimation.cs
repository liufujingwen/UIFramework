using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(PlayableDirector))]
public class UIPlayTimelineAnimation : MonoBehaviour
{
    public string AnimName;
    private PlayableDirector Director;
    private bool IsPlaying;
    private Action FinishedCallback;//动画播放完成回调
    private float ElapsedTime;//动画经过的时间
    private float Length;//动画时长
    private static readonly Dictionary<int, UIPlayTimelineAnimation> TimelineAnimAnimDic = new Dictionary<int, UIPlayTimelineAnimation>();
    private bool CheckPlayState = false;
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

        if (Director.state != PlayState.Playing)
        {
            return;
        }

        if (WaitFrame > 0)
        {
            Director.time = ElapsedTime;
            WaitFrame--;
            return;
        }

        if (CheckPlayState)
        {
            CheckPlayState = false;
        }

        ElapsedTime += Time.deltaTime;
        Director.time = ElapsedTime;

        if (ElapsedTime >= Length && Director.time >= Length)
        {
            OnFinished();
        }
    }

    //动画播放完成
    private void OnFinished()
    {
        IsPlaying = false;
        Director?.Stop();
        ElapsedTime = 0;
        Action tempHandle = FinishedCallback;
        FinishedCallback = null;
        tempHandle?.Invoke();
    }

    //播放动画
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
            Debug.LogError($"UIPlayTimelineAnimation.Play Error:[{gameObject.name}:隐藏的gameObject不能播放动画]");
            return;
        }

        if (Director == null)
        {
            Director = gameObject.GetComponent<PlayableDirector>();
            Director.playOnAwake = false;
            Director.extrapolationMode = DirectorWrapMode.Hold;
        }

        if (!Director)
        {
            Debug.LogError($"UIPlayTimelineAnimation.Play Error:[{gameObject.name}:不存在Animation组件]");
            return;
        }

        IsPlaying = true;
        Length = (float)Director.duration;
        Director.Play();
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
        Director?.Stop();
    }

    private void OnDestroy()
    {
        if (TimelineAnimAnimDic.ContainsKey(gameObject.GetHashCode()))
        {
            TimelineAnimAnimDic.Remove(gameObject.GetHashCode());
        }
    }

    //播放动画
    public static void Play(Transform transform, string animName, Action finishedCallback)
    {
        if (!transform)
        {
            Debug.LogError($"UIPlayTimelineAnimation.Play Error:[transform = null]");
            return;
        }

        Play(transform.gameObject, animName, finishedCallback);
    }

    //播放动画
    public static void Play(GameObject gameObject, string animName, Action finishedCallback)
    {
        if (!gameObject)
        {
            Debug.LogError($"UIPlayTimelineAnimation.Play Error:[gameObject = null]");
            return;
        }

        UIPlayTimelineAnimation playTimelineAnimation = null;
        if (!TimelineAnimAnimDic.TryGetValue(gameObject.GetHashCode(), out playTimelineAnimation))
        {
            playTimelineAnimation = gameObject.AddComponent<UIPlayTimelineAnimation>();
            TimelineAnimAnimDic.Add(gameObject.GetHashCode(), playTimelineAnimation);
        }

        if (!playTimelineAnimation)
        {
            Debug.LogError($"UIPlayTimelineAnimation.Play Error:[playTimelineAnimation = null]");
            return;
        }

        playTimelineAnimation.Play(animName, finishedCallback);
    }
}

