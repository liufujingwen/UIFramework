using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(PlayableDirector))]
public class UIPlayTimelineAnimation : MonoBehaviour
{
    public string animName;
    private PlayableDirector m_Director;
    private bool m_IsPlaying;
    private Action m_FinishedCallback;//动画播放完成回调
    private float m_ElapsedTime;//动画经过的时间
    private float m_Length;//动画时长
    private static readonly Dictionary<int, UIPlayTimelineAnimation> m_TimelineAnimAnimDic = new Dictionary<int, UIPlayTimelineAnimation>();
    private int m_WaitFrame = 0;

    private void OnDisable()
    {
        Stop(false);
    }

    private void Update()
    {
        if (!m_IsPlaying)
        {
            return;
        }

        if (m_Director.state != PlayState.Playing)
        {
            return;
        }

        if (m_WaitFrame > 0)
        {
            m_Director.time = m_ElapsedTime;
            m_WaitFrame--;
            return;
        }

        m_ElapsedTime += Time.deltaTime;
        m_Director.time = m_ElapsedTime;

        if (m_ElapsedTime >= m_Length)
        {
            OnFinished();
        }
    }

    //动画播放完成
    private void OnFinished()
    {
        m_IsPlaying = false;
        m_Director.time = m_Length;
        m_Director.Evaluate();
        m_ElapsedTime = 0;
        Action tempHandle = m_FinishedCallback;
        m_FinishedCallback = null;
        tempHandle?.Invoke();
    }

    //播放动画
    public void Play(string animName, Action finishedCallback)
    {
        if (m_IsPlaying)
        {
            Debug.LogWarning($"上一个动画:{this.animName}还没有播放完成，然后就直接播放:{animName}");
        }

        //先停止
        Stop();

        this.animName = animName;
        m_FinishedCallback = finishedCallback;

        if (!gameObject.activeInHierarchy)
        {
            Debug.LogError($"UIPlayTimelineAnimation.Play Error:[{gameObject.name}:隐藏的gameObject不能播放动画]");
            return;
        }

        if (m_Director == null)
        {
            m_Director = gameObject.GetComponent<PlayableDirector>();
            m_Director.playOnAwake = false;
            m_Director.extrapolationMode = DirectorWrapMode.Hold;
        }

        if (!m_Director)
        {
            Debug.LogError($"UIPlayTimelineAnimation.Play Error:[{gameObject.name}:不存在Animation组件]");
            return;
        }

        m_IsPlaying = true;
        m_Length = (float)m_Director.duration;
        m_Director.Play();
        m_WaitFrame = 2;
    }

    //停止播放动画
    public void Stop(bool evaluate = true)
    {
        if (m_FinishedCallback != null)
        {
            Action tempCallback = m_FinishedCallback;
            m_FinishedCallback = null;
            tempCallback.Invoke();
        }

        m_ElapsedTime = 0;
        m_IsPlaying = false;
        m_Director?.Stop();

        if (evaluate)
        {
            m_Director?.Evaluate();
        }
    }

    private void OnDestroy()
    {
        if (m_TimelineAnimAnimDic.ContainsKey(gameObject.GetHashCode()))
        {
            m_TimelineAnimAnimDic.Remove(gameObject.GetHashCode());
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
        if (!m_TimelineAnimAnimDic.TryGetValue(gameObject.GetHashCode(), out playTimelineAnimation))
        {
            playTimelineAnimation = gameObject.AddComponent<UIPlayTimelineAnimation>();
            m_TimelineAnimAnimDic.Add(gameObject.GetHashCode(), playTimelineAnimation);
        }

        if (!playTimelineAnimation)
        {
            Debug.LogError($"UIPlayTimelineAnimation.Play Error:[playTimelineAnimation = null]");
            return;
        }

        playTimelineAnimation.Play(animName, finishedCallback);
    }
}

