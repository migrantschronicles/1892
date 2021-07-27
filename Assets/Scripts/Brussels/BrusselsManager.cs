using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class BrusselsManager : MonoBehaviour
{

    public Canvas UI;
    public Button GlobeButton;
    public GameObject CloseUp;

    public GameObject StartButton;
    public Button FredericTalkButton;
    public Button JeanTalkButton;
    public Button AdolpheTalkButton;
    public GameObject StartPopup;

    public GameObject JeanDialog0;
    public GameObject JeanDialog1;
    public GameObject JeanDialog2;
    public GameObject JeanDialog3;
    public GameObject JeanDialog4;
    public GameObject JeanDialog5;
    public GameObject JeanDialog6;
    public GameObject JeanDialog7;
    public GameObject JeanDialog8;
    public GameObject JeanDialog9;
    public Button Jean1Button;
    public Button Jean2Button;
    public Button Jean3Button;
    public Button Jean4Button;
    public Button Jean5Button;
    public Button Jean6Button;
    public Button Jean7Button;
    public Button Jean8Button;
    public Button Jean9Button;
    public Button Jean10Button;
    public Button Jean1EndButton; 
    public Button Jean2EndButton; 

    public GameObject FredericDialog1;
    public GameObject FredericDialog2;
    public GameObject FredericDialog3;
    public Button Frederic1Button;
    public Button Frederic2Button;
    public Button FredericEndButton1;
    public Button FredericEndButton2;

    public GameObject AdolpheDialog1;
    public GameObject AdolpheDialog2;
    public GameObject AdolpheDialog3;
    public GameObject AdolpheDialog4;
    public Button AdolpheButton1; 
    public Button AdolpheButton2; 
    public Button AdolpheButton3; 
    public Button EndAdolpheButton;

    public GameObject Mother;
    public GameObject Boy;
    public GameObject Girl;

    public GameObject NPCPanel;

    public GameObject Blur;

    private TimeSpan buttonAnimationTime;

    private static bool spokeToFrederic;
    private static bool spokeToJean;
    private static bool spokeToAdolphe;
    private static bool isInitialized;

    void Start()
    {
        if (isInitialized)
        {
            StartGame();
        }
        NPCPanel.SetActive(false);
        StartButton.SetActive(true);
        GlobeButton.onClick.AddListener(GoToGlobe);

        JeanTalkButton.onClick.AddListener(TalkToJean);
        FredericTalkButton.onClick.AddListener(TalkToFrederic1);
        AdolpheTalkButton.onClick.AddListener(TalkToAdolphe1);

        Jean1Button.onClick.AddListener(TalkToJean2);
        Jean2Button.onClick.AddListener(TalkToJean3);
        Jean3Button.onClick.AddListener(TalkToJean7);
        Jean4Button.onClick.AddListener(TalkToJean4);
        Jean5Button.onClick.AddListener(TalkToJean9);
        Jean6Button.onClick.AddListener(TalkToJean5);
        Jean7Button.onClick.AddListener(TalkToJean6);
        /*Jean8Button.onClick.AddListener();*/
        Jean9Button.onClick.AddListener(TalkToJean8);
        /*Jean10Button.onClick.AddListener();*/
        Jean1EndButton.onClick.AddListener(EndJeanDialog);
        Jean2EndButton.onClick.AddListener(EndJeanDialog);

        Frederic1Button.onClick.AddListener(TalkToFrederic2);
        Frederic2Button.onClick.AddListener(TalkToFrederic3);
        FredericEndButton1.onClick.AddListener(EndFredericDialog);
        FredericEndButton2.onClick.AddListener(EndFredericDialog);

        AdolpheButton1.onClick.AddListener(TalkToAdolphe2);
        AdolpheButton2.onClick.AddListener(TalkToAdolphe3);
        AdolpheButton3.onClick.AddListener(TalkToAdolphe4);
        EndAdolpheButton.onClick.AddListener(EndAdolpheDialog);
    }

    private void GoToGlobe()
    {
        LevelManager.StartLevel("GlobeScene");
    }

    private void TalkToJean() 
    {
        if (spokeToFrederic)
        {
            JeanDialog1.SetActive(true);
        }
        else
        {
            JeanDialog0.SetActive(true);
        }
    }

    private void TalkToJean2() 
    {
        JeanDialog1.SetActive(false);
        JeanDialog2.SetActive(true);
    }

    private void TalkToJean3()
    {
        JeanDialog2.SetActive(false);
        JeanDialog3.SetActive(true);
    }

    private void TalkToJean4()
    {
        JeanDialog3.SetActive(false);
        JeanDialog4.SetActive(true);
    }

    private void TalkToJean5()
    {
        JeanDialog4.SetActive(false);
        JeanDialog5.SetActive(true);
    }

    private void TalkToJean6()
    {
        JeanDialog5.SetActive(false);
        JeanDialog6.SetActive(true);
    }

    private void TalkToJean7()
    {
        JeanDialog2.SetActive(false);
        JeanDialog7.SetActive(true);
    }

    private void TalkToJean8()
    {
        JeanDialog7.SetActive(false);
        JeanDialog8.SetActive(true);
    }

    private void TalkToJean9()
    {
        JeanDialog3.SetActive(false);
        JeanDialog9.SetActive(true);
    }

    private void EndJeanDialog()
    {
        JeanDialog6.SetActive(false);
        JeanDialog8.SetActive(false);
        NPCPanel.transform.Find("JeanButton").gameObject.SetActive(false);
        UnhideAll();

        spokeToJean = true;
    }

    private void TalkToFrederic1()
    {
        HideAll();
        CloseUp.SetActive(true);
        FredericDialog1.SetActive(true);
    }

    private void TalkToFrederic2()
    {
        FredericDialog1.SetActive(false);
        FredericDialog2.SetActive(true);
    }

    private void TalkToFrederic3()
    {
        FredericDialog1.SetActive(false);
        FredericDialog3.SetActive(true);
    }

    private void EndFredericDialog()
    {
        FredericDialog3.SetActive(false);
        FredericDialog2.SetActive(false);
        NPCPanel.transform.Find("FredericButton").gameObject.SetActive(false);
        UnhideAll();

        spokeToFrederic = true;
        CloseUp.SetActive(false);
    }

    private void TalkToAdolphe1()
    {
        HideAll();
        CloseUp.SetActive(true);
        AdolpheDialog1.SetActive(true);
    }

    private void TalkToAdolphe2()
    {
        AdolpheDialog1.SetActive(false);
        AdolpheDialog2.SetActive(true);
    }

    private void TalkToAdolphe3()
    {
        AdolpheDialog2.SetActive(false);
        AdolpheDialog3.SetActive(true);
    }

    private void TalkToAdolphe4()
    {
        AdolpheDialog3.SetActive(false);
        AdolpheDialog4.SetActive(true);
    }

    private void EndAdolpheDialog()
    {
        AdolpheDialog4.SetActive(false);
        NPCPanel.transform.Find("AdolpheSaxButton").gameObject.SetActive(false);
        UnhideAll();

        spokeToAdolphe = true;
        CloseUp.SetActive(false);
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



    public void StartGame()
    {
        StateManager.CurrentState.FreezeTime = false;
        isInitialized = true;

        StartPopup.SetActive(false);
        StartButton.SetActive(false);
        NPCPanel.SetActive(true);

        if (spokeToFrederic)
        {
            NPCPanel.transform.Find("FredericButton").gameObject.SetActive(false);
        }

        if (spokeToJean)
        {
            NPCPanel.transform.Find("JeanButton").gameObject.SetActive(false);
        }

        if (spokeToAdolphe)
        {
            NPCPanel.transform.Find("AdolpheSaxButton").gameObject.SetActive(false);
        }

        Blur.SetActive(false);
    }

    public void CloseDialogs()
    {
        JeanDialog0.SetActive(false);
        JeanDialog1.SetActive(false);
        JeanDialog2.SetActive(false);
        JeanDialog3.SetActive(false);
        JeanDialog4.SetActive(false);
        JeanDialog5.SetActive(false);
        JeanDialog6.SetActive(false);
        JeanDialog7.SetActive(false);
        JeanDialog8.SetActive(false);
        JeanDialog9.SetActive(false);
        FredericDialog1.SetActive(false);
        FredericDialog2.SetActive(false);
        FredericDialog3.SetActive(false);
        AdolpheDialog1.SetActive(false);
        AdolpheDialog2.SetActive(false);
        AdolpheDialog3.SetActive(false);
        AdolpheDialog4.SetActive(false);
        CloseUp.SetActive(false);

        UnhideAll();
    }
}
