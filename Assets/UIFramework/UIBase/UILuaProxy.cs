#if XLUA

using System;
using UnityEngine;
using XLua;

namespace UIFramework
{
    public class UILuaProxy : UIProxy
    {
        public LuaTable uiLuaTable { get; set; }
        ILuaUI luaUI = null;
        private static Func<string, UILuaProxy, ILuaUI> ms_NewFunc = null;

        public override void SetUi(UI ui)
        {
            this.ui = ui;

            if (ms_NewFunc == null)
                ms_NewFunc = Main.Instance.LuaEnv.Global.GetInPath<Func<string, UILuaProxy, ILuaUI>>("LuaUIManager.New");

            if (ms_NewFunc != null)
                this.luaUI = ms_NewFunc(this.ui.uiData.uiName, this);

            this.events = OnGetEvents();
        }

        //UI设置GameObject
        public override void SetGameObejct()
        {
            base.SetGameObejct();
            this.luaUI?.SetGameObject();
        }

        public void SetLuaTable(LuaTable uiLuaTable)
        {
            this.uiLuaTable = uiLuaTable;
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
            this.uiLuaTable = null;
            this.luaUI = null;
        }

        public Component FindComponent(string name, Type type)
        {
            if (this.ui == null || !this.ui.transform || type == null)
                return null;
            GameObject tempGo = this.ui.transform.FindGameObject(name);
            if (!tempGo)
                return null;
            return tempGo.GetComponent(type);
        }

        public override void OnNotify(string evt, IEventArgs args)
        {
            this.luaUI?.OnNotify(evt, args);
        }
    }
}
#endif