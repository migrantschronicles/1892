using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PinchToZoomAndShrink : MonoBehaviour
{
    /*public float zoomSpeedPinch = 0.001f;
    public float zoomSpeedMouseScrollWheel = 0.05f;
    public float zoomMin = 0.1f;
    public float zoomMax = 1f;
    RectTransform rectTransform;
    public int type = 1; // for device testing type 1 use LateUpdate; type 2 use Update

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    //public void OnValueChanged(Vector2 v) // test failed: called by scroll view event
    //{
    //    //Zoom();
    //}

    void Update()
    {
        //Zoom();
        if (type == 2)
        {
            if (Input.touchCount == 2)
                StartCoroutine(ZoomInTheEndOfFrame(Input.mouseScrollDelta.y, Input.touchCount, Input.GetTouch(0), Input.GetTouch(1), Input.mousePosition));
            else
                StartCoroutine(ZoomInTheEndOfFrame(Input.mouseScrollDelta.y, Input.touchCount, default(Touch), default(Touch), Input.mousePosition));
        }
    }

    private void LateUpdate()
    {
        if (type == 1) Zoom();
    }

    void Zoom()
    {
        var mouseScrollWheel = Input.mouseScrollDelta.y;
        float scaleChange = 0f;
        Vector2 midPoint = Vector2.zero;
        if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            float deltaMagnitudeDiff = touchDeltaMag - prevTouchDeltaMag;

            scaleChange = deltaMagnitudeDiff * zoomSpeedPinch;

            midPoint = (touchOne.position + touchZero.position) / 2;
        }

        if (mouseScrollWheel != 0)
        {
            scaleChange = mouseScrollWheel * zoomSpeedMouseScrollWheel;
            midPoint = Input.mousePosition;
        }

        if (scaleChange != 0)
        {
            var scaleX = transform.localScale.x;
            scaleX += scaleChange;
            scaleX = Mathf.Clamp(scaleX, zoomMin, zoomMax);
            var size = rectTransform.rect.size;
            size.Scale(rectTransform.localScale);
            var parentRect = ((RectTransform)rectTransform.parent);
            var parentSize = parentRect.rect.size;
            parentSize.Scale(parentRect.localScale);
            if (size.x > parentSize.x && size.y > parentSize.y)
            {
                var p1 = Camera.main.ScreenToWorldPoint(midPoint);
                var p2 = transform.InverseTransformPoint(p1);
                var pivotP = rectTransform.pivot * rectTransform.rect.size;
                var p3 = (Vector2)p2 + pivotP;
                var newPivot = p3 / rectTransform.rect.size;
                newPivot = new Vector2(Mathf.Clamp01(newPivot.x), Mathf.Clamp01(newPivot.y));
                rectTransform.SetPivot(newPivot);
            }
            else
            {
                rectTransform.SetPivot(new Vector2(0.5f, 0.5f));
            }

            transform.localScale = new Vector3(scaleX, scaleX, transform.localScale.z);
        }
    }

    // please note that scrollRect is the component on the scroll view game object, not where this script is

    public void OnDrag(PointerEventData eventData)
    {
        Zoom();
        if (Input.touchCount <= 1) scrollRect.OnDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        scrollRect.OnEndDrag(eventData);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (Input.touchCount <= 1) scrollRect.OnBeginDrag(eventData);
    }
*/
    //private IEnumerator ZoomInTheEndOfFrame(float mouseScrollWheel, int touchCount, Touch touchZero, Touch touchOne, Vector3 mousePosition) // testing failed
    //{
    //    yield return new WaitForEndOfFrame();
    //    ZoomWithData(mouseScrollWheel, touchCount, touchZero, touchOne, mousePosition);
    //}

}