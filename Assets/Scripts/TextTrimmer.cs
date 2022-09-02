using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextTrimmer : MonoBehaviour
{
    public List<GameObject> textParts;

    public GameObject NextButton;
    public GameObject PreviousButton;

    public float timeForCharacters = 0.1f;

    private int count;
    private int index;
    private string tempText;
    private bool completedText = false;

    void Start()
    {
        count = textParts.Count;

        PreviousButton.SetActive(false);
        NextButton.SetActive(true);

        NextButton.GetComponent<Button>().onClick.AddListener(NextButtonClick);
        PreviousButton.GetComponent<Button>().onClick.AddListener(PreviousButtonClick);

        StartCoroutine(AnimateText());
    }

    public void Update() 
    {
        if (Input.GetMouseButtonDown(0) && !completedText) 
        {
            CompleteText();
        }
    }

    public void NextButtonClick()
    {
        CompleteText();

        this.gameObject.transform.localPosition = new Vector3(this.gameObject.transform.localPosition.x, Mathf.Lerp(this.gameObject.transform.localPosition.y, this.gameObject.transform.localPosition.y+100, Time.deltaTime *1), this.gameObject.transform.localPosition.z);

        // NEED TO FIX THIS

        textParts[index].SetActive(false);

        if (++index == count - 1)
        {
            NextButton.SetActive(false);
        }

        PreviousButton.SetActive(true);
        textParts[index].SetActive(true);

        StartCoroutine(AnimateText());
    }

    public void PreviousButtonClick()
    {
        CompleteText();

        textParts[index].SetActive(false);

        if (--index == 0)
        {
            PreviousButton.SetActive(false);
        }

        NextButton.SetActive(true);
        textParts[index].SetActive(true);

        StartCoroutine(AnimateText());
    }

    public IEnumerator AnimateText() 
    {
        completedText = false;
        tempText = textParts[index].GetComponent<Text>().text;
        textParts[index].GetComponent<Text>().text = "";

        for (int i = 0; i < tempText.Length; i++) 
        {
            textParts[index].GetComponent<Text>().text += tempText[i];
            yield return new WaitForSeconds(timeForCharacters);
        }
        completedText = true;
    }

    public void CompleteText() 
    {
        if (tempText != "" || tempText!=null)
        {
            textParts[index].GetComponent<Text>().text = tempText;
            tempText = "";
        }
        completedText = true;
    }
}
