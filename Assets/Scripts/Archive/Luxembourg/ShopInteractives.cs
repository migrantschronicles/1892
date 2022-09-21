using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopInteractives : MonoBehaviour
{
    private static bool isStarted;

    public GameObject shopButton;
    public GameObject childrenButton;
    public GameObject backButton;
    public GameObject ticketButton;
    public GameObject agentButton;

    public GameObject AgentPopupInfo;

    public GameObject TicketSetting;

    public GameObject shopFrame;

    //public GameObject leftArrow;
    //public GameObject rightArrow;

    public GameObject Blur;
    public GameObject StartBlur;

    public GameObject StartPanel;

    public Button GoToGlobe;

    public void Start()
    {
        StartBlur.SetActive(true);
        StartPanel.SetActive(true);
        backButton.SetActive(true);
        shopButton.SetActive(false);
        agentButton.SetActive(false);
        childrenButton.SetActive(false);
        ticketButton.SetActive(false);

        if (isStarted)
        {
            BackClick();
        }

        if(isTicketEnabled)
        {
            TicketClick();
        }
    }

    public void AgentButtonClick()
    {
        AgentPopupInfo.SetActive(true);
        backButton.SetActive(true);

        shopButton.SetActive(false);
        agentButton.SetActive(false);

        if (isChDialogActive)
        {
            childrenButton.SetActive(false);
        }
        else
        {
            ticketButton.SetActive(false);
        }
    }

    public void BackClick()
    {
        StartBlur.SetActive(false);
        StartPanel.SetActive(false);

        shopButton.SetActive(true);
        agentButton.SetActive(true);

        if (isChDialogActive)
        {
            childrenButton.SetActive(true);
        }
        else
        {
            ticketButton.SetActive(true);
        }

        backButton.SetActive(false);
        Blur.SetActive(false);

        shopFrame.SetActive(false);
        //leftArrow.SetActive(false);
        //rightArrow.SetActive(false);

        AgentPopupInfo.SetActive(false);

        CloseChAll();

        isStarted = true;
    }

    public void ShopClick()
    {
        shopButton.SetActive(false);
        agentButton.SetActive(false);
        childrenButton.SetActive(false);
        ticketButton.SetActive(false);
        backButton.SetActive(true);
        Blur.SetActive(true);
        //leftArrow.SetActive(true);
        //rightArrow.SetActive(true);

        shopFrame.SetActive(true);
    }

    public void ChildClick()
    {
        shopButton.SetActive(false);
        agentButton.SetActive(false);
        childrenButton.SetActive(false);
        ticketButton.SetActive(false);
        backButton.SetActive(true);

        GotToChDialog1();
    }

    public void InventoryItemClick(OldInventorySlot item)
    {
        if (!item.IsEmpty && item.Location == "Luggage")
        {
            //leftArrow.SetActive(true);
        }
        else if (!item.IsEmpty && item.Location == "Shop")
        {
            //rightArrow.SetActive(true);
        }
    }

    public void GoToGlobeScene()
    {
        LevelManager.StartLevel("GlobeScene");
    }

    #region Ticket Scene Interaction

    public GameObject Dialog1;
    public GameObject Dialog2;
    public GameObject Dialog3;
    public GameObject Dialog4;
    public GameObject Dialog5;
    public GameObject Dialog6;

    private static bool isRouteDiscovered;
    private static bool isTicketEnabled;
    private static int dialogIndex = 0;

    public void TicketClick()
    {
        TicketSetting.SetActive(true);

        CloseAll();

        if(dialogIndex == 3)
        {
            Dialog3.SetActive(true);
        }
        else if (dialogIndex == 4)
        {
            Dialog4.SetActive(true);
        }
        else if (isRouteDiscovered)
        {
            Dialog5.SetActive(true);
        }
        else
        {
            Dialog1.SetActive(true);
        }

        isTicketEnabled = true;
    }

    public void TicketBackClick()
    {
        CloseAll();

        TicketSetting.SetActive(false);

        isTicketEnabled = false;
        dialogIndex = 0;
    }

    public void GotToDialog2()
    {
        CloseAll();

        Dialog2.SetActive(true);
    }

    public void GotToDialog3()
    {
        CloseAll();

        isRouteDiscovered = true;
        Dialog3.SetActive(true);
        dialogIndex = 3;

        LevelManager.StartLevel("GlobeScene");
        GameManager.DiscoverLeg(CityData.Luxembourg + CityData.Paris);
    }

    public void GotToDialog4()
    {
        CloseAll();

        isRouteDiscovered = true;
        Dialog4.SetActive(true);
        dialogIndex = 4;

        LevelManager.StartLevel("GlobeScene");
        GameManager.DiscoverLeg(CityData.Luxembourg + CityData.Brussels);
    }

    public void GotToDialog5()
    {
        CloseAll();

        Dialog5.SetActive(true);
    }

    public void GotToDialog6()
    {
        CloseAll();

        Dialog6.SetActive(true);
    }

    private void CloseAll()
    {
        Dialog1.SetActive(false);
        Dialog2.SetActive(false);
        Dialog3.SetActive(false);
        Dialog4.SetActive(false);
        Dialog5.SetActive(false);
        Dialog6.SetActive(false);
    }

    #endregion

    #region Children Dialog

    public GameObject ChDialog1;
    public GameObject ChDialog2;
    public GameObject ChDialog3;
    public GameObject ChDialog4;

    private static bool isChDialogActive = true;

    public void GotToChDialog1()
    {
        CloseChAll();
        ChDialog1.SetActive(true);
    }

    public void GotToChDialog2()
    {
        CloseChAll();
        ChDialog2.SetActive(true);
    }

    public void GotToChDialog3()
    {
        CloseChAll();
        ChDialog3.SetActive(true);
    }

    public void GotToChDialog4()
    {
        CloseChAll();
        ChDialog4.SetActive(true);
        isChDialogActive = false;
    }

    private void CloseChAll()
    {
        ChDialog1.SetActive(false);
        ChDialog2.SetActive(false);
        ChDialog3.SetActive(false);
        ChDialog4.SetActive(false);
    }

    #endregion
}
