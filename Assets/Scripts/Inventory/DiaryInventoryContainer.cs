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
    [SerializeField]
    private Button removeItemButton;

    public delegate void OnRemoveItemEvent(DiaryInventoryContainer container);
    public event OnRemoveItemEvent onRemoveItem;

    private void Start()
    {
        removeItemButton.onClick.AddListener(() => onRemoveItem?.Invoke(this));
    }

    public void SetUnlocked(bool unlocked)
    {
        backgroundImage.enabled = unlocked;
        SlotsParent.SetActive(unlocked);
        lockedImage.gameObject.SetActive(!unlocked);
        removeItemButton.gameObject.SetActive(unlocked);
    }
}
