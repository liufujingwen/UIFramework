using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using UIFramework;
using UnityEngine.EventSystems;

[UIAttribute("MainMenuUI", UIType.Normal, UIResType.Resorces, UICloseType.Destroy, true)]
public class MainMenuUI : UIMonoProxy
{
    public override void OnInit()
    {
        Debug.Log("MainMenuUI OnInit");
        RegisterListener("HighScoreButton", OnClickHighScoresBtn);
        RegisterListener("OptionButton", OnClickOptionBtn);
        RegisterListener("ExitButton", OnClickExitBtn);
    }

    void OnClickHighScoresBtn(PointerEventData eventData)
    {
        UIManager.Instance.Push("HighScoreUI");
    }

    void OnClickOptionBtn(PointerEventData eventData)
    {
        UIManager.Instance.Push("OptionMenuUI");
    }

    void OnClickExitBtn(PointerEventData eventData)
    {
        UIManager.Instance.Pop();
    }

    public override void OnEnter(params object[] args)
    {
        Debug.Log("MainMenuUI OnEnter");
    }

    public override void OnPause()
    {
        Debug.Log("MainMenuUI OnPause");
    }

    public override void OnResume()
    {
        Debug.Log("MainMenuUI OnResume");
    }

    public override void OnExit()
    {
        Debug.Log("MainMenuUI OnExit");
    }

    public override void OnNotifiy(string evt, params object[] args)
    {
    }
}