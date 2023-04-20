using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DevMapButton : MonoBehaviour
{

    void OnEnable()
    {
        GetComponentInChildren<Text>().text = this.transform.name;
        GetComponent<Button>().onClick.AddListener(delegate () { NewGameManager.Instance.DevGoTo(this.transform.name); });
    }
}
