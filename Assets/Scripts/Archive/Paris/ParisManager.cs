using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ParisManager : MonoBehaviour
{
    public Canvas UI;
    public Button GlobeButton;

    public GameObject StartButton;
    public Button GuydeMaupassantTalkButton;
    public Button MattisTalkButton;
    public Button CholeraGuyTalkButton;
    public GameObject StartPopup;

    public GameObject MaupassantDialog1;
    public GameObject MaupassantDialog2;
    public GameObject MaupassantDialog3;
    public GameObject MaupassantDialog4;
    public GameObject MaupassantDialog5;
    public Button Maupassant1Button; // What is it?
    public Button Maupassant2Button; // Excuse me sir...
    public Button Maupassant3Button; // You mean Eiffel tower? I love it.
    public Button Maupassant4Button; // I hate it.
    public Button Maupassant5Button; // Oh thank you, I dont have enough space... I'll come back
    public Button Maupassant1EndButton; // Okay...
    public Button Maupassant2EndButton; // Thank you so much! + Ben Ami book

    public GameObject MattisDialog;
    public Button MattisEndButton;

    public GameObject CholeraDialog1;
    public GameObject CholeraDialog2;
    public GameObject CholeraDialog3;
    public Button CholeraButton1; // Is everything okay?
    public Button CholeraButton2; // I have to leave now. Sorry.
    public Button CholeraButton3; // Sure. (gift food or medicine)
    public Button CholeraButton4; // I'm sorry. I can't. you should go see a doctor.
    public Button EndCholeraButton; // X to end

    public GameObject InteractivesPanel;

    public GameObject Blur;

    private TimeSpan buttonAnimationTime;

    private static bool spokeToMaupassant;
    private static bool spokeToMattis;
    private static bool spokeToCholera;
    private static bool isInitialized;

    public GameObject ShopInventory;
    public GameObject CholeraGuyInventory;
    public GameObject MaupassantInventory;

    public void DiscoverHavre()
    {
        LevelManager.StartLevel("GlobeScene");
        GameManager.DiscoverLeg(CityData.Paris + CityData.Havre);
    }

    public void CloseInventories()
    {
        CloseShopInventory();
        CloseCholeraGuyInventory();
        CloseMaupassantInventory();
    }

    public void OpenShopInventory()
    {
        HideAll();
        StartButton.SetActive(true);
        ShopInventory.SetActive(true);
    }

    public void CloseShopInventory()
    {
        UnhideAll();
        StartButton.SetActive(false);
        ShopInventory.SetActive(false);
    }

    public void OpenCholeraGuyInventory()
    {
        HideAll();
        StartButton.SetActive(true);
        CholeraGuyInventory.SetActive(true);
    }

    public void CloseCholeraGuyInventory()
    {
        UnhideAll();
        StartButton.SetActive(false);
        CholeraGuyInventory.SetActive(false);
    }

    public void OpenMaupassantInventory()
    {
        HideAll();
        StartButton.SetActive(true);
        MaupassantInventory.SetActive(true);
    }

    public void CloseMaupassantInventory()
    {
        UnhideAll();
        StartButton.SetActive(false);
        MaupassantInventory.SetActive(false);
    }

    void Start()
    {
        if (isInitialized)
        {
            StartGame();
        }

        InteractivesPanel.SetActive(false);
        StartButton.SetActive(true);
        GlobeButton.onClick.AddListener(GoToGlobe);

        GuydeMaupassantTalkButton.onClick.AddListener(TalkToMaupassant);
        MattisTalkButton.onClick.AddListener(TalkToMattis);
        CholeraGuyTalkButton.onClick.AddListener(TalkToCholera);
        EndCholeraButton.onClick.AddListener(EndCholeraDialog);

        Maupassant1Button.onClick.AddListener(TalkToMaupassant2);
        Maupassant2Button.onClick.AddListener(TalkToMaupassant3);
        Maupassant3Button.onClick.AddListener(TalkToMaupassant4);
        Maupassant4Button.onClick.AddListener(TalkToMaupassant5);
        Maupassant1EndButton.onClick.AddListener(EndMaupassantDialog);
        Maupassant2EndButton.onClick.AddListener(EndMaupassantDialog);

        MattisTalkButton.onClick.AddListener(TalkToMattis);
        MattisEndButton.onClick.AddListener(EndMattisDialog);

        CholeraButton1.onClick.AddListener(TalkToCholera2);
        CholeraButton2.onClick.AddListener(StopTalkingToCholera);
        CholeraButton3.onClick.AddListener(TalkToCholera3);
        CholeraButton4.onClick.AddListener(StopTalkingToCholera);
        EndCholeraButton.onClick.AddListener(EndCholeraDialog);
    }

    private void GoToGlobe()
    {
        LevelManager.StartLevel("GlobeScene");
    }

    private void TalkToMaupassant()
    {
        HideAll();
        MaupassantDialog1.SetActive(true);
    }

    private void TalkToMaupassant2()
    {
        MaupassantDialog1.SetActive(false);
        MaupassantDialog2.SetActive(true);
    }

    private void TalkToMaupassant3()
    {
        MaupassantDialog2.SetActive(false);
        MaupassantDialog3.SetActive(true);
    }

    private void TalkToMaupassant4()
    {
        MaupassantDialog3.SetActive(false);
        MaupassantDialog4.SetActive(true);
    }

    private void TalkToMaupassant5()
    {
        MaupassantDialog3.SetActive(false);
        MaupassantDialog5.SetActive(true);
    }

    private void EndMaupassantDialog()
    {
        MaupassantDialog4.SetActive(false);
        MaupassantDialog5.SetActive(false);
        InteractivesPanel.transform.Find("MaupassantButton").gameObject.SetActive(false);

        spokeToMaupassant = true;
    }

    private void TalkToMattis()
    {
        HideAll();
        MattisDialog.SetActive(true);
    }

    private void EndMattisDialog()
    {
        MattisDialog.SetActive(false);
        InteractivesPanel.transform.Find("MattisButton").gameObject.SetActive(false);
        UnhideAll();

        spokeToMattis = true;

/*        GoToGlobe();
        GameManager.DiscoverLeg(CityData.Pfaffenthal + CityData.Luxembourg);*/
    }

    private void TalkToCholera()
    {
        HideAll();
        CholeraDialog1.SetActive(true);
    }

    private void TalkToCholera2()
    {
        CholeraDialog1.SetActive(false);
        CholeraDialog2.SetActive(true);
    }

    private void TalkToCholera3()
    {
        CholeraDialog2.SetActive(false);
        CholeraDialog3.SetActive(true);
    }

    private void StopTalkingToCholera() // Will still have ability to talk again.
    {
        CholeraDialog1.SetActive(false);
        CholeraDialog2.SetActive(false);
        CholeraDialog3.SetActive(false);
        UnhideAll();
    }

    private void EndCholeraDialog()
    {
        CholeraDialog3.SetActive(false);
        InteractivesPanel.transform.Find("CholeraButton").gameObject.SetActive(false);

        spokeToCholera = true;
    }

    private void HideAll()
    {
        InteractivesPanel.SetActive(false);
    }

    private void UnhideAll()
    {
        InteractivesPanel.SetActive(true);
    }



    public void StartGame()
    {
        isInitialized = true;

        StartPopup.SetActive(false);
        StartButton.SetActive(false);
        InteractivesPanel.SetActive(true);

        if (spokeToMaupassant)
        {
            InteractivesPanel.transform.Find("MaupassantButton").gameObject.SetActive(false);
        }

        if (spokeToMattis)
        {
            InteractivesPanel.transform.Find("MattisButton").gameObject.SetActive(false);
        }

        if (spokeToCholera)
        {
            InteractivesPanel.transform.Find("CholeraButton").gameObject.SetActive(false);
        }
    }

    public void CloseDialogs()
    {
        MaupassantDialog1.SetActive(false);
        MaupassantDialog2.SetActive(false);
        MaupassantDialog3.SetActive(false);
        MaupassantDialog4.SetActive(false);
        MaupassantDialog5.SetActive(false);
        MattisDialog.SetActive(false);
        CholeraDialog1.SetActive(false);
        CholeraDialog2.SetActive(false);
        CholeraDialog3.SetActive(false);

        UnhideAll();
    }

}
