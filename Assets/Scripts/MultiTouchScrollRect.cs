using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MultiTouchScrollRect : ScrollRect
{
    // https://stackoverflow.com/questions/56221113/fix-for-scrollrect-multi-touch-in-unity
    // https://bitbucket.org/UnityUIExtensions/unity-ui-extensions/wiki/Controls/MultiTouchScrollRect
    // Store the id of the touch.
    private int pid = -100;

    public override void OnBeginDrag(PointerEventData eventData)
    {
        if(Input.touchCount == 1)
        {
            // Only store the id of the first touch
            pid = eventData.pointerId;
            base.OnBeginDrag(eventData);
        }
        else
        {
            pid = -100;
            base.StopMovement();
        }
    }

    public override void OnDrag(PointerEventData eventData)
    {
        if(pid == eventData.pointerId)
        {
            // Only move if it's only one touch
            base.OnDrag(eventData);
        }
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        pid = -100;
        base.OnEndDrag(eventData);
    }
}
