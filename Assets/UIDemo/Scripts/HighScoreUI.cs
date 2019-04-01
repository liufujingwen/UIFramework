using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using UIFramework;
using UnityEngine.EventSystems;

//[UIAttribute("HighScoreUI", UIType.Normal, UIResType.Resorces, UICloseType.Hide, true)]
public class HighScoreUI : UIMonoProxy
{
    public override void OnAwake()
    {
        Debug.Log("HighScoreUI OnAwake");
        RegisterListener("BackButton", OnClickExitBtn);
    }

    void OnClickExitBtn(PointerEventData eventData)
    {
        UIManager.Instance.Pop();
    }

    public override void OnStart(params object[] args)
    {
        Debug.Log("HighScoreUI OnStart");
    }

    public override void OnEnable()
    {
        Debug.Log("HighScoreUI OnEnable");
    }

    public override void OnDisable()
    {
        Debug.Log("HighScoreUI OnDisable");
    }

    public override void OnDestroy(bool delete)
    {
        Debug.Log("HighScoreUI OnDestroy");
    }
}