﻿using UnityEngine;
using UIFramework;
using UnityEngine.EventSystems;

[UIAttribute("OptionMenuUI", UIType.Normal, UIResType.Resorces, true)]
public class OptionMenuUI : UIMonoProxy
{
    public override void OnAwake()
    {
        Debug.Log("OptionMenuUI OnAwake");
        RegisterListener("BackButton", OnClickExitBtn);
    }

    void OnClickExitBtn(PointerEventData eventData)
    {
        UIManager.instance.Pop();
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