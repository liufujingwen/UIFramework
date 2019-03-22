using System;
using System.Collections.Generic;
using System.Linq;


public abstract class EventListener : IBaseEventListener
{
    string[] Events = null;

    public abstract string[] OnGetEvents();

    public abstract void OnNotifiy(string evt, params object[] args);

    public bool Contains(string evt)
    {
        if (Events == null || string.IsNullOrEmpty(evt))
            return false;

        for (int i = 0; i < Events.Length; i++)
        {
            string temp = Events[i];
            if (temp == evt)
                return true;
        }

        return false;
    }

    public void SetEvent(string[] events)
    {
        this.Events = events;
    }

    public bool HasEvents()
    {
        return Events != null;
    }
}
