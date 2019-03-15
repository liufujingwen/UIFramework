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
        public void SetContext(UIContex uiContext)
        {
            this.UIContext = uiContext;
        }

        public UIContex UIContext;

        public abstract void OnAwake();

        public abstract void OnStart(params object[] args);

        public abstract void OnEnable();

        public abstract void OnDisable();

        public abstract void OnDestroy();

        public GameObject FindGameObject(string name)
        {
            if (UIContext == null || UIContext.UI == null || !UIContext.UI.Transform)
                return null;
            return UIContext.UI.Transform.FindGameObject(name);
        }

        public Transform FindTransform(string name)
        {
            if (UIContext == null || UIContext.UI == null || !UIContext.UI.Transform)
                return null;
            return UIContext.UI.Transform.FindTransform(name);
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

        public abstract void OnNotifiy(string evt, params object[] args);
    }
}
