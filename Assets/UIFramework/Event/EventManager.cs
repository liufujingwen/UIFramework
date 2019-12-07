using System;
using System.Collections.Generic;

public interface IEventArgs
{
}

public interface IEventArgs1<T> : IEventArgs
{
    T args1 { get; set; }
}

public interface IEventArgs2<T1, T2> : IEventArgs1<T1>
{
    T2 args2 { get; set; }
}

public interface IEventArgs3<T1, T2, T3> : IEventArgs2<T1, T2>
{
    T3 args3 { get; set; }
}

public interface IEventArgs4<T1, T2, T3, T4> : IEventArgs3<T1, T2, T3>
{
    T4 args4 { get; set; }
}

public interface IEventArgs5<T1, T2, T3, T4, T5> : IEventArgs4<T1, T2, T3, T4>
{
    T5 args5 { get; set; }
}

public struct EventArgs1<T> : IEventArgs1<T>
{
    public T args1 { get; set; }
}

public struct EventArgs2<T1, T2> : IEventArgs2<T1, T2>
{
    public T1 args1 { get; set; }
    public T2 args2 { get; set; }
}

public struct EventArgs3<T1, T2, T3> : IEventArgs3<T1, T2, T3>
{
    public T1 args1 { get; set; }
    public T2 args2 { get; set; }
    public T3 args3 { get; set; }
}

public struct EventArgs4<T1, T2, T3, T4> : IEventArgs4<T1, T2, T3, T4>
{
    public T1 args1 { get; set; }
    public T2 args2 { get; set; }
    public T3 args3 { get; set; }
    public T4 args4 { get; set; }
}

public struct EventArgs5<T1, T2, T3, T4, T5> : IEventArgs5<T1, T2, T3, T4, T5>
{
    public T1 args1 { get; set; }
    public T2 args2 { get; set; }
    public T3 args3 { get; set; }
    public T4 args4 { get; set; }
    public T5 args5 { get; set; }
}

public class EventManager
{
    public static EventManager instance { get; } = new EventManager();
    private Dictionary<string, List<Action<string, IEventArgs>>> m_EventDic = new Dictionary<string, List<Action<string, IEventArgs>>>();

    public void RegisterEvent(string evt, Action<string, IEventArgs> callback)
    {
        if (string.IsNullOrEmpty(evt))
        {
            throw new ArgumentException("EventManager.RegisterEvent() evt is null.");
        }

        if (callback == null)
        {
            throw new ArgumentException("EventManager.RegisterEvent() callback is null.");
        }

        List<Action<string, IEventArgs>> callbackList = null;
        if (!m_EventDic.TryGetValue(evt, out callbackList))
        {
            callbackList = new List<Action<string, IEventArgs>>();
            m_EventDic[evt] = callbackList;
        }

        if (callbackList.Contains(callback))
        {
            throw new Exception(string.Format("EventManager.RegisterEvent() evt:{0} repeat registered.", evt));
        }

        callbackList.Add(callback);
    }

    public void RemoveEvent(string evt, Action<string, IEventArgs> callback)
    {
        if (string.IsNullOrEmpty(evt))
        {
            throw new ArgumentException("EventManager.RemoveEvent() evt is null.");
        }

        if (callback == null)
        {
            throw new ArgumentException("EventManager.RemoveEvent() callback is null.");
        }

        List<Action<string, IEventArgs>> callbackList = null;
        if (!m_EventDic.TryGetValue(evt, out callbackList))
        {
            return;
        }

        callbackList.Remove(callback);
    }

    public void RemoveEvent(string evt)
    {
        if (string.IsNullOrEmpty(evt))
        {
            throw new ArgumentException("EventManager.RemoveEvent() evt is null.");
        }

        List<Action<string, IEventArgs>> callbackList = null;
        if (!m_EventDic.TryGetValue(evt, out callbackList))
        {
            return;
        }

        callbackList.Clear();
    }

    public void Notify<T>(string evt, T args)
    {
        if (string.IsNullOrEmpty(evt))
        {
            throw new ArgumentException("EventManager.Notify<T>() evt is null.");
        }

        List<Action<string, IEventArgs>> callbackList = null;
        if (!m_EventDic.TryGetValue(evt, out callbackList))
        {
            return;
        }

        EventArgs1<T> tmpEvent = new EventArgs1<T>();
        tmpEvent.args1 = args;

        for (int i = 0; i < callbackList.Count; i++)
        {
            Action<string, IEventArgs> callback = callbackList[i];
            callback?.Invoke(evt, tmpEvent);
        }
    }

    public void Notify<T1, T2>(string evt, T1 args1, T2 args2)
    {
        if (string.IsNullOrEmpty(evt))
        {
            throw new ArgumentException("EventManager.Notify<T> evt is null.");
        }

        List<Action<string, IEventArgs>> callbackList = null;
        if (!m_EventDic.TryGetValue(evt, out callbackList))
        {
            throw new Exception(string.Format("EventManager.Notify<T1, T2>() Can not find evt:{0}", evt));
        }

        EventArgs2<T1, T2> tmpEvent = new EventArgs2<T1, T2>();
        tmpEvent.args1 = args1;
        tmpEvent.args2 = args2;

        for (int i = 0; i < callbackList.Count; i++)
        {
            Action<string, IEventArgs> callback = callbackList[i];
            callback?.Invoke(evt, tmpEvent);
        }
    }

    public void Notify<T1, T2, T3>(string evt, T1 args1, T2 args2, T3 args3)
    {
        if (string.IsNullOrEmpty(evt))
        {
            throw new ArgumentException("EventManager.Notify<T> evt is null.");
        }

        List<Action<string, IEventArgs>> callbackList = null;
        if (!m_EventDic.TryGetValue(evt, out callbackList))
        {
            throw new Exception(string.Format("EventManager.Notify<T1, T2, T3>() Can not find evt:{0}", evt));
        }

        EventArgs3<T1, T2, T3> tmpEvent = new EventArgs3<T1, T2, T3>();
        tmpEvent.args1 = args1;
        tmpEvent.args2 = args2;
        tmpEvent.args3 = args3;

        for (int i = 0; i < callbackList.Count; i++)
        {
            Action<string, IEventArgs> callback = callbackList[i];
            callback?.Invoke(evt, tmpEvent);
        }
    }

    public void Notify<T1, T2, T3, T4>(string evt, T1 args1, T2 args2, T3 args3, T4 args4)
    {
        if (string.IsNullOrEmpty(evt))
        {
            throw new ArgumentException("EventManager.Notify<T>() evt is null.");
        }

        List<Action<string, IEventArgs>> callbackList = null;
        if (!m_EventDic.TryGetValue(evt, out callbackList))
        {
            throw new Exception(string.Format("EventManager.Notify<T1, T2, T3, T4>() Can not find evt:{0}", evt));
        }

        EventArgs4<T1, T2, T3, T4> tmpEvent = new EventArgs4<T1, T2, T3, T4>();
        tmpEvent.args1 = args1;
        tmpEvent.args2 = args2;
        tmpEvent.args3 = args3;
        tmpEvent.args4 = args4;

        for (int i = 0; i < callbackList.Count; i++)
        {
            Action<string, IEventArgs> callback = callbackList[i];
            callback?.Invoke(evt, tmpEvent);
        }
    }

    public void Notify<T1, T2, T3, T4, T5>(string evt, T1 args1, T2 args2, T3 args3, T4 args4, T5 args5)
    {
        if (string.IsNullOrEmpty(evt))
        {
            throw new ArgumentException("EventManager.Notify<T> evt is null.");
        }

        List<Action<string, IEventArgs>> callbackList = null;
        if (!m_EventDic.TryGetValue(evt, out callbackList))
        {
            throw new Exception(string.Format("EventManager.Notify<T1, T2, T3, T4, T5>() Can not find evt:{0}", evt));
        }

        EventArgs5<T1, T2, T3, T4, T5> tmpEvent = new EventArgs5<T1, T2, T3, T4, T5>();
        tmpEvent.args1 = args1;
        tmpEvent.args2 = args2;
        tmpEvent.args3 = args3;
        tmpEvent.args4 = args4;
        tmpEvent.args5 = args5;

        for (int i = 0; i < callbackList.Count; i++)
        {
            Action<string, IEventArgs> callback = callbackList[i];
            callback?.Invoke(evt, tmpEvent);
        }
    }
}