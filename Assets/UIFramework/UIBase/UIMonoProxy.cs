using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UIFramework
{
    public class UIMonoProxy : UIProxy
    {
        public override void SetUi(UI ui)
        {
            this.UI = ui;
            this.Events = OnGetEvents();
        }

        public override void OnAwake()
        {
        }

        public override void OnStart(params object[] args)
        {
        }

        public override void OnEnable()
        {
        }

        public override void OnDisable()
        {
        }

        public override void OnDestroy()
        {
        }

        public T FindComponent<T>(string name) where T : Component
        {
            if (UI == null || UI == null || !UI.GameObject)
                return null;
            return UI.GameObject.FindComponent<T>(name);
        }

        public override string[] OnGetEvents()
        {
            return null;
        }

        public override void OnNotify(string evt, IEventArgs args)
        {
        }
    }
}
