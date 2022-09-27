using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiaryInventoryContainer : InventoryContainer
{
    [SerializeField]
    private Image backgroundImage;
    [SerializeField]
    private Image lockedImage;

    public void SetUnlocked(bool unlocked)
    {
        backgroundImage.enabled = unlocked;
        SlotsParent.SetActive(unlocked);
        lockedImage.gameObject.SetActive(!unlocked);
    }
}
