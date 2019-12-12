namespace UIFramework
{
    public class ChildUI : UI
    {
        public GameUI parentUI { get; set; }

        public ChildUI(UIData uiData) : base(uiData)
        {
        }

        public override void Awake()
        {
            base.Awake();
        }

    }
}
