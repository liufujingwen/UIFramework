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
    public abstract class UIProxy : EventListener
    {
        public void SetUi(UI ui)
        {
            this.UI = ui;
        }

        public UI UI;

        public override string[] OnGetEvents()
        {
            return null;
        }

        public abstract void OnAwake();

        public abstract void OnStart(params object[] args);

        public abstract void OnEnable();

        public abstract void OnDisable();

        public abstract void OnDestroy();

        public GameObject FindGameObject(string name)
        {
            if (UI == null || UI == null || !UI.Transform)
                return null;
            return UI.Transform.FindGameObject(name);
        }

        public Transform FindTransform(string name)
        {
            if (UI == null || UI == null || !UI.Transform)
                return null;
            return UI.Transform.FindTransform(name);
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

        public void OpenChildUI(string childUIName, params object[] args)
        {
            if (UI == null)
                return;

            GameUI gameUI = UI as GameUI;
            if (gameUI == null)
                return;

            gameUI.OpenChildUI(childUIName, args);
        }

        public void CloseChildUI(string childUIName)
        {
            GameUI gameUI = UI as GameUI;
            if (gameUI == null)
                return;

            gameUI.CloseChildUI(childUIName);
        }
    }
}
