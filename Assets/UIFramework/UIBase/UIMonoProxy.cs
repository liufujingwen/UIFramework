using UnityEngine;

namespace UIFramework
{
    public class UIMonoProxy : UIProxy
    {
        public override void SetUi(UI ui)
        {
            this.ui = ui;
            events = OnGetEvents();
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
            if (ui == null || ui == null || !ui.gameObject)
                return null;
            return ui.gameObject.FindComponent<T>(name);
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
