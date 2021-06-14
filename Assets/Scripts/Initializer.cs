using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using WPM;

public class Initializer : MonoBehaviour
{
    public WorldMapGlobe map;

    public Button StartButton;
    public Button CenterButton;

    public Text CurrentCity;

    public Canvas UICanvas;

    public GameObject Popup;

    int lastClickedCity;

    Vector2 prevPos;
    Vector2 nextPos;

    GameObject panel;

    void Start()
    {
        map.OnCityClick += ChooseTransport;
    }

    void FixPosition()
    {
        var luxIndex = map.GetCountryIndex("Luxembourg");

        map.FlyToCountry(luxIndex).Then(() => map.ZoomTo(0f));

        prevPos = map.GetCountry(luxIndex).latlonCenter;
    }

    void ChooseTransport(int cityIndex)
    {
        if (nextPos != null)
        {
            prevPos = nextPos;
        }

        lastClickedCity = cityIndex;
        nextPos = map.GetCity(lastClickedCity).latlon;

        Popup.transform.SetParent(UICanvas.transform, true);
    }

    void Update()
    {
    }
}
