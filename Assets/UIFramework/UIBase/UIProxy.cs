using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using static UGUIEventListener;

namespace UIFramework
{
    public abstract class UIProxy : IBaseEventListener
    {
        public GameUI GameUI;

        public abstract void OnInit();

        public abstract void OnEnter(params object[] args);

        public abstract void OnPause();

        public abstract void OnResume();

        public abstract void OnExit();

        public GameObject FindGameObject(string name)
        {
            if (GameUI == null || !GameUI.Transform)
                return null;
            return GameUI.Transform.FindGameObject(name);
        }

        public void RegisterListener(GameObject go, VoidDelegate handle, bool clear = true)
        {
            if (go)
            {
                UGUIEventListener listener = UGUIEventListener.Get(go);
                if (listener != null)
                {
                    if (clear) listener.onClick = null;
                    listener.onClick += handle;
                }
            }
        }

        public void RegisterListener(string name, VoidDelegate handle, bool clear = true)
        {
            GameObject go = FindGameObject(name);
            if (go)
            {
                UGUIEventListener listener = UGUIEventListener.Get(go);
                if (listener != null)
                {
                    if (clear) listener.onClick = null;
                    listener.onClick += handle;
                }
            }
        }

        public abstract void OnNotifiy(string evt, params object[] args);
    }
}
