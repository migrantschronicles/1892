using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

enum Mode
{
    None,
    Shop,
    Dialog,
    Diary
}

enum OverlayMode
{
    None,
    Shop,
    Diary
}

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
 * You can add a SceneElementSelector anywhere in Background, Middleground or Foreground.
 * In the SceneElementSelector, you can set the condition under which the childs of the selector should be visible,
 * so you can hide elements based on a condition (e.g. some people are only there if the ship is there).
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
 * You can add a ShopButton prefab to the Interactives game object of the scene it should appear on.
 * To open the shop, you need to select the shop you want to open in the ShopButton script on the ShopButton.
 * The level instance takes care of showing / hiding everything.
 * If this is a shop for the dialog (for a Quest / Items decision option), it should be placed in the Overlays game object.
 * Otherwise, it can be placed in the scene where it is used.
 */
public class LevelInstance : MonoBehaviour
{
    [SerializeField]
    private GameObject sceneParent;
    [SerializeField]
    private GameObject foregroundSceneParent;
    [SerializeField]
    private Button backButton;
    [SerializeField]
    private DialogSystem dialogSystem;
    [SerializeField]
    private GameObject blur;
    [SerializeField]
    private Interface ui;
    [SerializeField]
    private string defaultScene;
    [SerializeField]
    private DiaryEntry diaryEntry;
    [SerializeField]
    private AudioClip[] musicClips;
    [SerializeField]
    private GameObject draggedItemPrefab;
    [SerializeField]
    private GameObject sceneInteractables;
    [SerializeField]
    private Canvas canvas;

    private List<Scene> scenes = new List<Scene>();
    private Scene currentScene;
    private Shop currentShop;
    private Scene currentAdditiveScene = null;
    private IEnumerable<GameObject> currentHiddenObjects;
    private string previousScene;
    private OverlayMode overlayMode = OverlayMode.None;
    private bool startedPlayingMusic = false;
    private Mode mode = Mode.None;
    private DraggedItem draggedItem;

    private static LevelInstance instance;
    public static LevelInstance Instance { get { return instance; } }

    public IngameDiary IngameDiary { get { return ui.IngameDiary; } }
    public bool IsDragging { get { return draggedItem != null; } }
    public Shop CurrentShop { get { return currentShop; } }

    private void Awake()
    {
        instance = this;
        previousScene = defaultScene;

        for (int i = 0; i < sceneParent.transform.childCount; ++i)
        {
            Scene scene = sceneParent.transform.GetChild(i).GetComponent<Scene>();
            if (scene != null)
            {
                scenes.Add(scene);
            }
        }

        for (int i = 0; i < foregroundSceneParent.transform.childCount; ++i)
        {
            Scene scene = foregroundSceneParent.transform.GetChild(i).GetComponent<Scene>();
            if (scene != null)
            {
                scenes.Add(scene);
            }
        }

        // Set the frame rate limit to 30 fps.
        // This should suffice for mobile, 60 fps should not be needed.
        Application.targetFrameRate = 30;
    }

    private void Start()
    {
        if (string.IsNullOrWhiteSpace(defaultScene))
        {
            Debug.Log("Default scene is empty in level instance");
            return;
        }

        backButton.onClick.AddListener(OnBack);
        blur.SetActive(false);
        IngameDiary.Diary.onDiaryStatusChanged += OnDiaryStatusChanged;

        foreach(Scene scene in scenes)
        {
            scene.OnActiveStatusChanged(false);
            scene.gameObject.SetActive(false);
        }

        OpenScene(defaultScene);

        dialogSystem.gameObject.SetActive(false);
        if (diaryEntry)
        {
            blur.SetActive(true);
            NewGameManager.Instance.AddDiaryEntry(diaryEntry);
            backButton.gameObject.SetActive(true);
            ui.SetUIElementsVisible(InterfaceVisibilityFlags.None);
            ui.OpenDiaryImmediately(DiaryPageLink.Diary);
            mode = Mode.Diary;
        }
        else
        {
            backButton.gameObject.SetActive(false);
            ui.SetUIElementsVisible(InterfaceVisibilityFlags.All);
            AudioManager.Instance.PlayMusic(musicClips);
            startedPlayingMusic = true;
        }
    }

    private void OnBack()
    {
        if(overlayMode != OverlayMode.None)
        {
            // The back button has been pressed during an overlay (can only happen during a dialog, when a shop or the map is opened),
            // so return to the dialog system.

            switch(overlayMode)
            {
                case OverlayMode.Shop:
                {
                    // A shop was open, so deactivate it.
                    currentShop.onTradeAccepted -= OnTradeAccepted;
                    currentShop.gameObject.SetActive(false);
                    currentShop.OnClosed();
                    AudioManager.Instance.PlayFX(currentShop.closeClip);
                    currentShop = null;
                    break;
                }

                case OverlayMode.Diary:
                {
                    // The map was open, hide it.
                    AudioManager.Instance.PlayFX(ui.IngameDiary.Diary.closeClip);
                    ui.CloseDiaryImmediately();
                    break;
                }
            }

            // Readd all the necessary elements for the dialog

            if(currentAdditiveScene)
            {
                currentAdditiveScene.gameObject.SetActive(true);
            }

            SetBackButtonVisible(true);
            ui.SetUIElementsVisible(InterfaceVisibilityFlags.None);
            blur.SetActive(true);
            overlayMode = OverlayMode.None;
            dialogSystem.gameObject.SetActive(true);
            dialogSystem.OnOverlayClosed();
        }
        else
        {
            // The back button has been pressed during a dialog, in a shop, in the diary, etc.
            switch (mode)
            {
                case Mode.Dialog:
                    // If the dialog was active, notify it to clear its entries.
                    dialogSystem.OnClose();
                    AudioManager.Instance.PlayFX(dialogSystem.closeClip);
                    dialogSystem.gameObject.SetActive(false);

                    if (currentAdditiveScene)
                    {
                        // Hide the additive scene that was enabled during a dialog.
                        currentAdditiveScene.OnActiveStatusChanged(false);
                        currentAdditiveScene.gameObject.SetActive(false);
                        currentAdditiveScene = null;
                    }

                    if (previousScene != null && currentScene.SceneName != previousScene)
                    {
                        // If the scene switched for a dialog temporarily, return to the previous scene.
                        OpenScene(previousScene);
                        previousScene = null;
                    }

                    if (currentHiddenObjects != null)
                    {
                        // Reactivate all the characters that were hidden during the dialog.
                        foreach (GameObject go in currentHiddenObjects)
                        {
                            go.SetActive(true);
                        }
                        currentHiddenObjects = null;
                    }

                    break;

                case Mode.Diary:
                    if(ui.IngameDiary.Diary.IsAnimationInProgress)
                    {
                        return;
                    }

                    ui.SetDiaryOpened(false);
                    AudioManager.Instance.PlayFX(ui.IngameDiary.Diary.closeClip);
                    if (!startedPlayingMusic)
                    {
                        AudioManager.Instance.PlayMusic(musicClips);
                        startedPlayingMusic = true;
                    }
                    break;

                case Mode.Shop:
                    // Hide the shop
                    currentShop.gameObject.SetActive(false);
                    currentShop.OnClosed();
                    AudioManager.Instance.PlayFX(currentShop.closeClip);
                    currentShop = null;
                    break;
            }

            backButton.gameObject.SetActive(false);

            if (mode != Mode.Diary)
            {
                // Hide everything
                ui.SetUIElementsVisible(InterfaceVisibilityFlags.All);
                blur.SetActive(false);

                // Enable the buttons again.
                sceneInteractables.SetActive(true);

                mode = Mode.None;
            }
        }
    }

    public bool HasScene(string name)
    {
        if(string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        return scenes.Exists(scene => scene.SceneName == name);
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
        mode = Mode.Dialog;
        sceneInteractables.SetActive(false);
        blur.SetActive(true);
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

        AudioManager.Instance.PlayFX(dialogSystem.openClip);
    }

    public void OpenShop(Shop shop)
    {
        if(overlayMode != OverlayMode.None)
        {
            Debug.LogError("Only one shop may be opened during a dialog");
            return;
        }

        if(currentShop)
        {
            currentShop.gameObject.SetActive(false);
        }

        currentShop = shop;
        currentShop.gameObject.SetActive(true);
        backButton.gameObject.SetActive(true);
        ui.SetUIElementsVisible(InterfaceVisibilityFlags.StatusInfo);
        sceneInteractables.SetActive(false);
        blur.SetActive(true);

        if(dialogSystem.gameObject.activeSelf)
        {
            // This is an overlay (a shop was opened during a dialog (the corresponding decision option was selected), so hide the dialog).
            overlayMode = OverlayMode.Shop;
            dialogSystem.gameObject.SetActive(false);
            if(currentAdditiveScene)
            {
                currentAdditiveScene.gameObject.SetActive(false);
            }

            currentShop.onTradeAccepted += OnTradeAccepted;
        }
        else
        {
            mode = Mode.Shop;
        }

        currentShop.OnOpened();
        AudioManager.Instance.PlayFX(shop.openClip);
    }

    private void OnTradeAccepted()
    {
        IEnumerable<SetCondition> setConditions = currentShop.AcceptableItemSetConditions;
        NewGameManager.Instance.conditions.AddConditions(setConditions);

        OnBack();
    }

    public void OpenDiary()
    {
        OpenDiary(DiaryPageLink.Inventory);
    }

    public void OpenDiary(DiaryPageLink type)
    {
        blur.SetActive(true);
        ui.SetUIElementsVisible(InterfaceVisibilityFlags.None);

        if (mode == Mode.Dialog)
        {
            // This is an overlay (the travel decision option was selected during a dialog), so hide the dialog system.
            ui.OpenDiaryImmediately(type);
            overlayMode = OverlayMode.Diary;
            dialogSystem.gameObject.SetActive(false);
            if (currentAdditiveScene)
            {
                currentAdditiveScene.gameObject.SetActive(false);
            }
        }
        else
        {
            ui.SetDiaryOpened(type);
            mode = Mode.Diary;
            sceneInteractables.SetActive(false);
        }

        AudioManager.Instance.PlayFX(ui.IngameDiary.Diary.openClip);
    }

    private void OnDiaryStatusChanged(OpenStatus status)
    {
        switch(status)
        {
            case OpenStatus.Opened:
                if (mode == Mode.Diary)
                {
                    backButton.gameObject.SetActive(true);
                }
                break;

            case OpenStatus.Closed:
                if (mode == Mode.Diary)
                {
                    ui.SetUIElementsVisible(InterfaceVisibilityFlags.All);
                    blur.SetActive(false);
                    sceneInteractables.SetActive(true);
                    mode = Mode.None;
                }
                break;
        }
    }

    public void OpenClock()
    {
        ui.OpenEndDayPopUp();
    }

    public void SetBackButtonVisible(bool visible)
    {
        backButton.gameObject.SetActive(visible);
    }

    public Texture2D TakeDiaryScreenshot(DiaryEntryData entry)
    {
        ui.PrepareForDiaryScreenshot(entry);

        Texture2D renderedTexture = TakeScreenshot(816, 510);

        Debug.Log($"Captured diary screenshot");
        ui.ResetFromScreenshot();

        return renderedTexture;
    }

    /**
     * Takes a screenshot of the map and travel routes.
     * Should only be called from PDFBuilder, not manually.
     */
    public Texture2D TakeMapScreenshot()
    {
        ui.PrepareForMapScreenshot();

        Texture2D renderedTexture = TakeScreenshot(944, 590);

        Debug.Log($"Captured map screenshot");
        ui.ResetFromScreenshot();

        return renderedTexture;
    }

    private Texture2D TakeScreenshot(int outputWidth, int outputHeight)
    {
        // Hide ui elements
        bool wasBackButtonVisible = backButton.gameObject.activeSelf;
        backButton.gameObject.SetActive(false);

        // Set the canvas to render the ui as well
        Canvas canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = Camera.main;

        // Render the camera view to a new render texture
        RenderTexture screenTexture = new RenderTexture(Screen.width, Screen.height, 16);
        Camera.main.targetTexture = screenTexture;
        Camera.main.Render();

        // Set the output size and adjust the image size that is actually rendered (same aspect ratio of screen).
        int targetWidth = outputWidth;
        int targetHeight = outputHeight;
        float sourceAspect = (float)Screen.width / Screen.height;
        float outputAspect = outputWidth / outputHeight;
        if (!Mathf.Approximately(sourceAspect, outputAspect))
        {
            if (outputAspect > sourceAspect)
            {
                targetWidth = (int)(targetHeight * sourceAspect);
            }
            else if (outputAspect < sourceAspect)
            {
                targetHeight = (int)(targetWidth / sourceAspect);
            }
        }

        // Resize the screen texture to the new target size
        RenderTexture resizedTexture = new RenderTexture(targetWidth, targetHeight, 16);
        RenderTexture.active = resizedTexture;
        Graphics.Blit(screenTexture, resizedTexture);

        // Read the render texture into a texture.
        Texture2D renderedTexture = new Texture2D(outputWidth, outputHeight);
        int destX = 0;
        int destY = 0;
        if (!Mathf.Approximately(sourceAspect, outputAspect))
        {
            // Adjust the x and y position where the pixel data in the texture is written to.
            if (outputAspect > sourceAspect)
            {
                destX = (int)((outputWidth - (outputHeight * sourceAspect)) / 2);
            }
            else if (outputAspect < sourceAspect)
            {
                destY = (int)((outputHeight - (outputWidth / sourceAspect)) / 2);
            }

            // Fill the background transparent
            Color[] renderedTextureColors = renderedTexture.GetPixels();
            Color backgroundColor = new Color(0, 0, 0, 0);
            for (int i = 0; i < renderedTextureColors.Length; ++i)
            {
                renderedTextureColors[i] = backgroundColor;
            }
            renderedTexture.SetPixels(renderedTextureColors);
        }
        renderedTexture.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), destX, destY);
        RenderTexture.active = null;

        // Cleanup
        Camera.main.targetTexture = null;
        canvas.worldCamera = null;
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        screenTexture.Release();

        // Show ui elements again
        backButton.gameObject.SetActive(wasBackButtonVisible);

        return renderedTexture;
    }

    public void OnBeginDrag(PointerEventData data, ShopInventorySlot slot)
    {
        Debug.Assert(!IsDragging);
        draggedItem = Instantiate(draggedItemPrefab, canvas.transform).GetComponent<DraggedItem>();
        draggedItem.Slot = slot;
        draggedItem.OnBeginDrag(data);
        currentShop.OnBeginDrag(draggedItem);
    }

    public void OnDrag(PointerEventData data)
    {
        Debug.Assert(IsDragging);
        draggedItem.OnDrag(data);
        currentShop.OnDrag(draggedItem);
    }

    public void OnEndDrag(PointerEventData data)
    {
        Debug.Assert(IsDragging);
        draggedItem.OnEndDrag(data);
        currentShop.OnEndDrag(draggedItem);
        Destroy(draggedItem.gameObject);
        draggedItem = null;
    }
}
