using UIFramework;
using UnityEngine;
using UnityEngine.EventSystems;

[UIChildAttribute("ChildUI2", "ParentUI", true, UIResType.SetGameObject, true)]
public class ChildUI2 : UIMonoProxy
{
    public override void OnAwake()
    {
        Debug.Log("ChildUI2 OnAwake");
        RegisterListener("BackButton", OnClickExitBtn);
    }

    void OnClickExitBtn(PointerEventData eventData)
    {
        UIManager.instance.Pop();
    }

    public override void OnStart(params object[] args)
    {
        Debug.Log("ChildUI2 OnStart");
    }

    public override void OnEnable()
    {
        Debug.Log("ChildUI2 OnEnable");
    }

    public override void OnDisable()
    {
        Debug.Log("ChildUI2 OnDisable");
    }

    public override void OnDestroy()
    {
        Debug.Log("ChildUI2 OnDestroy");
    }
}

