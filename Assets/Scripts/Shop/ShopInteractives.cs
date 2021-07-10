using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopInteractives : MonoBehaviour
{

    public GameObject shopButton;
    public GameObject childrenButton;
    public GameObject backButton;
    public GameObject blurryBG;
    
    public GameObject shopFrame;
    public GameObject childFrame;

    public GameObject leftArrow;
    public GameObject rightArrow;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void BackClick()
    {
        shopButton.SetActive(true);
        childrenButton.SetActive(true);
        backButton.SetActive(false);
        blurryBG.SetActive(false);

        shopFrame.SetActive(false);
        leftArrow.SetActive(false);
        rightArrow.SetActive(false);

        childFrame.SetActive(false);

        
    }

    public void ShopClick() 
    {
        shopButton.SetActive(false);
        childrenButton.SetActive(false);
        backButton.SetActive(true);
        blurryBG.SetActive(true);

        shopFrame.SetActive(true);

    }

    public void ChildClick()
    {
        shopButton.SetActive(false);
        childrenButton.SetActive(false);
        backButton.SetActive(true);
        blurryBG.SetActive(true);

        childFrame.SetActive(true);

    }

    public void InventoryItemClick(InventoryItem item) 
    {
        if (!item.empty && item.location == "Luggage") 
        {
            leftArrow.SetActive(true);
        }
        else if (!item.empty && item.location == "Shop")
        {
            rightArrow.SetActive(true);
        }
    }
}
