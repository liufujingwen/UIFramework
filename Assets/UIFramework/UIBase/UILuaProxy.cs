using System;
using System.Collections.Generic;
using UnityEngine;
using XLua;

namespace UIFramework
{
    public class UILuaProxy : UIProxy
    {
        public LuaTable UiLuaTable = null;
        ILuaUI luaUI = null;
        static Func<string, UILuaProxy, ILuaUI> NewFunc = null;

        public UILuaProxy(UI ui)
        {
            this.SetUi(ui);
            if (NewFunc == null)
                NewFunc = Main.Instance.LuaEnv.Global.GetInPath<Func<string, UILuaProxy, ILuaUI>>("LuaUIManager.New");

            if (NewFunc != null)
                this.luaUI = NewFunc(this.UI.UiData.UiName, this);
        }

        public void SetLuaTable(LuaTable uiLuaTable)
        {
            this.UiLuaTable = uiLuaTable;
        }


        public override string[] OnGetEvents()
        {
            return this.luaUI.OnGetEvents();
        }

        public override void OnAwake()
        {
            this.luaUI?.OnAwake();
        }

        public override void OnStart(params object[] args)
        {
            this.luaUI?.OnStart(args);
        }

        public override void OnEnable()
        {
            this.luaUI?.OnEnable();
        }

        public override void OnDisable()
        {
            this.luaUI?.OnDisable();
        }

        public override void OnDestroy()
        {
            this.luaUI?.OnDestroy();
            this.UiLuaTable = null;
            this.luaUI = null;
        }

        public Component FindComponent(string name, Type type)
        {
            if (this.UI == null || !this.UI.Transform || type == null)
                return null;
            GameObject tempGo = this.UI.Transform.FindGameObject(name);
            if (!tempGo)
                return null;
            return tempGo.GetComponent(type);
        }

        public override void OnNotify(string evt, params object[] args)
        {
            this.luaUI?.OnNotify(evt, args);
        }
    }
}
