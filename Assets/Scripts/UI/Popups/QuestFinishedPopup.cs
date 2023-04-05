using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestFinishedPopup : MonoBehaviour, IPopup
{
    public bool CanClose { get { return false; } }

    public IPopup.OnPopupAction OnAccept;

    public void HandleAccept()
    {
        OnAccept?.Invoke(this);
    }
}