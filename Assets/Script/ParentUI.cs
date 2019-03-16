using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIFramework;
using UnityEngine;
using UnityEngine.EventSystems;

[UIAttribute("ParentUI", UIType.Normal, UIResType.Resorces, UICloseType.Destroy, false)]
public class ParentUI : UIMonoProxy
{
    public override void OnAwake()
    {
        Debug.Log("ParentUI OnAwake");
        RegisterListener("BackButton", OnClickExitBtn);
    }

    void OnClickExitBtn(PointerEventData eventData)
    {
        UIManager.Instance.Pop();
    }

    public override void OnStart(params object[] args)
    {
        Debug.Log("ParentUI OnStart");
    }

    public override void OnEnable()
    {
        Debug.Log("ParentUI OnEnable");
    }

    public override void OnDisable()
    {
        Debug.Log("ParentUI OnDisable");
    }

    public override void OnDestroy()
    {
        Debug.Log("ParentUI OnDestroy");
    }
}

