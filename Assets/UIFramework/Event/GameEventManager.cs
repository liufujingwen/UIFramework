using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class GameEventManager : Singleton<GameEventManager>
{
    //保存所有的监听事件
    List<IBaseEventListener> eventList = new List<IBaseEventListener>();

    /// <summary>
    /// 注册事件
    /// </summary>
    /// <param name="evt">事件ID</param>
    /// <param name="listener">事件监听者</param>
    public void RegistEvent(IBaseEventListener listener)
    {
        if (listener == null)
            return;

        if (eventList.Contains(listener))
            return;

        eventList.Add(listener);
    }

    /// <summary>
    /// 删除事件
    /// </summary>
    /// <param name="evt">事件ID</param>
    /// <param name="listener">事件监听者</param>
    public void RemoveEvent(IBaseEventListener listener)
    {
        if (listener == null)
            return;

        eventList.Remove(listener);
    }

    /// <summary>
    /// 派发事件
    /// </summary>
    /// <param name="evt">事件ID</param>
    /// <param name="args">事件参数</param>
    public void Notify(string evt, params object[] args)
    {
        for (int i = 0; i < eventList.Count; i++)
        {
            IBaseEventListener listener = eventList[i];
            listener?.OnNotifiy(evt, args);
        }

    }

    /// <summary>
    /// 清空所有事件
    /// </summary>
    public void Clear()
    {
        eventList.Clear();
    }

}

