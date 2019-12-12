namespace UIFramework
{
    public interface IUINotify
    {
        void OnAwake(UI ui);
        void OnStart(UI ui);
        void OnEnable(UI ui);
        void OnDisable(UI ui);
        void OnDestroy(UI ui);
    }
}
