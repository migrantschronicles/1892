using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextTrimmer : MonoBehaviour
{
    public List<GameObject> textParts;

    public GameObject NextButton;
    public GameObject PreviousButton;

    private int count;
    private int index;

    void Start()
    {
        count = textParts.Count;

        PreviousButton.SetActive(false);
        NextButton.SetActive(true);

        NextButton.GetComponent<Button>().onClick.AddListener(NextButtonClick);
        PreviousButton.GetComponent<Button>().onClick.AddListener(PreviousButtonClick);
    }

    public void NextButtonClick()
    {
        textParts[index].SetActive(false);

        if (++index == count - 1)
        {
            NextButton.SetActive(false);
        }

        PreviousButton.SetActive(true);
        textParts[index].SetActive(true);
    }

    public void PreviousButtonClick()
    {
        textParts[index].SetActive(false);

        if (--index == 0)
        {
            PreviousButton.SetActive(false);
        }

        NextButton.SetActive(true);
        textParts[index].SetActive(true);
    }
}
