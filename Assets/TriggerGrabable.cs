using Oculus.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CustomGrabbableEvent : MonoBehaviour
{
    public Grabbable grabbable;

    public UnityEvent OnGrab;
    public UnityEvent OnGrabEnd;

    private void OnEnable()
    {
        grabbable.WhenPointerEventRaised += OnPointerEvent;
    }
    private void OnDisable()
    {
        grabbable.WhenPointerEventRaised -= OnPointerEvent;
    }

    private void OnPointerEvent(PointerEvent e) 
    {
        switch (e.Type)
        {
            case PointerEventType.Select:
                OnGrab.Invoke();
                break;
            case PointerEventType.Unselect:
                OnGrabEnd.Invoke();
                break;
            default:
                return;
        }
    }

}
