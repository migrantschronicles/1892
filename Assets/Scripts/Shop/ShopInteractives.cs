using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopInteractives : MonoBehaviour
{

    public GameObject shopButton;
    public GameObject childrenButton;
    public GameObject backButton;
    
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

        shopFrame.SetActive(false);
        leftArrow.SetActive(false);
        rightArrow.SetActive(false);

        //childFrame.SetActive(false);

        
    }

    public void ShopClick() 
    {
        shopButton.SetActive(false);
        childrenButton.SetActive(false);
        backButton.SetActive(true);

        shopFrame.SetActive(true);

    }

    public void ChildClick()
    {
        shopButton.SetActive(false);
        childrenButton.SetActive(false);
        backButton.SetActive(true);

        //childFrame.SetActive(true);

    }
}
