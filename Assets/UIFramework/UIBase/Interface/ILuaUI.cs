namespace UIFramework
{
    public interface ILuaUI
    {
        string[] OnGetEvents();
        void SetGameObject();

        void OnAwake();

        void OnStart(params object[] args);

        void OnEnable();

        void OnDisable();

        void OnDestroy();

        void OnNotify(string evt, params object[] args);
    }
}
