using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopButton : MonoBehaviour
{
    [SerializeField]
    private Shop shop;

    private Button button;

    public Shop Shop { get { return shop; } }

    private void Awake()
    {
        button = GetComponentInChildren<Button>();
    }

    private void Start()
    {
        if(shop)
        {
            button.onClick.AddListener(OnOpenShop);
        }
        else
        {
            Debug.Log($"({name}): Remove the Button::OnClick and add the shop you want to show in the ShopButton script");
        }
    }

    private void OnOpenShop()
    {
        if(shop)
        {
            LevelInstance.Instance.OpenShop(shop);
        }
    }
}
