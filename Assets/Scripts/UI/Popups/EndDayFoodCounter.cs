using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndDayFoodCounter : MonoBehaviour
{
    [SerializeField]
    private Button subtractButton;
    [SerializeField]
    private Button addButton;
    [SerializeField]
    private Image counterBackground;
    [SerializeField]
    private Text counterText;
    [SerializeField]
    private bool isInfinite;
    [SerializeField]
    private bool canSubtract = true;
    [SerializeField]
    private bool canAdd = true;
    [SerializeField]
    private Sprite infiniteBackground;
    [SerializeField]
    private Sprite normalBackground;
    //[SerializeField]
    //private Sprite CurrencyBackgroundBase;
    //[SerializeField]
    //private Sprite CurrencyBackgroundActive;

    private int count = 0;
    public int Count 
    { 
        get { return count; } 
        set 
        { 
            if(!isInfinite)
            {
                count = value; 
                counterText.text = count.ToString(); 
            }
        } 
    }
    public bool IsInfinite { get { return isInfinite; } }

    public delegate void OnSubtractEvent();
    public event OnSubtractEvent OnSubtract;
    public delegate void OnAddEvent();
    public event OnAddEvent OnAdd;


    private void Awake()
    {
        SetIsInfinite(isInfinite);
        subtractButton.onClick.AddListener(() => OnSubtract?.Invoke());
        addButton.onClick.AddListener(() => OnAdd?.Invoke());
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        UnityEditor.EditorApplication.delayCall += _OnValidate;
    }

    private void _OnValidate()
    {
        if(this == null)
        {
            return;
        }

        UnityEditor.EditorApplication.delayCall -= _OnValidate;

        UpdateSprites();
    }
#endif

    public void SetIsInfinite(bool infinite)
    {
        isInfinite = infinite;
        UpdateSprites();
    }

    public void SetCanSubtract(bool canSubtract)
    {
        this.canSubtract = canSubtract;
        UpdateSprites();
    }

    public void SetCanAdd(bool canAdd)
    {
        this.canAdd = canAdd;
        UpdateSprites();
    }

    private void UpdateSprites()
    {
        counterBackground.sprite = isInfinite ? infiniteBackground : normalBackground;
        counterText.gameObject.SetActive(!isInfinite);
        subtractButton.gameObject.SetActive(!isInfinite && canSubtract);
        addButton.gameObject.SetActive(!isInfinite && canAdd);
    }
}
