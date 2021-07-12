using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopInteractives : MonoBehaviour
{
    public GameObject shopButton;
    public GameObject childrenButton;
    public GameObject backButton;
    
    public GameObject shopFrame;

    public GameObject leftArrow;
    public GameObject rightArrow;

    public GameObject Blur;

    public Button GoToGlobe;

    public void BackClick()
    {
        shopButton.SetActive(true);
        childrenButton.SetActive(true);
        backButton.SetActive(false);
        Blur.SetActive(false);

        shopFrame.SetActive(false);
        leftArrow.SetActive(false);
        rightArrow.SetActive(false);    
    }

    public void ShopClick() 
    {
        shopButton.SetActive(false);
        childrenButton.SetActive(false);
        backButton.SetActive(true);
        Blur.SetActive(true);

        shopFrame.SetActive(true);
    }

    public void ChildClick()
    {
        shopButton.SetActive(false);
        childrenButton.SetActive(false);
        backButton.SetActive(true);
    }

    public void InventoryItemClick(InventorySlot item) 
    {
        if (!item.IsEmpty && item.Location == "Luggage") 
        {
            leftArrow.SetActive(true);
        }
        else if (!item.IsEmpty && item.Location == "Shop")
        {
            rightArrow.SetActive(true);
        }
    }

    public void GoToGlobeScene()
    {
        LevelManager.StartLevel("GlobeScene");
    }
}
