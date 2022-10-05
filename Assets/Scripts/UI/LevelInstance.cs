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
 * SCENE BUTTON
 * This is a prefab to switch the scene on click.
 * You can add it to the level (to the interactables of the scene) and set the Scene Name property in the Scene Button script.
 * Now if you press the button, the scene switches to the new scene.
 * 
 * ADD A DIALOG BUTTON
 * If you add a dialog button, you can set the scene name the dialog should start in in the DialogButton script.
 * This tells the level instance to open that scene if the dialog button is pressed.
 * This is different from the Scene Button in that it will return to the previous scene after the dialog is closed.
 * Also, the blur gets added to the new scene.
 * If the field stays empty, the current scene stays.
 * In the Button::OnClick, you should add one callback: 
 * It should call LevelInstance.StartDialog with the dialog button as a parameter.
 * This automatically takes care of showing / hiding everything and the dialog starts.
 * It also adds a blur to the background.
 * If you want to have characters on the left and right during a dialog, create a new scene (after the scene your dialog is in),
 * and add the characters to it (basically everything you want above the blur).
 * Then go to the dialog button and set the AdditiveSceneName to the name of the new scene.
 * This is the scene that will be displayed on top of the blur.
 * If you want e.g. the characters that are involved in the dialog to disappear, you can add them to the DialogButton::HideObjects.
 * The objects in this list are hidden when the dialog starts and shown again when the dialog stops, so you can
 * hide characters that are in the additive scene anyway.
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
    private GameObject blur;
    [SerializeField]
    private Diary diary;
    [SerializeField]
    private Interface ui;
    [SerializeField]
    private string defaultScene;
    [SerializeField]
    private DiaryEntry diaryEntry;

    private List<Scene> scenes = new List<Scene>();
    private Scene currentScene;
    private Shop currentShop;
    private Scene currentAdditiveScene = null;
    private IEnumerable<GameObject> currentHiddenObjects;
    private string previousScene;

    private static LevelInstance instance;
    public static LevelInstance Instance { get { return instance; } }

    public Diary Diary { get { return diary; } }

    private void Awake()
    {
        instance = this;
        previousScene = defaultScene;
    }

    private void Start()
    {
        if(string.IsNullOrWhiteSpace(defaultScene))
        {
            Debug.Log("Default scene is empty in level instance");
            return;
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
        blur.SetActive(false);

        foreach(Scene scene in scenes)
        {
            scene.OnActiveStatusChanged(false);
            scene.gameObject.SetActive(false);
        }

        OpenScene(defaultScene);

        dialogSystem.gameObject.SetActive(false);
        if (diaryEntry)
        {
            SetBlurAfterGameObject(sceneParent);
            NewGameManager.Instance.AddDiaryEntry(diaryEntry);
            backButton.gameObject.SetActive(true);
            ui.SetUIElementsVisible(InterfaceVisibilityFlags.None);
            ui.SetDiaryVisible(true, DiaryPageType.Diary);
        }
        else
        {
            backButton.gameObject.SetActive(false);
            ui.SetUIElementsVisible(InterfaceVisibilityFlags.All);
            ui.SetDiaryVisible(false);
        }
    }

    private void OnBack()
    {
        dialogSystem.gameObject.SetActive(false);
        backButton.gameObject.SetActive(false);
        ui.SetUIElementsVisible(InterfaceVisibilityFlags.All);
        ui.SetDiaryVisible(false);
        DisableBlur();
        
        if(currentShop)
        {
            currentShop.gameObject.SetActive(false);
            currentShop = null;
        }

        if(currentAdditiveScene)
        {
            currentAdditiveScene.OnActiveStatusChanged(false);
            currentAdditiveScene.gameObject.SetActive(false);
            currentAdditiveScene = null;
        }

        if(previousScene != null && currentScene.SceneName != previousScene)
        {
            OpenScene(previousScene);
            previousScene = null;
        }

        if(currentScene)
        {
            currentScene.SetInteractablesVisible(true);
        }

        if(currentHiddenObjects != null)
        {
            foreach(GameObject go in currentHiddenObjects)
            {
                go.SetActive(true);
            }
            currentHiddenObjects = null;
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
            Debug.LogError($"Scene \"{sceneName}\" could not be found");
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

    public void OpenSceneAdditive(string sceneName)
    {
        Scene scene = GetScene(sceneName);
        if(scene == null)
        {
            Debug.LogError($"Scene \"{sceneName}\" could not be found");
            return;
        }

        if(currentAdditiveScene)
        {
            currentAdditiveScene.OnActiveStatusChanged(false);
            currentAdditiveScene.gameObject.SetActive(false);
        }

        currentAdditiveScene = scene;
        currentAdditiveScene.gameObject.SetActive(true);
        currentAdditiveScene.OnActiveStatusChanged(true);
    }

    private void OnDialogStarted()
    {
        dialogSystem.gameObject.SetActive(true);
        backButton.gameObject.SetActive(true);
        ui.SetUIElementsVisible(InterfaceVisibilityFlags.None);

        if(currentScene)
        {
            currentScene.SetInteractablesVisible(false);
            SetBlurAfterGameObject(currentScene.gameObject);
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
            previousScene = currentScene?.SceneName;
            OpenScene(button.SceneName);
        }

        currentHiddenObjects = button.HideObjects;
        foreach(GameObject go in currentHiddenObjects)
        {
            go.SetActive(false);
        }

        StartDialog(button.gameObject);

        if(!string.IsNullOrWhiteSpace(button.AdditiveSceneName))
        {
            OpenSceneAdditive(button.AdditiveSceneName);
        }
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
        ui.SetUIElementsVisible(InterfaceVisibilityFlags.StatusInfo);

        if(currentScene)
        {
            currentScene.SetInteractablesVisible(false);
            SetBlurInFrontOfGameObject(shop.gameObject);
        }
    }

    private void SetBlurAfterGameObject(GameObject previous)
    {
        blur.transform.SetParent(previous.transform.parent, false);
        blur.transform.SetSiblingIndex(previous.transform.GetSiblingIndex() + 1);
        blur.SetActive(true);
    }

    private void SetBlurInFrontOfGameObject(GameObject next)
    {
        blur.transform.SetParent(next.transform.parent, false);
        blur.transform.SetSiblingIndex(next.transform.GetSiblingIndex());
        blur.SetActive(true);
    }

    private void DisableBlur()
    {
        blur.transform.SetParent(transform);
        blur.SetActive(false);
    }

    public void OpenDiary()
    {
        SetBlurAfterGameObject(sceneParent);
        ui.SetUIElementsVisible(InterfaceVisibilityFlags.None);
        ui.SetDiaryVisible(true);
        backButton.gameObject.SetActive(true);
    }
}
