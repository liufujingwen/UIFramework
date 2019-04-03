using System;
using System.Collections.Generic;
using UnityEngine;
using XLua;

namespace UIFramework
{
    public class UILuaProxy : UIProxy
    {
        ILuaUI luaUI = null;
        static Action<string> RemoveAction = null;
        static Func<string, UILuaProxy, ILuaUI> NewFunc = null;

        public UILuaProxy(UI ui)
        {
            this.SetUi(ui);
            if (NewFunc == null)
                NewFunc = Main.Instance.LuaEnv.Global.GetInPath<Func<string, UILuaProxy, ILuaUI>>("LuaUIManager.New");

            if (RemoveAction == null)
                RemoveAction = Main.Instance.LuaEnv.Global.GetInPath<Action<string>>("LuaUIManager.RemoveClassType");

            if (NewFunc != null)
                luaUI = NewFunc(this.UI.UiData.UiName, this);
        }

        public override string[] OnGetEvents()
        {
            return luaUI.OnGetEvents();
        }

        public override void OnAwake()
        {
            luaUI?.OnAwake();
        }

        public override void OnStart(params object[] args)
        {
            luaUI?.OnStart(args);
        }

        public override void OnEnable()
        {
            luaUI?.OnEnable();
        }

        public override void OnDisable()
        {
            luaUI?.OnDisable();
        }

        public override void OnDestroy()
        {
            luaUI?.OnDestroy();
            RemoveAction?.Invoke(this.UI.UiData.UiName);
            luaUI = null;

        }

        public Component FindComponent(string name, Type type)
        {
            if (this.UI == null || this.UI == null || !this.UI.Transform || type == null)
                return null;
            GameObject tempGo = this.UI.Transform.FindGameObject(name);
            if (!tempGo)
                return null;
            return tempGo.GetComponent(type);
        }

        public override void OnNotifiy(string evt, params object[] args)
        {
            luaUI?.OnNotifiy(evt, args);
        }
    }
}
