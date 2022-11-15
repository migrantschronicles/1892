using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupManager : MonoBehaviour
{

    private NewGameManager gameManager;
    public GameObject endDayPopup;
    public GameObject startDayHostelPopup;
    public GameObject startDayOutsidePopup;
    public GameObject foodDistributePopup;
    public GameObject endGamePopup;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<NewGameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenEndDayPopUp() 
    {
        endDayPopup.SetActive(true);
        gameManager.gameRunning = false;
    }

    public void OpenStartDayHostelPopUp()
    {
        startDayHostelPopup.SetActive(true);
        gameManager.gameRunning = false;
    }

    public void OpenStartDayOutsidePopUp()
    {
        startDayOutsidePopup.SetActive(true);
        gameManager.gameRunning = false;
    }

    public void OpenFoodDistributePopUp()
    {
        foodDistributePopup.SetActive(true);
        gameManager.gameRunning = false;
    }

    public void OpenEndGamePopUp()
    {
        endGamePopup.SetActive(true);
        gameManager.gameRunning = false;
    }


}
