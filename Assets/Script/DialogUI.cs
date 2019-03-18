using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIFramework;
using UnityEngine;
using UnityEngine.EventSystems;

[UI("DialogUI", UIType.Dialog, UIResType.Resorces, UICloseType.Destroy, true)]
public class DialogUI : UIMonoProxy
{
    public override void OnAwake()
    {
        Debug.Log("DialogUI OnAwake");
        RegisterListener("CancelButton", OnClickCancelBtn);
        RegisterListener("OkButton", OnClickOkBtn);
    }

    void OnClickCancelBtn(PointerEventData eventData)
    {
        UIManager.Instance.Close("DialogUI");
    }

    void OnClickOkBtn(PointerEventData eventData)
    {
        UIManager.Instance.Close("DialogUI");
    }

    public override void OnStart(params object[] args)
    {
        Debug.Log("DialogUI OnStart");
    }

    public override void OnEnable()
    {
        Debug.Log("DialogUI OnEnable");
    }

    public override void OnDisable()
    {
        Debug.Log("DialogUI OnDisable");
    }

    public override void OnDestroy()
    {
        Debug.Log("DialogUI OnDestroy");
    }
}
