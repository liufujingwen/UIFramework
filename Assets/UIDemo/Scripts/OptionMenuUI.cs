using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using UIFramework;
using UnityEngine.EventSystems;

[UIAttribute("OptionMenuUI", UIType.Normal, UIResType.Resorces, UICloseType.Destroy, true)]
public class OptionMenuUI : UIMonoProxy
{
    public override void OnAwake()
    {
        Debug.Log("OptionMenuUI OnAwake");
        RegisterListener("BackButton", OnClickExitBtn);
    }

    void OnClickExitBtn(PointerEventData eventData)
    {
        UIManager.Instance.Pop();
    }

    public override void OnStart(params object[] args)
    {
        Debug.Log("OptionMenuUI OnStart");
    }

    public override void OnEnable()
    {
        Debug.Log("OptionMenuUI OnEnable");
    }

    public override void OnDisable()
    {
        Debug.Log("OptionMenuUI OnDisable");
    }

    public override void OnDestroy()
    {
        Debug.Log("OptionMenuUI OnDestroy");
    }
}