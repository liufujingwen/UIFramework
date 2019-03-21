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
        Task OpenAsync(string uiName, params object[] args);
        void Close(string uiName);
        Task CloseAsync(string uiName);
        void Pop();
        Task PopAsync();
        Task PopThenOpenAsync(string uiName, params object[] args);
        void Remove(string uiName);
        void Clear();
    }
}
