using System;
using System.Collections.Generic;

namespace UIFramework
{
    public interface IBaseEventListener
    {
        void OnNotifiy(string evt, params object[] args);
    }
}

