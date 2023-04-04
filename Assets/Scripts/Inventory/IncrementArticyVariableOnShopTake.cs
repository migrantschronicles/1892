using Articy.TheMigrantsChronicles.GlobalVariables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncrementArticyVariableOnShopTake : MonoBehaviour
{
    [SerializeField]
    private string articyVariable;
    [SerializeField]
    private int incrementAmount = 1;

    private Shop shop;

    private void Start()
    {
        shop = GetComponent<Shop>();
        shop.onTradeAccepted += OnTradeAccepted;
    }

    private void OnTradeAccepted(Dictionary<Item, int> transfers)
    {
        foreach(var item in transfers)
        {
            if(item.Value > 0)
            {
                int value = ArticyGlobalVariables.Default.GetVariableByString<int>(articyVariable);
                ArticyGlobalVariables.Default.SetVariableByString(articyVariable, value + incrementAmount);
                shop.onTradeAccepted -= OnTradeAccepted;
                break;
            }
        }
    }
}
