using System;
using System.Collections.Generic;
using UnityEngine;

public class UIPlayLegacyAnimation : MonoBehaviour
{
    public string animName;
    public Animation anim;
    private bool m_IsPlaying;
    private Action m_FinishedCallback;//动画播放完成回调
    private float m_ElapsedTime;//动画经过的时间
    private float m_Length;//动画时长
    private AnimationState m_AnimationState;
    private static readonly Dictionary<int, UIPlayLegacyAnimation> m_LegacyAnimDic = new Dictionary<int, UIPlayLegacyAnimation>();
    private bool m_CheckPlayState = false;
    private float m_Speed = 0;
    private int m_WaitFrame = 0;

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

        if (!anim)
        {
            return;
        }

        if (m_WaitFrame > 0)
        {
            m_WaitFrame--;
            return;
        }

        if (m_CheckPlayState)
        {
            m_CheckPlayState = false;
            m_AnimationState.speed = m_Speed;
        }

        m_ElapsedTime += Time.deltaTime;
        if (!anim.isPlaying && m_ElapsedTime >= m_Length)
        {
            OnFinished();
        }
    }

    //动画播放完成
    private void OnFinished()
    {
        m_IsPlaying = false;
        m_ElapsedTime = 0;
        //保证动画最后一帧执行
        m_AnimationState.normalizedTime = 1;
        anim.Play(animName);
        Action tempHandle = m_FinishedCallback;
        m_FinishedCallback = null;
        tempHandle?.Invoke();
    }

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
            Debug.LogError($"UIPlayLegacyAnimation.Play Error:[{gameObject.name}:隐藏的gameObject不能播放动画]");
            return;
        }

        if (anim == null)
        {
            anim = gameObject.GetComponent<Animation>();
        }

        if (!anim)
        {
            Debug.LogError($"UIPlayLegacyAnimation.Play Error:[{gameObject.name}:没有Animation组件]");
            return;
        }

        AnimationClip clip = anim.GetClip(animName);
        if (!clip)
        {
            Debug.LogError($"UIPlayLegacyAnimation.Play Error:[{gameObject.name}:不存在Clip:{animName}]");
            return;
        }

        m_IsPlaying = true;
        m_ElapsedTime = 0;
        m_Length = clip.length;
        anim.Play(animName);
        m_AnimationState = anim[animName];
        m_Speed = m_AnimationState.speed;
        m_Speed = m_Speed == 0 ? 1 : m_Speed;
        m_AnimationState.speed = 0;
        m_CheckPlayState = true;
        m_WaitFrame = 2;
    }

    //停止播放动画
    public void Stop()
    {
        if (m_FinishedCallback != null)
        {
            Action tempCallback = m_FinishedCallback;
            m_FinishedCallback = null;
            tempCallback.Invoke();
        }

        m_ElapsedTime = 0;
        m_IsPlaying = false;
        if (m_AnimationState)
        {
            m_Speed = m_Speed == 0 ? 1 : m_Speed;
            m_AnimationState.speed = m_Speed;
        }
        anim?.Stop();
        m_CheckPlayState = false;
        m_WaitFrame = 0;
    }

    private void OnDestroy()
    {
        if (m_LegacyAnimDic.ContainsKey(gameObject.GetHashCode()))
        {
            m_LegacyAnimDic.Remove(gameObject.GetHashCode());
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
        if (!m_LegacyAnimDic.TryGetValue(gameObject.GetHashCode(), out playLegacyAnim))
        {
            playLegacyAnim = gameObject.AddComponent<UIPlayLegacyAnimation>();
            m_LegacyAnimDic.Add(gameObject.GetHashCode(), playLegacyAnim);
        }

        if (!playLegacyAnim)
        {
            Debug.LogError($"UIPlayLegacyAnimation.Play Error:[playLegacyAnim = null]");
            return;
        }

        playLegacyAnim.Play(animName, finishedCallback);
    }
}

