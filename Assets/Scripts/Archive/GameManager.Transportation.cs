using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public partial class GameManager : MonoBehaviour
{
    public Button AddCustomTransportationButton;
    public Button HideCustomTransportationButton;

    public RectTransform TransportationFormPrefab;

    public RectTransform TrainPrefab;
    public RectTransform TramPrefab;
    public RectTransform StageCoachPrefab;
    public RectTransform BoatPrefab;
    public RectTransform CartPrefab;
    public RectTransform FootPrefab;

    private RectTransform activeForm;
    private RectTransform activeDiscoverPanel;
    private RectTransform activeIllustration;

    public Button TransportationButtonPrefab;
    private List<Button> transportationButtonsToDestroy = new List<Button>();

    private int currentTransportationComponent = 0;

    public RectTransform DiscoveryPanelPrefab;
    public Button OpenDiscoveryPanelButton;
    public Button CloseDiscoveryPanelButton;

    public void OpenCustomTransportationForm()
    {
        activeForm = Instantiate(TransportationFormPrefab, Vector3.zero, Quaternion.identity);

        var width = activeForm.gameObject.GetComponent<RectTransform>().rect.width;
        var height = activeForm.gameObject.GetComponent<RectTransform>().rect.height;

        var canvasWidth = UICanvas.gameObject.GetComponent<RectTransform>().rect.width;
        var canvasHeight = UICanvas.gameObject.GetComponent<RectTransform>().rect.height;

        activeForm.transform.position = new Vector3((canvasWidth - width) / 2, (canvasHeight - height) / 2, 0);
        activeForm.transform.SetParent(UICanvas.transform);
    }

    public void HideCustomTransportationForm()
    {
        if(activeForm != null)
        {
            Destroy(activeForm.gameObject);
        }
    }

    private void ConstructTransportationOptions()
    {
        var legKey = origin?.name + destination?.name;

        if (origin != null && destination != null && LegData.CoordinatesByLegKey.ContainsKey(legKey) && TransportationData.TransportationByLegKey.ContainsKey(legKey))
        {
            //if (NavigationMarker.IsLegMarked(legKey))
            {
                float buttonWidth = 0;
                float buttonHeight = 0;

                foreach (var type in TransportationData.TransportationByLegKey[legKey])
                {
                    var button = Instantiate(TransportationButtonPrefab, Vector3.zero, Quaternion.identity);

                    button.onClick.AddListener(delegate
                    {
                        if(StateManager.CurrentState.AvailableMoney < TransportationData.TransportationCostByType[type])
                        {
                            return;
                        }

                        foreach (var button in transportationButtonsToDestroy)
                        {
                            Destroy(button.gameObject);
                        }

                        transportationButtonsToDestroy.Clear();

                        TravelLeg(origin, destination, type);
                    });

                    if (buttonWidth == 0 || buttonHeight == 0)
                    {
                        buttonWidth = button.gameObject.GetComponent<RectTransform>().rect.width;
                        buttonHeight = button.gameObject.GetComponent<RectTransform>().rect.height;
                    }

                    var iconImage = button.transform.Find("Icon").transform.GetComponent<Image>();
                    var typeText = button.transform.Find("Type").transform.GetComponent<Text>();
                    var costText = button.transform.Find("Cost").transform.GetComponentInChildren<Text>();
                    var luggageSpaceText = button.transform.Find("LuggageSpace").transform.GetComponentInChildren<Text>();
                    var durationText = button.transform.Find("Duration").transform.GetComponent<Text>();

                    if (iconImage != null)
                    {
                        iconImage.sprite = Resources.Load<Sprite>($"TransportationResources/Icons/{TransportationData.TransportationIconByType[type].Name}");
                        //iconImage.rectTransform.sizeDelta = TransportationData.TransportationIconByType[type].Size;
                    }

                    if (typeText != null)
                    {
                        typeText.text = type.ToString();
                    }

                    if (costText != null)
                    {
                        costText.text = $"{TransportationData.TransportationCostByType[type]}";
                    }

                    if (luggageSpaceText != null)
                    {
                        luggageSpaceText.text = $"{TransportationData.TransportationSpaceByType[type]}";
                    }

                    if (durationText != null)
                    {
                        var distance = 0;//NavigationMarker.GetDistance(LegData.CoordinatesByLegKey[legKey].First(), LegData.CoordinatesByLegKey[legKey].Last());
                        var time = TimeSpan.FromHours(distance / 1000 / TransportationData.TransportationSpeedByType[type]);
                        var timeString = time.Days > 0 ? $"{time.Days}d {time.Hours}h {time.Minutes}m" : $"{time.Hours}h {time.Minutes}m";
                        durationText.text = timeString;
                    }

                    transportationButtonsToDestroy.Add(button);
                }

                var count = TransportationData.TransportationByLegKey[legKey].Count();

                if (CustomTransportationBehaviour.CustomTransportationByLegKey.ContainsKey(legKey))
                {
                    count += CustomTransportationBehaviour.CustomTransportationByLegKey[legKey].Count;

                    foreach (var custom in CustomTransportationBehaviour.CustomTransportationByLegKey[legKey])
                    {
                        var button = Instantiate(TransportationButtonPrefab, Vector3.zero, Quaternion.identity);

                        button.onClick.AddListener(delegate
                        {
                            foreach (var button in transportationButtonsToDestroy)
                            {
                                Destroy(button.gameObject);
                            }

                            transportationButtonsToDestroy.Clear();

                            TravelCustomLeg(origin, destination, custom);
                        });

                        var iconImage = button.transform.Find("Icon").transform.GetComponent<Image>();
                        var typeText = button.transform.Find("Type").transform.GetComponent<Text>();
                        var costText = button.transform.Find("Cost").transform.GetComponentInChildren<Text>();
                        var luggageSpaceText = button.transform.Find("LuggageSpace").transform.GetComponentInChildren<Text>();
                        var durationText = button.transform.Find("Duration").transform.GetComponent<Text>();

                        if (iconImage != null)
                        {
                            iconImage.sprite = Resources.Load<Sprite>($"TransportationResources/Icons/{custom.IconName}");
                        }

                        if (typeText != null)
                        {
                            typeText.text = custom.Type;
                        }

                        if (costText != null)
                        {
                            costText.text = $"{custom.Cost}";
                        }

                        if (luggageSpaceText != null)
                        {
                            luggageSpaceText.text = $"{custom.Luggage}";
                        }

                        if (durationText != null)
                        {
                            var distance = 0;// NavigationMarker.GetDistance(LegData.CoordinatesByLegKey[legKey].First(), LegData.CoordinatesByLegKey[legKey].Last());
                            var time = TimeSpan.FromHours(distance / 1000 / custom.Speed);
                            var timeString = time.Days > 0 ? $"{time.Days}d {time.Hours}h {time.Minutes}m" : $"{time.Hours}h {time.Minutes}m";
                            durationText.text = timeString;
                        }

                        transportationButtonsToDestroy.Add(button);
                    }
                }

                var canvasWidth = UICanvas.gameObject.GetComponent<RectTransform>().rect.width;
                var canvasHeight = UICanvas.gameObject.GetComponent<RectTransform>().rect.height;
                var margin = 15;
                var index = 0;

                foreach (var button in transportationButtonsToDestroy)
                {
                    button.transform.position = new Vector3((canvasWidth - count * (buttonWidth + margin)) / 2 + index++ * (buttonWidth + margin), (canvasHeight - buttonHeight) / 2, 0);
                    button.transform.SetParent(UICanvas.transform);
                }
            }
        }
    }

    private void DisplayTransportationIllustration(TransportationType type)
    {
        RectTransform illustrationPrefab = null;

        switch(type)
        {
            case TransportationType.Train:
                illustrationPrefab = TrainPrefab;
                break;
            case TransportationType.Boat:
                illustrationPrefab = BoatPrefab;
                break;
            case TransportationType.TramRail:
                illustrationPrefab = TramPrefab;
                break;
            case TransportationType.StageCoach:
                illustrationPrefab = StageCoachPrefab;
                break;
            case TransportationType.Cart:
                illustrationPrefab = CartPrefab;
                break;
            case TransportationType.Foot:
                illustrationPrefab = FootPrefab;
                break;
            default:
                break;
        }

        if(illustrationPrefab != null)
        {
            var canvasWidth = UICanvas.gameObject.GetComponent<RectTransform>().rect.width;

            activeIllustration = Instantiate(illustrationPrefab, new Vector3(canvasWidth, 0, 0), Quaternion.identity);
            activeIllustration.transform.SetParent(UICanvas.transform);
        }
    }

    private void ShowNextTransportationIllustration()
    {
        DestroyTransportationIllustration();

        var index = currentTransportationComponent % Enum.GetNames(typeof(TransportationType)).Length;
        var name = Enum.GetName(typeof(TransportationType), index);
        
        if(Enum.TryParse(name, out TransportationType nextType))
        {
            DisplayTransportationIllustration(nextType);
            currentTransportationComponent++;
        }
    }

    private void DestroyTransportationIllustration()
    {
        if (activeIllustration != null)
        {
            Destroy(activeIllustration.gameObject);
        }
    }

    private void OpenDiscoveryPanel()
    {
        activeDiscoverPanel = Instantiate(DiscoveryPanelPrefab, Vector3.zero, Quaternion.identity);

        var width = activeDiscoverPanel.gameObject.GetComponent<RectTransform>().rect.width;
        var height = activeDiscoverPanel.gameObject.GetComponent<RectTransform>().rect.height;

        var canvasWidth = UICanvas.gameObject.GetComponent<RectTransform>().rect.width;
        var canvasHeight = UICanvas.gameObject.GetComponent<RectTransform>().rect.height;

        activeDiscoverPanel.transform.SetParent(UICanvas.transform);
        activeDiscoverPanel.transform.localPosition = Vector3.zero;
    }

    private void CloseDiscoveryPanel()
    {
        if(activeDiscoverPanel != null)
        {
            Destroy(activeDiscoverPanel.gameObject);
        }
    }
}
