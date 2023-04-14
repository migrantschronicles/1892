using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeveloperLocationButton : MonoBehaviour
{
    public string sceneName;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            AudioManager.Instance.StopMusic();
            Destroy(AudioManager.Instance.gameObject);
            Destroy(NewGameManager.Instance.gameObject);
            /*
            for(int i = 0; i < AudioManager.Instance.transform.childCount; i++)
            {
                Destroy(AudioManager.Instance.transform.GetChild(i).gameObject);
            }
            Destroy(AudioManager.Instance.gameObject);
            */
            SceneManager.LoadScene(sceneName);
        });
    }
}
