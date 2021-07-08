using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class GameManager : MonoBehaviour
{
    public Button AddCustomTransportationButton;
    public Button HideCustomTransportationButton;

    public RectTransform TransportationFormPrefab;

    private RectTransform form;

    public void OpenCustomTransportationForm()
    {
        form = Instantiate(TransportationFormPrefab, Vector3.zero, Quaternion.identity);

        var width = form.gameObject.GetComponent<RectTransform>().rect.width;
        var height = form.gameObject.GetComponent<RectTransform>().rect.height;

        var canvasWidth = UICanvas.gameObject.GetComponent<RectTransform>().rect.width;
        var canvasHeight = UICanvas.gameObject.GetComponent<RectTransform>().rect.height;

        form.transform.position = new Vector3((canvasWidth - width) / 2, (canvasHeight - height) / 2, 0);
        form.transform.SetParent(UICanvas.transform);
    }

    public void HideCustomTransportationForm()
    {
        if(form != null)
        {
            Destroy(form.gameObject);
        }
    }
}
