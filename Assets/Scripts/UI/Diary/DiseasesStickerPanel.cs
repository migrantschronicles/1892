using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiseasesStickerPanel : MonoBehaviour
{
    [SerializeField]
    private GameObject seasickness;
    [SerializeField]
    private GameObject hungry;
    [SerializeField]
    private GameObject cholera;

    public void UpdateDiseases(ProtagonistHealthData data)
    {
        seasickness.SetActive(data.SeasicknessStatus.IsCurrentlySeasick);
        hungry.SetActive(data.HungryStatus.DaysWithoutEnoughFood > 0);
        cholera.SetActive(data.CholeraStatus.IsSick);
    }
}
