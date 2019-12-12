using UnityEngine;
using UIFramework;
using UnityEngine.EventSystems;

[UIAttribute("MainMenuUI", UIType.Normal, UIResType.Resorces, true)]
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
        UIManager.instance.Open("ParentUI");
    }

    void OnClickHighScoresBtn(PointerEventData eventData)
    {
        UIManager.instance.Open("HighScoreUI");
    }

    void OnClickOptionBtn(PointerEventData eventData)
    {
        UIManager.instance.Open("OptionMenuUI");
    }

    void OnClickExitBtn(PointerEventData eventData)
    {
        UIManager.instance.Pop();
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

    public override void OnNotify(string evt, IEventArgs args)
    {
    }
}