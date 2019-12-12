using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

public class UGUIEventListener : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler
{
    public delegate void VoidDelegate(PointerEventData eventData);
    public VoidDelegate onClick;
    public VoidDelegate onDown;
    public VoidDelegate onEnter;
    public VoidDelegate onExit;
    public VoidDelegate onUp;
    public VoidDelegate onSelect;
    public VoidDelegate onUpdateSelect;
    static Func<PointerEventData, bool> ms_GuideHandle;

    public static Func<PointerEventData, bool> guideHandle
    {
        get { return ms_GuideHandle; }
        set { ms_GuideHandle = value; }
    }

    static public UGUIEventListener Get(GameObject go)
    {
        UGUIEventListener listener = go.GetComponent<UGUIEventListener>();
        if (listener == null) listener = go.AddComponent<UGUIEventListener>();
        return listener;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (ms_GuideHandle != null && !ms_GuideHandle(eventData))
            return;

        if (onClick != null && !eventData.dragging)
            onClick(eventData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (onDown != null) onDown(eventData);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (onEnter != null) onEnter(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (onExit != null) onExit(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (onUp != null) onUp(eventData);
    }

    //public void OnSelect(BaseEventData eventData)
    //{
    //    if (onSelect != null) onSelect(eventData);
    //}

    //public void OnUpdateSelected(BaseEventData eventData)
    //{
    //    if (onUpdateSelect != null) onUpdateSelect(eventData);
    //}

    public void SetEventNull()
    {
        onClick = null;
    }

    public void Coroutine(IEnumerator routine)
    {
        StartCoroutine(routine);
    }
}
