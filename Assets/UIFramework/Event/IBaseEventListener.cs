using System;
using System.Collections.Generic;


public interface IBaseEventListener
{
    string[] OnGetEvents();
    bool HasEvents();
    bool Contains(string evt);
    void OnNotify(string evt, params object[] args);
}

