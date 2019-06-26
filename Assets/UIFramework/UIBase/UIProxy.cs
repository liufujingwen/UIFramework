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
    public abstract class UIProxy
    {
        public UI UI = null;
        public GameObject GameObject = null;
        public Transform Transform = null;
        public string[] Events = null;

        public abstract void SetUi(UI ui);

        public virtual void SetGameObejct()
        {
            GameObject = UI.GameObject;
            Transform = UI.Transform;
        }

        public abstract string[] OnGetEvents();

        public abstract void OnAwake();

        public abstract void OnStart(params object[] args);

        public abstract void OnEnable();

        public abstract void OnDisable();

        public abstract void OnDestroy();

        public abstract void OnNotify(string evt, params object[] args);

        public GameObject FindGameObject(string name)
        {
            if (UI == null || !UI.Transform)
                return null;
            return UI.Transform.FindGameObject(name);
        }

        public Transform FindTransform(string name)
        {
            if (UI == null || !UI.Transform)
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

        //只打开一个子UI，已显示的UI会被关闭
        public void OpenOneChildUI(string childUiName, params object[] args)
        {
            if (this.UI == null)
                return;

            GameUI gameUi = this.UI as GameUI;
            if (gameUi == null)
                return;

            gameUi.OpenOneChildUi(childUiName, args);
        }

        public void CloseChildUI(string childUIName)
        {
            GameUI gameUI = UI as GameUI;
            if (gameUI == null)
                return;

            gameUI.CloseChildUI(childUIName);
        }

        public void PlayAnimation(string animName, Action finishedCallback = null)
        {
            UI?.PlayAnimation(animName, finishedCallback);
        }

        public void Close()
        {
            if (this.UI == null)
                return;

            if (this.UI is GameUI)
            {
                UIManager.Instance.Close(this.UI.UiData.UiName);
            }
            else if (this.UI is ChildUI)
            {
                ChildUI childUi = this.UI as ChildUI;
                if (childUi.ParentUI != null)
                    childUi.ParentUI.CloseChildUI(this.UI.UiData.UiName);
            }
        }
    }
}
