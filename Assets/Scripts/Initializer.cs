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

    public Canvas UICanvas;

    public GameObject Popup;

    int lastClickedCity;

    Vector2 prevPos;
    Vector2 nextPos;

    GameObject panel;

    void Start()
    {
        map = WorldMapGlobe.instance;
        var countries = map.countries;
        foreach(var c in countries)
        {
            c.labelVisible = false;
        }
        //map.DrawLine();
        //var allCountries = map.countries;
        //allCountries.ForEach(c => c.hidden = true);

        StartButton.onClick.AddListener(FixPosition);
        map.OnCityClick += ChooseTransport;
    }

    void FixPosition()
    {
        map.allowUserRotation = false;
        map.allowUserZoom = false;

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

        // panel = new GameObject("Panel");
        // panel.AddComponent<CanvasRenderer>();

        // var rt = panel.transform;
        // rt.localScale = new Vector3(5f, 4f, 1f);

        // Image i = panel.AddComponent<Image>();
        // i.color = new Color(237, 242, 239, 0.5f);

        // GameObject b1 = new GameObject();
        // b1.transform.parent = panel.transform;
        // b1.AddComponent<RectTransform>();
        // b1.AddComponent<Button>();
        // b1.AddComponent<Text>();
        // b1.transform.position = new Vector3(10, 10, 10);
        // b1.GetComponent<RectTransform>().localScale = new Vector3(2,2,2);
        // b1.GetComponent<Button>().onClick.AddListener(FlyToCity);
        // b1.transform.SetParent(panel.transform, false);
        // var t = b1.GetComponent<Button>().GetComponentInChildren<Text>();


        // var b1_ = b1.GetComponent<Button>();
        // t.text = "Transport 1";
        // var t1 = b1.GetComponentInChildren<Text>().text = "Transport 1";

        // GameObject b2 = new GameObject();
        // b2.transform.parent = panel.transform;
        // b2.AddComponent<RectTransform>();
        // b2.AddComponent<Button>();
        // b2.AddComponent<Text>();
        // b2.GetComponentInChildren<Text>().text = "Transport 2";
        // b2.transform.position = new Vector3(40, 10, 10);
        // b2.GetComponent<RectTransform>().localScale = new Vector3(2, 2, 2);
        // b2.GetComponent<Button>().onClick.AddListener(FlyToCity);
        // b2.transform.SetParent(panel.transform, false);

        Popup.transform.SetParent(UICanvas.transform, true);
    }

    public void FlyToCity()
    {
        Popup.transform.parent = null;

        map.FlyToCity(lastClickedCity);
    }

    public void DrawLine()
    {
        // map.AddLine(prevPos, nextPos, Color.red, 1, map.navigationTime, 1, 1);
    }    

    void Update()
    {
        
    }
}
