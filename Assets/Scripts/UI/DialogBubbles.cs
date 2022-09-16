using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogBubbles : MonoBehaviour
{
    public bool hasPrompt = false; // Checked true if has a prompt (decision) in the end to be done.
    public List<GameObject> textParts;
    public int bubblesIndex = -1;


    public float timeForCharacters = 0.1f;

    public int count;
    //private int index;
    public string tempText;
    public bool completedText = false;

    void Start()
    {
        if (this.gameObject.transform.parent.tag == "Content")
            this.gameObject.transform.parent.transform.localPosition = new Vector3(gameObject.transform.parent.transform.localPosition.x, -100f, gameObject.transform.parent.transform.localPosition.z);

        count = textParts.Count;

        /*        PreviousButton.SetActive(false);
                NextButton.SetActive(true);*/

        /*        NextButton.GetComponent<Button>().onClick.AddListener(NextButtonClick);
                PreviousButton.GetComponent<Button>().onClick.AddListener(PreviousButtonClick);
        */
        DisplayNextBubble();
    }

    public void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
           // if (!completedText)
         //       CompleteText();
       //     else
           DisplayNextBubble();
        }
    }

    public void DisplayNextBubble() 
    {
        if (bubblesIndex < count-1) { 
            bubblesIndex++;
            textParts[bubblesIndex].SetActive(true);
        }
        //   StartCoroutine(AnimateText());
    }

/*    public void NextButtonClick()
    {
        CompleteText();

        this.gameObject.transform.localPosition = new Vector3(this.gameObject.transform.localPosition.x, Mathf.Lerp(this.gameObject.transform.localPosition.y, this.gameObject.transform.localPosition.y + 100, Time.deltaTime * 1), this.gameObject.transform.localPosition.z);

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
    }*/

    public IEnumerator AnimateText()
    {
        completedText = false;
        tempText = textParts[bubblesIndex].GetComponent<Text>().text;
        textParts[bubblesIndex].GetComponent<Text>().text = "";

        for (int i = 0; i < tempText.Length; i++)
        {
            textParts[bubblesIndex].GetComponent<Text>().text += tempText[i];
            yield return new WaitForSeconds(timeForCharacters);
        }
        completedText = true;
    }

    public void CompleteText()
    {
        if (tempText != "" || tempText != null)
        {
            textParts[bubblesIndex].GetComponent<Text>().text = tempText;
            tempText = "";
        }
        completedText = true;
    }
}
