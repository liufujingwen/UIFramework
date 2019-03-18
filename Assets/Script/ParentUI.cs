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
        RegisterListener("OptionMenu", OnClickExitOptionMenu);
        RegisterListener("Tab1", OnClickTab1);
        RegisterListener("Tab2", OnClickTab2);
        RegisterListener("Tab3", OnClickTab3);

    }

    void OnClickExitBtn(PointerEventData eventData)
    {
        UIManager.Instance.Pop();
    }

    void OnClickExitOptionMenu(PointerEventData eventData)
    {
        UIManager.Instance.Open("OptionMenuUI");
    }

    void OnClickTab1(PointerEventData eventData)
    {
        CloseChildUI("ChildUI2");
        CloseChildUI("ChildUI3");
        OpenChildUI("ChildUI1");
    }

    void OnClickTab2(PointerEventData eventData)
    {
        CloseChildUI("ChildUI1");
        CloseChildUI("ChildUI3");
        OpenChildUI("ChildUI2");
    }

    void OnClickTab3(PointerEventData eventData)
    {
        CloseChildUI("ChildUI1");
        CloseChildUI("ChildUI2");
        OpenChildUI("ChildUI3");
    }

    public override void OnStart(params object[] args)
    {
        Debug.Log("ParentUI OnStart");
        OpenChildUI("ChildUI2");
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

