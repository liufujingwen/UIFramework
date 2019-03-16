using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIFramework
{
    public class ChildUI : UI
    {
        public GameUI ParentUI = null;

        public ChildUI(string uiName, GameUI parentUI)
        {
            this.ParentUI = parentUI;
            this.ParentUI.AddChildUI(uiName, this);
        }

        public override void Awake()
        {
            base.Awake();
        }
       
    }
}
