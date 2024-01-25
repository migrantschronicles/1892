using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPage : MonoBehaviour
{
    [SerializeField]
    private GameObject viewPanelLeft;
    [SerializeField]
    private GameObject viewPanelRight;
    [SerializeField]
    private GameObject moreInformationPanelOne;
    [SerializeField]
    private GameObject moreInformationPanelTwo;
    [SerializeField]
    private GameObject moreInformationPanelThree;
    [SerializeField]
    private GameObject oneCharacterImage;
    [SerializeField]
    private GameObject characterHealthStatusViewPrefab;
    [SerializeField]
    private GameObject characterHealthStatusViewSmallPrefab;

    private void Start()
    {
        int characterCount = NewGameManager.Instance.PlayerCharacterManager.SelectedData.protagonistData.Length;
        switch(characterCount)
        {
            case 1:
                oneCharacterImage.SetActive(true);
                moreInformationPanelOne.SetActive(true);
                break;

            case 2:
                moreInformationPanelTwo.SetActive(true);
                break;

            case 3:
                moreInformationPanelThree.SetActive(true);
                break;
        }

        List<ProtagonistHealthData> characters = new(NewGameManager.Instance.HealthStatus.Characters);
        foreach(ProtagonistHealthData character in characters)
        {
            if(character.CharacterData.isMainProtagonist)
            {
                GameObject view = Instantiate(characterHealthStatusViewPrefab, viewPanelLeft.transform);
                CharacterHealthStatusView status = view.GetComponent<CharacterHealthStatusView>();
                status.SetCharacter(character);

                break;
            }
        }

        foreach(ProtagonistHealthData character in characters)
        {
            if(!character.CharacterData.isMainProtagonist)
            {
                GameObject prefab = characterCount == 2 ? characterHealthStatusViewPrefab : characterHealthStatusViewSmallPrefab;
                GameObject view = Instantiate(prefab, viewPanelRight.transform);
                CharacterHealthStatusView status = view.GetComponent<CharacterHealthStatusView>();
                status.SetCharacter(character);
            }
        }

    }
}
