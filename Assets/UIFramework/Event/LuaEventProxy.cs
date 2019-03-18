using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIFramework
{
    public class LuaEventProxy : IBaseEventListener
    {
        Action<string, object[]> luaAction = null;

        public LuaEventProxy(Action<string, object[]> action)
        {
            this.luaAction = action;
        }

        public void OnNotifiy(string evt, params object[] args)
        {
            this.luaAction?.Invoke(evt, args);
        }
    }
}
