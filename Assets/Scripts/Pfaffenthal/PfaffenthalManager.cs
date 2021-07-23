using System;
using UnityEngine;
using UnityEngine.UI;

public class PfaffenthalManager : MonoBehaviour
{
    public Canvas UI;
    public Button GlobeButton;

    public GameObject StartButton;
    public Button KatrinTalkButton;
    public Button JhangTalkButton;
    public GameObject StartPopup;

    public GameObject KatrinDialog;
    public GameObject KatrinDialog1;
    public GameObject KatrinDialog2;
    public Button KatrinDialog1Button;
    public Button KatrinDialog2Button;
    public Button KatrinDialog2EndButton;

    public GameObject JhangDialog;
    public GameObject JhangDialog1;
    public Button JhangDialog1Button;
    public Button JhangDialog1EndButton;

    public GameObject Mother;
    public GameObject Boy;
    public GameObject Girl;

    public GameObject NPCPanel;

    public GameObject Inventory;

    public GameObject Blur;

    private TimeSpan buttonAnimationTime;

    private static bool spokeToKatrin;
    private static bool spokeToJhang;
    private static bool isInitialized;

    void Start()
    {
        if(isInitialized)
        {
            StartGame();
        }

        StartButton.SetActive(true);
        GlobeButton.onClick.AddListener(GoToGlobe);

        KatrinTalkButton.onClick.AddListener(TalkToKatrin);
        JhangTalkButton.onClick.AddListener(TalkToJhang);

        KatrinDialog1Button.onClick.AddListener(TalkToKatrin1);

        KatrinDialog2Button.onClick.AddListener(TalkToKatrin2);
        KatrinDialog2EndButton.onClick.AddListener(EndKatrinDialog2);

        JhangDialog1Button.onClick.AddListener(TalkToJhang1);
        JhangDialog1EndButton.onClick.AddListener(EndJhangDialog1);

        KatrinDialog.SetActive(false);
        KatrinDialog1.SetActive(false);
        KatrinDialog2.SetActive(false);
        JhangDialog.SetActive(false);
        JhangDialog1.SetActive(false);
    }

    private void GoToGlobe()
    {
        LevelManager.StartLevel("GlobeScene");
    }

    private void TalkToKatrin()
    {
        HideAll();
        KatrinDialog.SetActive(true);
    }

    private void TalkToKatrin1()
    {
        KatrinDialog.SetActive(false);
        KatrinDialog1.SetActive(true);
    }

    private void TalkToKatrin2()
    {
        KatrinDialog.SetActive(false);
        KatrinDialog2.SetActive(true);
    }

    private void EndKatrinDialog2()
    {
        KatrinDialog2.SetActive(false);
        NPCPanel.transform.Find("KatrinButton").gameObject.SetActive(false);
        UnhideAll();

        spokeToKatrin = true;
    }

    private void TalkToJhang()
    {
        HideAll();
        JhangDialog.SetActive(true);
    }

    private void TalkToJhang1()
    {
        JhangDialog.SetActive(false);
        JhangDialog1.SetActive(true);
    }

    private void EndJhangDialog1()
    {
        JhangDialog1.SetActive(false);
        NPCPanel.transform.Find("JhangButton").gameObject.SetActive(false);
        UnhideAll();

        spokeToJhang = true;

        GoToGlobe();
        GameManager.DiscoverLeg(CityData.Pfaffenthal + CityData.Luxembourg);
    }

    private void HideAll()
    {
        Mother.SetActive(false);
        Boy.SetActive(false);
        Girl.SetActive(false);
        NPCPanel.SetActive(false);
    }

    private void UnhideAll()
    {
        Mother.SetActive(true);
        Boy.SetActive(true);
        Girl.SetActive(true);
        NPCPanel.SetActive(true);
    }

    void Update()
    {
    }

    public void StartGame()
    {
        StateManager.CurrentState.FreezeTime = false;
        isInitialized = true;

        StartPopup.SetActive(false);
        StartButton.SetActive(false);
        NPCPanel.SetActive(true);
        Inventory.SetActive(true);

        if (spokeToKatrin)
        {
            NPCPanel.transform.Find("KatrinButton").gameObject.SetActive(false);
        }

        if(spokeToJhang)
        {
            NPCPanel.transform.Find("JhangButton").gameObject.SetActive(false);
        }

        Blur.SetActive(false);
    }

    public void CloseDialogs()
    {
        KatrinDialog.SetActive(false);
        KatrinDialog1.SetActive(false);
        KatrinDialog2.SetActive(false);
        JhangDialog.SetActive(false);
        JhangDialog1.SetActive(false);

        UnhideAll();
    }

    //obsolete
    private void AnimateStartButton()
    {
        bool isEmpty = string.IsNullOrEmpty(StartButton.GetComponentInChildren<Text>().text);

        if (isEmpty && buttonAnimationTime > TimeSpan.FromMilliseconds(350) || !isEmpty && buttonAnimationTime > TimeSpan.FromMilliseconds(800))
        {
            StartButton.GetComponentInChildren<Text>().text = isEmpty ? "" : null;
            buttonAnimationTime = TimeSpan.Zero;
        }
        else
        {
            buttonAnimationTime += TimeSpan.FromSeconds(Time.deltaTime);
        }
    }

    //obsolete
    private void AnimateTalkButton()
    {
        bool isSmall = KatrinTalkButton.GetComponentInChildren<Image>().rectTransform.sizeDelta.x != 70;

        if (isSmall && buttonAnimationTime > TimeSpan.FromMilliseconds(800) || !isSmall && buttonAnimationTime > TimeSpan.FromMilliseconds(800))
        {
            KatrinTalkButton.GetComponentInChildren<Image>().rectTransform.sizeDelta = isSmall ? new Vector2(70, 70) : new Vector2(66, 66);
            JhangTalkButton.GetComponentInChildren<Image>().rectTransform.sizeDelta = isSmall ? new Vector2(70, 70) : new Vector2(66, 66);
            buttonAnimationTime = TimeSpan.Zero;
        }
        else
        {
            buttonAnimationTime += TimeSpan.FromSeconds(Time.deltaTime);
        }
    }
}
