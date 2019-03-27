using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIFramework
{
    public interface IUIContainer
    {
        void Open(string uiName, Action<UI> callback, params object[] args);
        void Close(string uiName, Action callback);
        void Pop(Action action);
        void PopThenOpen(string uiName, params object[] args);
        void PopAllThenOpen(string uiName, params object[] args);
        void Remove(string uiName);
        void Clear();
    }
}
