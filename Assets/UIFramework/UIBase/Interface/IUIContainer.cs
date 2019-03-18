using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIFramework
{
    public interface IUIContainer
    {
        void Open(string uiName, params object[] args);
        void Close(string uiName);
        void Pop();
        void Remove(string uiName);
        void Clear();
    }
}
