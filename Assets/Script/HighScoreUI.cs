using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using UIFramework;
using UnityEngine.EventSystems;

[UIAttribute("HighScoreUI", UIType.Normal, UIResType.Resorces, UICloseType.Hide, true)]
public class HighScoreUI : UIMonoProxy
{
    public override void OnInit()
    {
        Debug.Log("HighScoreUI OnInit");
        RegisterListener("BackButton", OnClickExitBtn);
    }

    void OnClickExitBtn(PointerEventData eventData)
    {
        UIManager.Instance.Pop();
    }

    public override void OnEnter(params object[] args)
    {
        Debug.Log("HighScoreUI OnEnter");
    }

    public override void OnPause()
    {
        Debug.Log("HighScoreUI OnPause");
    }

    public override void OnResume()
    {
        Debug.Log("HighScoreUI OnResume");
    }

    public override void OnExit()
    {
        Debug.Log("HighScoreUI OnExit");
    }
}