using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

class PopupStackInfo
{
    public GameObject popupGO;
    public IPopup popup;
}

public class PopupManager : MonoBehaviour
{
    Stack<PopupStackInfo> popupStack = new Stack<PopupStackInfo>();

    public int Count { get { return popupStack.Count; } }

    public void PushPopup(GameObject popupGO)
    {
        if(popupStack.TryPeek(out PopupStackInfo prevInfo))
        {
            prevInfo.popup.RemoveOnCanCloseChangedListener(OnCanCloseChanged);
            prevInfo.popupGO.SetActive(false);
        }

        IPopup popup = popupGO.GetComponent<IPopup>();
        bool canClose = popup != null ? popup.CanClose : true;
        InterfaceVisibilityFlags flags = popup != null ? popup.InterfaceVisibilityFlags : InterfaceVisibilityFlags.None;
        PopupStackInfo info = new()
        {
            popupGO = popupGO,
            popup = popup
        };
        popupStack.Push(info);
        popup.AddOnCanCloseChangedListener(OnCanCloseChanged);
        LevelInstance.Instance.SetBackButtonVisible(canClose);
        LevelInstance.Instance.SetInterfaceVisibilityFlags(flags);
    }

    /**
     * Removes the top popup from the stack and destroys it.
     * @return True if there are still more popups, false if this was the last popup.
     */
    public bool PopPopup()
    {
        if(popupStack.TryPop(out PopupStackInfo info))
        {
            Destroy(info.popupGO);
        }

        if(popupStack.TryPeek(out PopupStackInfo nextInfo))
        {
            nextInfo.popupGO.SetActive(true);
            nextInfo.popup.AddOnCanCloseChangedListener(OnCanCloseChanged);
            LevelInstance.Instance.SetBackButtonVisible(nextInfo.popup.CanClose);
            LevelInstance.Instance.SetInterfaceVisibilityFlags(nextInfo.popup.InterfaceVisibilityFlags);
            return true;
        }

        return false;
    }

    /**
     * Deletes the stack and destroys all popups.
     */
    public void ClearHistory()
    {
        while(popupStack.TryPop(out PopupStackInfo info))
        {
            Destroy(info.popupGO);
        }
    }

    private void OnCanCloseChanged(IPopup popup, bool canClose)
    {
        if(popupStack.TryPeek(out PopupStackInfo info))
        {
            if(info.popup == popup)
            {
                LevelInstance.Instance.SetBackButtonVisible(canClose);
            }
        }
    }
}
