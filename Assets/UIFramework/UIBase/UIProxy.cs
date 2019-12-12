using System;
using UnityEngine;

namespace UIFramework
{
    public abstract class UIProxy
    {
        public UI ui = null;
        public GameObject gameObject { get; set; }
        public Transform transform { get; set; }
        public string[] events { get; set; }

        public abstract void SetUi(UI ui);

        public virtual void SetGameObejct()
        {
            gameObject = ui.gameObject;
            transform = ui.transform;
        }

        public abstract string[] OnGetEvents();

        public abstract void OnAwake();

        public abstract void OnStart(params object[] args);

        public abstract void OnEnable();

        public abstract void OnDisable();

        public abstract void OnDestroy();

        public abstract void OnNotify(string evt, IEventArgs args);

        public GameObject FindGameObject(string name)
        {
            if (ui == null || !ui.transform)
                return null;
            return ui.transform.FindGameObject(name);
        }

        public Transform FindTransform(string name)
        {
            if (ui == null || !ui.transform)
                return null;
            return ui.transform.FindTransform(name);
        }

        public void RegisterListener(string name, UGUIEventListener.VoidDelegate handle, bool clear = true)
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

        public void RegisterListener(GameObject go, UGUIEventListener.VoidDelegate handle, bool clear = true)
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
            if (ui == null)
                return;

            GameUI gameUI = ui as GameUI;
            if (gameUI == null)
                return;

            gameUI.OpenChildUI(childUIName, args);
        }

        //只打开一个子UI，已显示的UI会被关闭
        public void OpenOneChildUI(string childUiName, params object[] args)
        {
            if (this.ui == null)
                return;

            GameUI gameUi = this.ui as GameUI;
            if (gameUi == null)
                return;

            gameUi.OpenOneChildUi(childUiName, args);
        }

        public void CloseChildUI(string childUIName)
        {
            GameUI gameUI = ui as GameUI;
            if (gameUI == null)
                return;

            gameUI.CloseChildUI(childUIName);
        }

        public void PlayAnimation(string animName, Action finishedCallback = null)
        {
            ui?.PlayAnimation(animName, finishedCallback);
        }

        public void Close()
        {
            if (this.ui == null)
                return;

            if (this.ui is GameUI)
            {
                UIManager.instance.Close(this.ui.uiData.uiName);
            }
            else if (this.ui is ChildUI)
            {
                ChildUI childUi = this.ui as ChildUI;
                if (childUi.parentUI != null)
                    childUi.parentUI.CloseChildUI(this.ui.uiData.uiName);
            }
        }
    }
}
