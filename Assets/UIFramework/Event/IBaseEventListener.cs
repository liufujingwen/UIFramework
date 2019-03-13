using System;
using System.Collections.Generic;

public interface IBaseEventListener
{
    void OnNotifiy(string evt, params object[] args);
}

