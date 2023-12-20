using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShipBackground : MonoBehaviour
{
    private enum Daytime
    {
        Morning = 0,
        Midday = 1,
        Sunset = 2,
        Night = 3,
    }

    public Sprite[] Skies;
    public Sprite[] Oceans;
    public GameObject OceanContainer;
    public GameObject SkyContainer;
    
    //public SpriteRenderer OceanSprite;
    //public SpriteRenderer SkySprite;
    // Start is called before the first frame update
    void Start()
    {
        LevelInstance.Instance.OnDialogsTodayChanged += OnDialogsTodayChanged;
        var OceanSprite = OceanContainer.GetComponent<SpriteRenderer>();
        var SkySprite = SkyContainer.GetComponent<SpriteRenderer>();
        OceanSprite.sprite = Oceans[0];
        SkySprite.sprite = Skies[0];
    }

    void OnDialogsTodayChanged(int hours)
    {
        if (LevelInstance.Instance.LevelMode == LevelInstanceMode.Ship)
        {
            var OceanSprite = OceanContainer.GetComponent<SpriteRenderer>();
            var SkySprite = SkyContainer.GetComponent<SpriteRenderer>();
            if (hours > 3) 
            {
                OceanSprite.sprite = Oceans[1];
                SkySprite.sprite = Skies[1];
            }
            if (hours > 7)
            {
                OceanSprite.sprite = Oceans[2];
                SkySprite.sprite = Skies[2];
            }
            if (hours > 10)
            {
                OceanSprite.sprite = Oceans[3];
                SkySprite.sprite = Skies[3];
            }
            if (hours > 13)
            {
                OceanSprite.sprite = Oceans[4];
                SkySprite.sprite = Skies[4];
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
