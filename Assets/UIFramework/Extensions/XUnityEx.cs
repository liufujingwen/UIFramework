using System;
using System.Collections.Generic;
using UnityEngine;

public static class XUnityEx
{
    private readonly static List<Transform> TempList = new List<Transform>();

    /// <summary>
    /// 通过名字查找GameObject
    /// 例如:A/B/C       
    /// C = FindGameObject("C");
    /// </summary>
    /// <param name="go"></param>
    /// <param name="name">GameOject名字</param>
    /// <returns></returns>
    public static GameObject FindGameObject(this GameObject go, string name)
    {
        if (!go) return null;
        return go.transform.FindGameObject(name);
    }

    /// <summary>
    /// 通过名字查找GameObject
    /// 例如:A/B/C       
    /// C = FindGameObject("C");
    /// </summary>
    /// <param name="t"></param>
    /// <param name="name">GameOject名字</param>
    /// <returns></returns>
    public static GameObject FindGameObject(this Transform t, string name)
    {
        Transform targetTransform = t.FindTransform(name);
        return targetTransform ? targetTransform.gameObject : null;
    }

    /// <summary>
    /// 通过名字查找GameObject
    /// 例如:A/B/C       
    /// C = FindTransform("C");
    /// </summary>
    /// <param name="go"></param>
    /// <param name="name">Transform名字</param>
    /// <returns></returns>
    public static Transform FindTransform(this GameObject go, string name)
    {
        if (!go) return null;
        return go.transform.FindTransform(name);
    }

    /// <summary>
    /// 通过名字查找Transform
    /// 例如:A/B/C 
    /// C = FindTransform("C");
    /// </summary>
    /// <param name="t"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static Transform FindTransform(this Transform t, string name)
    {
        if (!t) return null;
        TempList.Clear();
        TempList.Add(t);
        int index = 0;
        while (TempList.Count > index)
        {
            Transform transform = TempList[index++];
            for (int i = 0; i < transform.childCount; ++i)
            {
                Transform childTransform = transform.GetChild(i);
                if (childTransform.name == name)
                {
                    return childTransform;
                }
                TempList.Add(childTransform);
            }
        }
        return null;
    }

    /// <summary>
    /// 通过名字查找GameObject
    /// 例如:A/B/C
    /// 1、 C = FindGameObjectWithSprit("C");
    /// 2、 C = FindGameObjectWithSprit("B/C");
    /// 3、 C = FindGameObjectWithSprit("A/B/C");
    /// 4、 C = FindGameObjectWithSprit("A/C");
    /// </summary>
    /// <param name="go"></param>
    /// <param name="name">GameObject名字</param>
    /// <returns></returns>
    public static Transform FindGameObjectWithSprit(this GameObject go, string name)
    {
        if (!go) return null;
        return go.transform.FindTransformWithSprit(name);
    }

    /// <summary>
    /// 通过名字查找GameObject
    /// 例如:A/B/C
    /// 1、 C = FindGameObjectWithSprit("C");
    /// 2、 C = FindGameObjectWithSprit("B/C");
    /// 3、 C = FindGameObjectWithSprit("A/B/C");
    /// 4、 C = FindGameObjectWithSprit("A/C");
    /// </summary>
    /// <param name="t"></param>
    /// <param name="name">Transform名字</param>
    /// <returns></returns>
    public static GameObject FindGameObjectWithSprit(this Transform t, string name)
    {
        if (!t) return null;
        Transform tempTransform = t.FindTransformWithSprit(name);
        return tempTransform ? tempTransform.gameObject : null;
    }

    /// <summary>
    /// 通过名字查找Transform
    /// 例如:A/B/C
    /// 1、 C = FindTransformWithSprit("C");
    /// 2、 C = FindTransformWithSprit("B/C");
    /// 3、 C = FindTransformWithSprit("A/B/C");
    /// 4、 C = FindTransformWithSprit("A/C");
    /// </summary>
    /// <param name="t"></param>
    /// <param name="name">Transform名字</param>
    /// <returns></returns>
    public static Transform FindTransformWithSprit(this Transform t, string name)
    {
        if (!t || string.IsNullOrEmpty(name))
            return null;

        string[] names = name.Split('/');

        if (names.Length == 0)
            return null;

        Transform trans = t;

        for (int i = 0; i < names.Length; i++)
        {
            trans = trans.FindTransform(names[i]);

            if (!trans)
                return null;

            if (i == names.Length - 1)
                return trans;
        }

        return null;
    }

    /// <summary>
    /// 通过名字查找某个子节点的Component
    /// </summary>
    /// <typeparam name="T">Component</typeparam>
    /// <param name="t"></param>
    /// <param name="name">子节点名字</param>
    /// <returns></returns>
    public static T FindComponent<T>(this GameObject t, string name) where T : Component
    {
        if (t == null) return null;
        Transform tempTransform = t.transform.FindTransform(name);
        if (tempTransform == null) return null;
        return tempTransform.gameObject.GetComponent<T>();
    }

    #region PlayAnimation

    public static void PlayLegacyAnimation(this GameObject gameObject, string animName, Action finishedCallback)
    {
        if (!gameObject)
        {
            return;
        }

        PlayLegacyAnimation(gameObject.transform, animName, finishedCallback);
    }

    public static void PlayLegacyAnimation(this Transform transform, string animName, Action finishedCallback)
    {
        if (!transform)
        {
            return;
        }

        UIPlayLegacyAnimation.Play(transform, animName, finishedCallback);
    }

    public static void PlayAnimatorAnimation(this GameObject gameObject, string animName, Action finishedCallback)
    {
        if (!gameObject)
        {
            return;
        }

        PlayAnimatorAnimation(gameObject.transform, animName, finishedCallback);
    }

    public static void PlayAnimatorAnimation(this Transform transform, string animName, Action finishedCallback)
    {
        if (!transform)
        {
            return;
        }

        UIPlayAnimatorAnimation.Play(transform, animName, finishedCallback);
    }

    public static void PlayTimelineAnimation(this GameObject gameObject, string animName, Action finishedCallback)
    {
        if (!gameObject)
        {
            return;
        }

        PlayTimelineAnimation(gameObject.transform, animName, finishedCallback);
    }

    public static void PlayTimelineAnimation(this Transform transform, string animName, Action finishedCallback)
    {
        if (!transform)
        {
            return;
        }

        UIPlayTimelineAnimation.Play(transform, animName, finishedCallback);
    }

    #endregion
}
