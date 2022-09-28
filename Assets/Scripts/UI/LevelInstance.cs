using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**
 * A prefab to add to each level as the root for every ui element.
 * This is so that new elements like the new game manager can be easily added and existing elements can be easily changed across levels.
 * If automatically switches scenes, shows/hides the back button, interactables etc.
 * 
 * If you create a new scene, this is the only prefab you need to add as a root game object in the hierarchy.
 * Everything else is inside of this level instance prefab.
 * 
 * In the prefab, there is a parent game object "Scenes".
 * Here you can add the prefab "Scene". This represents one scene, e.g. there could be one main scene and one closeup scene.
 * Each scene contains everything that appears in that scene, i.e. background images, characters, buttons, but only in that scene.
 * In the scene (the script of the Scene prefab) you should add the scene name (how you identify the scene).
 * Then you can add all your backgrounds / characters / art to the Background, Middleground and Foreground game objects.
 * These should contain all static (non-interactable) elements of the scene.
 * In the Interactives object, you can add all of the buttons for the scene (DialogButton / ShopButton).
 * The Interactives automatically hide when a dialog is started or a shop is opened.
 * 
 * In the LevelInstance object, you can now set the default scene (the main scene).
 * 
 * ADD A DIALOG BUTTON
 * If you add a dialog button, you can set the scene name the dialog should start in in the DialogButton script.
 * This tells the level instance to open that scene if the dialog button is pressed.
 * If the field stays empty, the main scene is used.
 * In the Button::OnClick, you should add one callback: 
 * It should call LevelInstance.StartDialog with the dialog button as a parameter.
 * This automatically takes care of showing / hiding everything and the dialog starts.
 * 
 * ADD A SHOP
 * If you have a shop on a scene, you can add the prefab Shop to the scene it appears on.
 * Set the back button in the Shop script to the back button in the level instance object.
 * You can add a ShopButton prefab to the Interactives game object of the scene it should appear on.
 * To open the shop, you can add a callback to the Button::OnClick:
 * It should call LevelInstance.OpenShop with the shop you want to open as the argument.
 * The level instance takes care of showing / hiding everything.
 */
public class LevelInstance : MonoBehaviour
{
    [SerializeField]
    private GameObject sceneParent;
    [SerializeField]
    private Button backButton;
    [SerializeField]
    private DialogSystem dialogSystem;
    [SerializeField]
    private string defaultScene;

    private List<Scene> scenes = new List<Scene>();
    private Scene currentScene;
    private Shop currentShop;

    private void Start()
    {
        if(string.IsNullOrWhiteSpace(defaultScene))
        {
            Debug.LogError("Default scene is empty in level instance");
        }

        for(int i = 0; i < sceneParent.transform.childCount; ++i)
        {
            Scene scene = sceneParent.transform.GetChild(i).GetComponent<Scene>();
            if(scene != null)
            {
                scenes.Add(scene);
            }
        }

        backButton.onClick.AddListener(OnBack);

        foreach(Scene scene in scenes)
        {
            scene.OnActiveStatusChanged(false);
            scene.gameObject.SetActive(false);
        }

        OpenScene(defaultScene);

        if(dialogSystem.StartDialogObject != null)
        {
            OnDialogStarted();
            dialogSystem.gameObject.SetActive(true);
        }
        else
        {
            backButton.gameObject.SetActive(false);
            dialogSystem.gameObject.SetActive(false);
        }
    }

    private void OnBack()
    {
        dialogSystem.gameObject.SetActive(false);
        backButton.gameObject.SetActive(false);
        
        if(currentShop)
        {
            currentShop.gameObject.SetActive(false);
            currentShop = null;
        }

        if(currentScene.SceneName != defaultScene)
        {
            OpenScene(defaultScene);
        }

        if(currentScene)
        {
            currentScene.SetInteractablesVisible(true);
        }
    }

    public Scene GetScene(string name)
    {
        foreach(Scene scene in scenes)
        {
            if(scene.SceneName == name)
            {
                return scene;
            }
        }

        return null;
    }

    public void OpenScene(string sceneName)
    {
        Scene scene = GetScene(sceneName);
        if(scene == null)
        {
            scene = scenes.Count > 0 ? scenes[0] : null;
            if(scene == null)
            {
                return;
            }
        }

        if(currentScene)
        {
            currentScene.OnActiveStatusChanged(false);
            currentScene.gameObject.SetActive(false);
        }

        currentScene = scene;
        currentScene.gameObject.SetActive(true);
        currentScene.OnActiveStatusChanged(true);
    }

    private void OnDialogStarted()
    {
        dialogSystem.gameObject.SetActive(true);
        backButton.gameObject.SetActive(true);

        if(currentScene)
        {
            currentScene.SetInteractablesVisible(false);
        }
    }

    public void StartDialog(GameObject dialogParent)
    {
        dialogSystem.StartDialog(dialogParent);
        OnDialogStarted();
    }

    public void StartDialog(Dialog dialog)
    {
        dialogSystem.StartDialog(dialog);
        OnDialogStarted();
    }

    public void StartDialog(DialogButton button)
    {
        if(!string.IsNullOrWhiteSpace(button.SceneName))
        {
            OpenScene(button.SceneName);
        }

        StartDialog(button.gameObject);
    }

    public void OpenShop(Shop shop)
    {
        if(currentShop)
        {
            currentShop.gameObject.SetActive(false);
        }

        currentShop = shop;
        currentShop.gameObject.SetActive(true);
        backButton.gameObject.SetActive(true);

        if(currentScene)
        {
            currentScene.SetInteractablesVisible(false);
        }
    }
}
