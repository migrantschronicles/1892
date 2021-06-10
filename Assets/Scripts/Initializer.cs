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
        map = WorldMapGlobe.instance;
        var countries = map.countries;
        //foreach(var c in countries)
        //{
        //    c.labelVisible = false;
        //}
        //map.DrawLine();
        //var allCountries = map.countries;
        //allCountries.ForEach(c => c.hidden = true);

        var list = map.GetCityIndex("Luxembourg");
        var list2 = map.GetCityIndex("Brussels");
        var list3 = map.GetCityIndex("Antwerpen"); //add
        var list4 = map.GetCityIndex("Rotterdam");
        var list5 = map.GetCityIndex("Metz");
        var list6 = map.GetCityIndex("Arlon");
        var list7 = map.GetCityIndex("Pfaffenthal"); //?

        CenterButton.onClick.AddListener(CenterCurrentPosition);

        StartButton.onClick.AddListener(FixPosition);
        map.OnCityClick += ChooseTransport;

        var luxIndex = map.GetCountryIndex("Luxembourg");
        var frIndex = map.GetCountryIndex("France");
        var grIndex = map.GetCountryIndex("Germany");
        var itIndex = map.GetCountryIndex("Italy");

        new CityManager(map).DrawLabels();
        map.rightButtonDragBehaviour = DRAG_BEHAVIOUR.None;

        CurrentCity.GetComponent<Text>().text = $"Current City: {map.GetCity("Luxembourg", "Luxembourg").name}";

        map.SetZoomLevel(0);
        map.FlyToLocation(map.GetCountry(luxIndex).latlonCenter);

        var marker = new NavigationMarker(map);

        foreach (var leg in CityData.CoordinatesByCity)
        {
            marker.MarkLeg(leg.Value);
            marker.TravelLeg(leg.Value, 20);
        }

        //var marker = new NavigationMarker(map);
        //marker.MarkPath(CityManager.CoordinatesByPath[(CityManager.Luxembourg, CityManager.Paris)], new Color(196f / 255f, 184f / 255f, 149f / 255f, 0.3f));
        //marker.TravelPath(CityManager.CoordinatesByPath[(CityManager.Luxembourg, CityManager.Paris)], new Color(240f / 255f, 210f / 255f, 122f / 255f), 10);
    }

    private void CenterCurrentPosition()
    {
        var initSpeed = map.navigationTime;

        map.navigationTime = 1;
        var luxIndex = map.GetCountryIndex("Luxembourg");
        map.FlyToLocation(map.GetCountry(luxIndex).latlonCenter).Then(() => map.ZoomTo(0, 1));
        map.ZoomTo(0, 1);

        map.navigationTime = initSpeed;
    }

    private void dd(LineMarkerAnimator lma)
    {
        throw new System.NotImplementedException();
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

        //var marker = new NavigationMarker(map);
        //marker.TravelPath(Cities.CoordinatesByPath[(Cities.Brussels, Cities.Antwerp)], new Color(240f / 255f, 210f / 255f, 122f / 255f), 10);
    }

    public void DrawLine()
    {
        // map.AddLine(prevPos, nextPos, Color.red, 1, map.navigationTime, 1, 1);
    }    

    void Update()
    {
        var luxIndex = map.GetCountryIndex("Luxembourg");
        var frIndex = map.GetCountryIndex("France");

        var startLine = map.GetCountry(luxIndex).latlonCenter;
        var endLine = map.GetCountry(frIndex).latlonCenter;

        //map.AddLine(startLine, endLine, Color.red, 0, 0, 0.001f, 0);
        //map.AddLine(startLine, endLine, Color.green, 0, 10, 0.001f, 0);

        //map.AddText("bla", map.GetCountry(luxIndex).sphereCenter, Color.red);
    }
}
