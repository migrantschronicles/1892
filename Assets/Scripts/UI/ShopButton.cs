using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopButton : MonoBehaviour
{
    [SerializeField]
    private Shop shop;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void Start()
    {
        button.onClick.AddListener(OnOpenShop);
    }

    private void OnOpenShop()
    {
        if(shop)
        {
            LevelInstance.Instance.OpenShop(shop);
        }
    }
}
