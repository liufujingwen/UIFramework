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
            if (UIContext == null || UIContext.UI == null || !UIContext.UI.GameObject)
                return null;
            return UIContext.UI.GameObject.FindComponent<T>(name);
        }

        public override void OnNotifiy(string evt, params object[] args)
        {

        }

        
    }
}
