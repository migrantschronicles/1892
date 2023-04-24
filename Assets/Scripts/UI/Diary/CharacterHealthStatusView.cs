using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterHealthStatusView : MonoBehaviour
{
    [SerializeField]
    private Image characterImage;
    [SerializeField]
    private Text nameText;
    [SerializeField]
    private HealthHomesicknessValue homesicknessValue;
    [SerializeField]
    private DiseasesStickerPanel diseases;

    private ProtagonistHealthData healthData;

    public void SetCharacter(ProtagonistHealthData data)
    {
        healthData = data;
        nameText.text = LocalizationManager.Instance.GetLocalizedString(data.CharacterData.fullName);
        healthData.onHealthChanged += OnHealthChanged;
        OnHealthChanged(data);
    }

    private void OnDestroy()
    {
        if (healthData != null)
        {
            healthData.onHealthChanged -= OnHealthChanged;
        }
    }

    private void OnHealthChanged(ProtagonistHealthData data)
    {
        characterImage.sprite = data.CharacterData.GetPortraitByHealthState(data.HealthState);
        homesicknessValue.Value = data.HomesickessStatus.Value;
        diseases.UpdateDiseases(data);
    }
}
