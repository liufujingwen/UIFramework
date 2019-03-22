using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class GameEventManager : Singleton<GameEventManager>
{
    //保存所有的监听事件
    private List<EventListener> eventList = new List<EventListener>();
    private Dictionary<string, List<Action<string, object[]>>> eventDic = new Dictionary<string, List<Action<string, object[]>>>();

    /// <summary>
    /// 注册事件
    /// </summary>
    /// <param name="evt">事件ID</param>
    /// <param name="action">事件回调</param>
    public void RegisterEvent(string evt, Action<string, object[]> action)
    {
        if (string.IsNullOrEmpty(evt) || action == null)
            return;

        List<Action<string, object[]>> actionList = null;
        if (!eventDic.TryGetValue(evt, out actionList))
        {
            actionList = new List<Action<string, object[]>>();
            eventDic[evt] = actionList;
        }

        if (!actionList.Contains(action))
            actionList.Add(action);
    }

    /// <summary>
    /// 注册事件监听器
    /// </summary>
    /// <param name="listener">事件监听者</param>
    public void RegisterEvent(EventListener listener)
    {
        if (listener == null)
            return;

        //如果没有事件，尝试获取监听的事件名称
        if (!listener.HasEvents())
            listener.SetEvent(listener.OnGetEvents());

        if (!listener.HasEvents())
            return;

        if (eventList.Contains(listener))
            return;

        eventList.Add(listener);
    }

    /// <summary>
    /// 删除事件监听器
    /// </summary>
    /// <param name="listener">事件监听者</param>
    public void RemoveEvent(EventListener listener)
    {
        if (listener == null)
            return;
        if (!listener.HasEvents())
            return;
        eventList.Remove(listener);
    }

    /// <summary>
    /// 删除事件
    /// </summary>
    /// <param name="evt">事件ID</param>
    /// <param name="listener">事件监听者</param>
    public void RemoveEvent(string evt, Action<string, object[]> action)
    {
        if (string.IsNullOrEmpty(evt) || action == null)
            return;

        List<Action<string, object[]>> actionList = null;
        if (!eventDic.TryGetValue(evt, out actionList))
        {
            actionList = new List<Action<string, object[]>>();
            eventDic[evt] = actionList;
        }

        actionList.Remove(action);
    }

    /// <summary>
    /// 派发事件
    /// </summary>
    /// <param name="evt">事件ID</param>
    /// <param name="args">事件参数</param>
    public void Notify(string evt, params object[] args)
    {
        if (string.IsNullOrEmpty(evt))
            return;

        for (int i = 0; i < eventList.Count; i++)
        {
            EventListener listener = eventList[i];
            if (listener != null)
            {
                if (listener.Contains(evt))
                    listener.OnNotifiy(evt, args);
            }
        }

        List<Action<string, object[]>> actionList = null;
        if (eventDic.TryGetValue(evt, out actionList))
        {
            for (int i = 0; i < actionList.Count; i++)
            {
                Action<string, object[]> action = actionList[i];
                action?.Invoke(evt, args);
            }
        }
    }

    /// <summary>
    /// 清空所有事件
    /// </summary>
    public void Clear()
    {
        eventList.Clear();
        eventDic.Clear();
    }
}