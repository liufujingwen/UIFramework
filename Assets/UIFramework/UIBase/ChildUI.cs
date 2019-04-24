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

        public ChildUI(UIData uiData) : base(uiData)
        {
        }

        public override void Awake()
        {
            base.Awake();
        }

    }
}
