using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuFlag : MonoBehaviour
{
    public SpriteRenderer Flag;
    public Language language;
    private MainMenuDiaryState state;

    private void SetFlagSprite()
    {
        Flag.gameObject.SetActive(true);
    }


}
