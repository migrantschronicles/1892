using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndDayPortrait : MonoBehaviour
{
    [SerializeField]
    private Image portrait;
    [SerializeField]
    private Image foodItem;
    [SerializeField]
    private Image counterBackground;
    [SerializeField]
    private Text counter;
    [SerializeField]
    private int amount;
    [SerializeField]
    private string protagonistName;
    [SerializeField]
    private Sprite emptyFoodItemSprite;
    [SerializeField]
    private Sprite hungryFoodItemSprite;
    [SerializeField]
    private Sprite filledFoodItemSprite;

    public int FoodAmount { get { return amount; } set { amount = value; UpdateElements(); } }

    public delegate void OnPortraitClickedEvent(EndDayPortrait portrait);
    public event OnPortraitClickedEvent OnPortraitClicked;

    private void Start()
    {
        UpdateElements();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        UnityEditor.EditorApplication.delayCall += _OnValidate;
    }

    private void _OnValidate()
    {
        if (this == null)
        {
            return;
        }

        UnityEditor.EditorApplication.delayCall -= _OnValidate;

        UpdateElements();
    }
#endif

    private void UpdateElements()
    {
        Sprite portraitSprite = null;
        HealthState state = HealthState.Neutral;
#if UNITY_EDITOR
        if(!string.IsNullOrWhiteSpace(protagonistName))
        {
            ProtagonistData data = NewGameManager.Instance.PlayableCharacterData.GetProtagonistDataByName(protagonistName);
            ProtagonistHealthData healthData = NewGameManager.Instance.HealthStatus.GetHealthStatus(protagonistName);
            portraitSprite = data.GetPortraitByHealthState(healthData.HealthState);
        }
#endif

        if(portraitSprite != null)
        {
            portrait.sprite = portraitSprite;
        }
        portrait.gameObject.SetActive(portraitSprite != null);

        foodItem.sprite = amount > 0 ? filledFoodItemSprite : (state == HealthState.Hungry ? hungryFoodItemSprite : emptyFoodItemSprite);
        counterBackground.gameObject.SetActive(amount > 1);
        counter.text = amount.ToString();
    }

    public void Init(ProtagonistData data)
    {
        protagonistName = data.name;
        UpdateElements();
    }

    public void OnClick()
    {
        OnPortraitClicked?.Invoke(this);
    }
}
