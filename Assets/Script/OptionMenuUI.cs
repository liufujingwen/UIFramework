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
    public override void OnInit()
    {
        Debug.Log("OptionMenuUI OnInit");
        RegisterListener("BackButton", OnClickExitBtn);
    }

    void OnClickExitBtn(PointerEventData eventData)
    {
        UIManager.Instance.Pop();
    }

    public override void OnEnter(params object[] args)
    {
        Debug.Log("OptionMenuUI OnEnter");
    }

    public override void OnPause()
    {
        Debug.Log("OptionMenuUI OnPause");
    }

    public override void OnResume()
    {
        Debug.Log("OptionMenuUI OnResume");
    }

    public override void OnExit()
    {
        Debug.Log("OptionMenuUI OnExit");
    }
}