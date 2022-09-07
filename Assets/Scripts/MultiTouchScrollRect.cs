using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MultiTouchScrollRect : ScrollRect
{
    // https://stackoverflow.com/questions/56221113/fix-for-scrollrect-multi-touch-in-unity
    private int minimumTouchCount = 1;
    private int maximumTouchCount = 2;
    private int initialTouchCount = 0;

    public Vector2 MultiTouchPosition
    {
        get
        {
            Vector2 position = Vector2.zero;
            for(int i = 0; i < Input.touchCount && i < maximumTouchCount; ++i)
            {
                position += Input.touches[i].position;
            }
            position /= ((Input.touchCount <= maximumTouchCount) ? Input.touchCount : maximumTouchCount);
            return position;
        }
    }

    /**
     * Checks if the device is mobile (or remote connected).
     */
    private bool IsHandheld()
    {
        if (SystemInfo.deviceType == DeviceType.Handheld)
        {
            return true;
        }

#if UNITY_EDITOR
        if(UnityEditor.EditorApplication.isRemoteConnected)
        {
            return true;
        }
#endif

        return false;
    }

    private void Update()
    {
        if (IsHandheld())
        {
            if(Input.touchCount > 0)
            {
                if(initialTouchCount == 0)
                {
                    initialTouchCount = Input.touchCount;
                }
            }
            else
            {
                initialTouchCount = 0;
            }
        }
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        if(IsHandheld())
        {
            if(Input.touchCount >= minimumTouchCount && Input.touchCount == initialTouchCount)
            {
                eventData.position = MultiTouchPosition;
                base.OnBeginDrag(eventData);
            }
        }
        else if(SystemInfo.deviceType == DeviceType.Desktop)
        {
            base.OnBeginDrag(eventData);
        }
    }

    public override void OnDrag(PointerEventData eventData)
    {
        if(IsHandheld())
        {
            if(Input.touchCount >= minimumTouchCount && Input.touchCount == initialTouchCount)
            {
                eventData.position = MultiTouchPosition;
                base.OnDrag(eventData);
            }
        }
        else if(SystemInfo.deviceType == DeviceType.Desktop)
        {
            base.OnDrag(eventData);
        }
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        if(IsHandheld())
        {
            if(Input.touchCount >= minimumTouchCount)
            {
                eventData.position = MultiTouchPosition;
                base.OnEndDrag(eventData);
            }
        }
        else if(SystemInfo.deviceType == DeviceType.Desktop)
        {
            base.OnEndDrag(eventData);
        }
    }
}
