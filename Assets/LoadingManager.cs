using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour
{

    private bool loadInitiated = false;
    private NewGameManager gm;

    // Start is called before the first frame update
    void Start()
    {
        gm = GameObject.FindGameObjectWithTag("GameManager").GetComponent<NewGameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!loadInitiated) 
        {
            LoadNextLevel(gm.currentLocation);
            loadInitiated = true;
        }
    }

    public void LoadNextLevel(string name) 
    {
        StartCoroutine(WaitXSeconds(3));
        SceneManager.LoadScene(sceneName: name);
    }

    public IEnumerator WaitXSeconds(float seconds) 
    {
        yield return new WaitForSeconds(seconds);
    }
}
