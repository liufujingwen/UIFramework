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
        UIManager.Instance.Remove("MainMenuUI");
    }

    void OnClickOkBtn(PointerEventData eventData)
    {
        UIManager.Instance.Close("DialogUI");
        UIManager.Instance.Open("MainMenuUI");
    }

    public override void OnStart(params object[] args)
    {
        Debug.Log("DialogUI OnStart");
        for (int i = 0; i < args.Length; i++)
        {
            Debug.Log($"DialogUI OnStart agrs[{i}]={args[i]}");
        }
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
