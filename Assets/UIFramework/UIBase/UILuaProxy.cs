using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIFramework
{
    public class UILuaProxy : UIProxy
    {
        public override void OnInit()
        {
        }

        public override void OnEnter(params object[] args)
        {
        }

        public override void OnPause()
        {
        }

        public override void OnResume()
        {
        }

        public override void OnExit()
        {
        }

        public Component FindComponent(string name, Type type)
        {
            if (GameUI == null || !GameUI.Transform)
                return null;
            GameObject tempGo = GameUI.Transform.FindGameObject(name);
            if (!tempGo)
                return null;
            return tempGo.GetComponent(type);
        }

        public override void OnNotifiy(string evt, params object[] args)
        {
        }
    }
}
