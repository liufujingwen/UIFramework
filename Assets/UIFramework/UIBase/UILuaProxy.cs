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

        public UILuaProxy(UIContex uiContext)
        {
            this.SetContext(uiContext);
            if (NewFunc == null)
                NewFunc = Main.Instance.LuaEnv.Global.GetInPath<Func<string, UILuaProxy, ILuaUI>>("LuaUIManager.New");

            if (RemoveAction == null)
                RemoveAction = Main.Instance.LuaEnv.Global.GetInPath<Action<string>>("XUiManager.RemoveClassType");

            if (NewFunc != null)
                luaUI = NewFunc(this.UIContext.UIData.UIName, this);
        }

        public override void OnInit()
        {
            luaUI?.OnInit();
        }

        public override void OnEnter(params object[] args)
        {
            luaUI?.OnEnter(args);
        }

        public override void OnPause()
        {
            luaUI?.OnPause();
        }

        public override void OnResume()
        {
            luaUI?.OnResume();
        }

        public override void OnExit()
        {
            luaUI?.OnExit();
            //removeAction?.Invoke(this.UIContext.UIData.UIName);
        }

        public Component FindComponent(string name, Type type)
        {
            if (this.UIContext == null || this.UIContext.UI == null || !this.UIContext.UI.Transform || type == null)
                return null;
            GameObject tempGo = this.UIContext.UI.Transform.FindGameObject(name);
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
