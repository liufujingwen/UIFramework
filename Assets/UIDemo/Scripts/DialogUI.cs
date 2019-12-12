using UIFramework;
using UnityEngine;
using UnityEngine.EventSystems;

[UI("DialogUI", UIType.Dialog, UIResType.Resorces, true)]
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
        UIManager.instance.Close("DialogUI");
        UIManager.instance.Remove("MainMenuUI");
    }

    void OnClickOkBtn(PointerEventData eventData)
    {
        UIManager.instance.Close("DialogUI");
        UIManager.instance.Open("MainMenuUI");
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
