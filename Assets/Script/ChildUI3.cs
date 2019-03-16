using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIFramework;
using UnityEngine;
using UnityEngine.EventSystems;

[UIChildAttribute("ChildUI3", "ParentUI", false, UIResType.Resorces, UICloseType.Hide, false)]
public class ChildUI3 : UIMonoProxy
{
    public override void OnAwake()
    {
        Debug.Log("ChildUI3 OnAwake");
        RegisterListener("BackButton", OnClickExitBtn);
    }

    void OnClickExitBtn(PointerEventData eventData)
    {
        UIManager.Instance.Pop();
    }

    public override void OnStart(params object[] args)
    {
        Debug.Log("ChildUI3 OnStart");
    }

    public override void OnEnable()
    {
        Debug.Log("ChildUI3 OnEnable");
    }

    public override void OnDisable()
    {
        Debug.Log("ChildUI3 OnDisable");
    }

    public override void OnDestroy()
    {
        Debug.Log("ChildUI3 OnDestroy");
    }
}

