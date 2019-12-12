using UIFramework;
using UnityEngine;
using UnityEngine.EventSystems;

[UIChildAttribute("ChildUI1", "ParentUI", false, UIResType.Resorces, false)]
public class ChildUI1 : UIMonoProxy
{
    public override void OnAwake()
    {
        Debug.Log("ChildUI1 OnAwake");
        RegisterListener("BackButton", OnClickExitBtn);
    }

    void OnClickExitBtn(PointerEventData eventData)
    {
        UIManager.instance.Pop();
    }

    public override void OnStart(params object[] args)
    {
        Debug.Log("ChildUI1 OnStart");
    }

    public override void OnEnable()
    {
        Debug.Log("ChildUI1 OnEnable");
    }

    public override void OnDisable()
    {
        Debug.Log("ChildUI1 OnDisable");
    }

    public override void OnDestroy()
    {
        Debug.Log("ChildUI1 OnDestroy");
    }
}

