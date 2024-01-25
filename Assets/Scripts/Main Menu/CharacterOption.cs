using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterOption : MonoBehaviour
{
    [SerializeField]
    private CharacterType character;

    private Outline outline;
    private Button button;

    public CharacterType Character { get => character; }

    public delegate void OnSelectedEvent(CharacterOption option);
    public event OnSelectedEvent onSelected;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => onSelected?.Invoke(this));
    }

    private void Start()
    {
        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        if(outline == null)
        {
            outline = GetComponent<Outline>();
        }
        outline.enabled = selected;
    }
}
