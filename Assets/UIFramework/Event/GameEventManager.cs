using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class GameEventManager : Singleton<GameEventManager>
{
    //保存所有的监听事件
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
        eventDic.Clear();
    }
}