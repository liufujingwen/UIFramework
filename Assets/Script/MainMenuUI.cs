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
    public override void OnAwake()
    {
        Debug.Log("MainMenuUI OnInit");
        RegisterListener("ParentUIButton", OnClickParentUIButton);
        RegisterListener("HighScoreButton", OnClickHighScoresBtn);
        RegisterListener("OptionButton", OnClickOptionBtn);
        RegisterListener("ExitButton", OnClickExitBtn);
    }

    void OnClickParentUIButton(PointerEventData eventData)
    {
        UIManager.Instance.Push("ParentUI");
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

    public override void OnStart(params object[] args)
    {
        Debug.Log("MainMenuUI OnStart");
    }

    public override void OnEnable()
    {
        Debug.Log("MainMenuUI OnEnable");
    }

    public override void OnDisable()
    {
        Debug.Log("MainMenuUI OnDisable");
    }

    public override void OnDestroy()
    {
        Debug.Log("MainMenuUI OnDestroy");
    }

    public override void OnNotifiy(string evt, params object[] args)
    {
    }
}