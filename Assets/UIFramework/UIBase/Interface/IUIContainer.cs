using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UIFramework
{
    public interface IUIContainer
    {
        void Open(string uiName, Action<UI> callback, params object[] args);
        void Open(UI ui, Action<UI> callback, params object[] args);
        void Close(string uiName, Action callback);
        void Pop(Action callback);
        void PopThenOpen(string uiName, params object[] args);
        void PopAllThenOpen(string uiName, params object[] args);
        void Remove(string uiName);
        void Clear();
        void SetUiParent(Transform parent, bool worldPositionStays);
        void OnNotifyAnimationFinish(Animator animator);//通知动画播放完成
    }
}
