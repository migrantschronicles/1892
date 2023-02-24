using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapTransportationMethods : MonoBehaviour
{
    [SerializeField]
    private Image shipImage;
    [SerializeField]
    private Sprite shipIcon;
    [SerializeField]
    private Sprite ferryIcon;

    private MethodManager[] methods;
    private Vector3 initialScale;
    private MapZoom mapZoom;
    private RectTransform rectTransform;
    private Animator animator;

    public string ToLocation { get; private set; }

    private void Awake()
    {
        methods = GetComponentsInChildren<MethodManager>();
        rectTransform = GetComponent<RectTransform>();
        initialScale = rectTransform.localScale;
        mapZoom = GetComponentInParent<MapZoom>();
        mapZoom.onMapZoomChangedEvent += OnZoomChanged;
        OnZoomChanged(mapZoom.ZoomLevel);
        animator = GetComponent<Animator>();
    }

    private void OnDestroy()
    {
        if(mapZoom)
        {
            mapZoom.onMapZoomChangedEvent -= OnZoomChanged;
        }
    }

    private void OnZoomChanged(float zoomLevel)
    {
        float scaleFactor = mapZoom.DefaultZoomLevel / zoomLevel;
        rectTransform.localScale = scaleFactor * initialScale;
    }

    public void InitMethods(string from, string to)
    {
        ToLocation = to;
        foreach(MethodManager manager in methods)
        {
            if(NewGameManager.Instance.CanTravel(from, to, manager.Method))
            {
                TransportationRouteInfo info = NewGameManager.Instance.transportationInfo.GetRouteInfo(from, to, manager.Method);
                manager.SetRouteInfo(info);
                manager.gameObject.SetActive(true);
            }
            else
            {
                manager.gameObject.SetActive(false);
            }
        }

        bool useFerry = !NewGameManager.Instance.LocationManager.IsFromEuropeToAmerica(from, to);
        shipImage.sprite = useFerry ? ferryIcon : shipIcon;
        TransportationMethodBox box = shipImage.GetComponentInChildren<TransportationMethodBox>();
        box.SetUseFerry(useFerry);
    }

    public void Open()
    {
        animator.SetBool("Opened", true);
    }

    public void Close()
    {
        animator.SetBool("Opened", false);
    }

    public bool IsClosed()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName("Closed");
    }
}
