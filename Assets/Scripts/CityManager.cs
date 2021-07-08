using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CityManager : MonoBehaviour
{
    public Canvas UI;
    public Button GlobeButton;

    public Button StartButton;
    public Button KatrinTalkButton;
    public Button JhangTalkButton;
    public RawImage StartPopup;

    public RectTransform KatrinDialog;
    public RectTransform KatrinDialog1;
    public RectTransform KatrinDialog2;
    public Button KatrinDialog1Button;
    public Button KatrinDialog1EndButton;
    public Button KatrinDialog2Button;
    public Button KatrinDialog2EndButton;

    public RectTransform JhangDialog;
    public RectTransform JhangDialog1;
    public Button JhangDialog1Button;
    public Button JhangDialog1EndButton;

    public RectTransform Family;
    public RectTransform Katrin;
    public RectTransform Jhang;

    private TimeSpan buttonAnimationTime;
    private const string startButtonText = "click to start";

    private static bool spokeToKatrin;
    private static bool spokeToJhang;
    private static bool isInitialized;

    void Start()
    {
        if(isInitialized)
        {
            StartGame();
        }

        StartButton.onClick.AddListener(StartGame);
        GlobeButton.onClick.AddListener(GoToGlobe);

        KatrinTalkButton.onClick.AddListener(TalkToKatrin);
        JhangTalkButton.onClick.AddListener(TalkToJhang);

        KatrinDialog1Button.onClick.AddListener(TalkToKatrin1);
        KatrinDialog1EndButton.onClick.AddListener(EndKatrinDialog1);

        KatrinDialog2Button.onClick.AddListener(TalkToKatrin2);
        KatrinDialog2EndButton.onClick.AddListener(EndKatrinDialog2);

        JhangDialog1Button.onClick.AddListener(TalkToJhang1);
        JhangDialog1EndButton.onClick.AddListener(EndJhangDialog1);
    }

    private void GoToGlobe()
    {
        LevelManager.StartLevel("GlobeScene");
    }

    private void TalkToKatrin()
    {
        HideAll();
        KatrinDialog.SetParent(UI.transform);
    }

    private void TalkToKatrin1()
    {
        KatrinDialog.SetParent(null);
        KatrinDialog1.SetParent(UI.transform);
    }

    private void TalkToKatrin2()
    {
        KatrinDialog.SetParent(null);
        KatrinDialog2.SetParent(UI.transform);
    }

    private void EndKatrinDialog1()
    {
        KatrinDialog1.SetParent(null);
        UnhideAll();
    }

    private void EndKatrinDialog2()
    {
        KatrinDialog2.SetParent(null);
        Katrin.GetComponentInChildren<Button>().transform.SetParent(null);
        UnhideAll();

        spokeToKatrin = true;
    }

    private void TalkToJhang()
    {
        HideAll();
        JhangDialog.SetParent(UI.transform);
    }

    private void TalkToJhang1()
    {
        JhangDialog.SetParent(null);
        JhangDialog1.SetParent(UI.transform);
    }

    private void EndJhangDialog1()
    {
        JhangDialog1.SetParent(null);
        Jhang.GetComponentInChildren<Button>().transform.SetParent(null);
        UnhideAll();

        spokeToJhang = true;

        GoToGlobe();
        GameManager.DiscoverLeg(CityData.Pfaffenthal + CityData.Luxembourg);
    }

    private void HideAll()
    {
        Family.transform.SetParent(null);
        Katrin.transform.SetParent(null);
        Jhang.transform.SetParent(null);
    }

    private void UnhideAll()
    {
        Family.transform.SetParent(UI.transform);
        Katrin.transform.SetParent(UI.transform);
        Jhang.transform.SetParent(UI.transform);
    }

    void Update()
    {
        AnimateStartButton();
        AnimateTalkButton();
    }

    private void StartGame()
    {
        StateManager.CurrentState.FreezeTime = false;
        isInitialized = true;

        StartPopup.transform.SetParent(null);
        Katrin.SetParent(UI.transform);
        Jhang.SetParent(UI.transform);

        if(spokeToKatrin)
        {
            Katrin.GetComponentInChildren<Button>().transform.SetParent(null);
        }

        if(spokeToJhang)
        {
            Jhang.GetComponentInChildren<Button>().transform.SetParent(null);
        }
    }

    private void AnimateStartButton()
    {
        bool isEmpty = string.IsNullOrEmpty(StartButton.GetComponentInChildren<Text>().text);

        if (isEmpty && buttonAnimationTime > TimeSpan.FromMilliseconds(350) || !isEmpty && buttonAnimationTime > TimeSpan.FromMilliseconds(800))
        {
            StartButton.GetComponentInChildren<Text>().text = isEmpty ? startButtonText : null;
            buttonAnimationTime = TimeSpan.Zero;
        }
        else
        {
            buttonAnimationTime += TimeSpan.FromSeconds(Time.deltaTime);
        }
    }

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
