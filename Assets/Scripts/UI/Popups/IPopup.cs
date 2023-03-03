using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPopup 
{
    public bool CanClose { get { return true; } }

    public delegate void OnCanCloseChangedEvent(IPopup popup, bool canClose);

    public void AddOnCanCloseChangedListener(OnCanCloseChangedEvent onCanCloseChanged) { }
    public void RemoveOnCanCloseChangedListener(OnCanCloseChangedEvent onCanCloseChanged) { }

    public delegate void OnPopupAction(IPopup popup);
    public InterfaceVisibilityFlags InterfaceVisibilityFlags { get { return InterfaceVisibilityFlags.None; } }
}
