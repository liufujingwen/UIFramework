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

        public override void Awake()
        {
            base.Awake();
        }

    }
}
