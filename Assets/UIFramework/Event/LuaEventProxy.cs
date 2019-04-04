using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XLua;

public class XLuaEventProxy : EventListener
{
    LuaTable luaTable = null;
    Func<string[]> OnGetEventsFunc = null;
    Action<string, object[]> OnNotifyActoin = null;

    public XLuaEventProxy(LuaTable luaTable)
    {
        if (luaTable == null)
            return;

        this.luaTable = luaTable;
        this.OnGetEventsFunc = luaTable.Get<Func<string[]>>("OnGetEvents");
        this.OnNotifyActoin = luaTable.Get<Action<string, object[]>>("OnNotifiy");
    }

    public override string[] OnGetEvents()
    {
        return OnGetEventsFunc?.Invoke();
    }

    public override void OnNotify(string evt, params object[] args)
    {
        OnNotifyActoin?.Invoke(evt, args);
    }
}


