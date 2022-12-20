using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;

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
 * It automatically switches scenes, shows/hides the back button, interactables etc.
 * 
 * If you create a new scene, this is the only prefab you need to add as a root game object in the hierarchy.
 * Everything else is inside of this level instance prefab. 
 * Also you need to delete any preexising cameras, since a camera is included in the prefab.
 * 
 * In the prefab, there is a parent game object "Scenes".
 * Here you can add the prefab "Scene". This represents one scene, e.g. there could be one street scene and one ticket stand scene.
 * Each scene contains everything that appears in that scene, i.e. background images, characters, but only in that scene.
 * In the scene (the script of the Scene prefab) you should add the scene name (how you identify the scene).
 * Then you can add all your backgrounds / characters / art to the Background, Middleground and Foreground game objects.
 * These should contain all non-interactable elements of the scene like characters, houses etc.
 * You can add a SceneElementSelector anywhere in Background, Middleground or Foreground.
 * In the SceneElementSelector, you can set the condition under which the childs of the selector should be visible,
 * so you can hide elements based on a condition (e.g. some people are only there if the ship is there).
 * Probably all scenes have interactable buttons (dialog buttons / shop button).
 * For that there is a SceneInteractables parent game object in the Canvas GO.
 * You can add the prefab SceneInteractables to that parent game object, which contains all your buttons for the scene.
 * In the Interactables object, you can add all of the buttons for the scene (DialogButton / ShopButton).
 * Then you need to assign the SceneInteractables instance to the Scene component of the scene.
 * The Interactables automatically hide when a dialog is started or a shop is opened.
 * To add the playable character to the scene, add a PlayableCharacterSpawn to the scene (probably in the Middleground),
 * and set the CharacterSpawn in the Scene component. This automatically spawns the currently selected character.
 * You can have multiple different prefabs for your playable characters (e.g. the family has multiple arrangements of childs).
 * Add these prefabs to the PlayableCharacterData with a name, and in the PlayableCharacterSpawn you can set the name of the
 * prefab / arrangement you want to spawn in this scene.
 * 
 * In the LevelInstance object, you can now set the default scene (the main scene).
 * 
 * SCENE BUTTON
 * This is a prefab to switch the scene on click.
 * You can add it to the level (to the interactables of the scene) and set the Scene Name property in the Scene Button script.
 * Now if you press the button, the scene switches to the new scene.
 * To place it in the same position as it will appear later, no matter the aspect ratio, you can specify the background sprite as the sprite
 * in the PositionOnSprite component and set the normalized position to the normalized position on the background sprite you want the button to be.
 * 
 * ADD A DIALOG BUTTON
 * If you add a dialog button, you can set the scene name the dialog should start in in the DialogButton script.
 * This tells the level instance to open that scene if the dialog button is pressed.
 * This is different from the Scene Button in that it will return to the previous scene after the dialog is closed.
 * Also, the blur gets added to the new scene.
 * If the field stays empty, the current scene stays.
 * The dialog button automatically opens the dialog on click.
 * It also adds a blur to the background.
 * To have your characters on the left and right of the dialog, it is important that you set DialogPrefab of the DialogButton.
 * This is the prefab that will be automatically spawned on the left of the dialog (the NPC).
 * The playable character is automatically spawned to the right of the dialog.
 * Also add the sprite of the character you are talking to to the HideObjects.
 * If you want e.g. the characters that are involved in the dialog to disappear, you can add them to the DialogButton::HideObjects.
 * The objects in this list are hidden when the dialog starts and shown again when the dialog stops, so you can
 * hide characters that are in the additive scene anyway.
 * You can specify when the button is visible in the ConditionallyVisible component.
 * Also you can (and should) position the button on top of the character with the PositionOnSprite component.
 * If you don't, the button may not be where you place it in the editor, depending on the aspect ratio and resolution.
 * To place it on top of the character, specify the sprite and set the normalized position (0.5/0.5 sets it in the middle, 1/1 on the top right of the sprite).
 * 
 * ADD A SHOP
 * If you have a shop on a scene, you can add the prefab Shop to the Overlays parent game object in the Canvas object of the LevelInstance.
 * This applies for scene shops and shops during dialogs.
 * You can add a ShopButton prefab to the SceneInteractables game object of the scene it should appear on.
 * To open the shop, you need to select the shop you want to open in the ShopButton script on the ShopButton.
 * Also set the sprite and normalized position in the PositionOnSprite component to place it on top of the shop.
 * The level instance takes care of showing / hiding everything.
 */
public class LevelInstance : MonoBehaviour
{
    [SerializeField]
    private GameObject sceneParent;
    [SerializeField]
    private ForegroundScene foregroundScene;
    [SerializeField]
    private Button backButton;
    [SerializeField]
    private DialogSystem dialogSystem;
    [SerializeField]
    private Blur blur;
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
    [SerializeField]
    private Camera mainCamera;
    [SerializeField]
    private Camera uiCamera;
    [SerializeField]
    private float EndOfDayFadeTime = 20.0f;

    private List<Scene> scenes = new List<Scene>();
    private Scene currentScene;
    private Shop currentShop;
    private IEnumerable<GameObject> currentHiddenObjects;
    private string previousScene;
    private OverlayMode overlayMode = OverlayMode.None;
    private bool startedPlayingMusic = false;
    private Mode mode = Mode.None;
    private DraggedItem draggedItem;
    private bool isEndOfDayFade = false;

    private static LevelInstance instance;
    public static LevelInstance Instance { get { return instance; } }

    public IngameDiary IngameDiary { get { return ui.IngameDiary; } }
    public bool IsDragging { get { return draggedItem != null; } }
    public Shop CurrentShop { get { return currentShop; } }
    public Canvas Canvas { get { return canvas; } }
    public RectTransform CanvasRect { get { return canvas.GetComponent<RectTransform>(); } }

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

        NewGameManager.Instance.onTimeChanged += OnTimeChanged;
        backButton.onClick.AddListener(OnBack);
        blur.SetEnabled(false);
        IngameDiary.Diary.onDiaryStatusChanged += OnDiaryStatusChanged;

        foreach(Scene scene in scenes)
        {
            scene.OnActiveStatusChanged(false);
            scene.gameObject.SetActive(false);
        }

        foregroundScene.gameObject.SetActive(false);
        OpenScene(defaultScene);

        dialogSystem.gameObject.SetActive(false);
        if (diaryEntry)
        {
            blur.SetEnabled(true);
            NewGameManager.Instance.AddDiaryEntry(diaryEntry);
            backButton.gameObject.SetActive(true);
            ui.SetUIElementsVisible(InterfaceVisibilityFlags.None);
            ui.OpenDiaryImmediately(DiaryPageLink.Diary);
            sceneInteractables.SetActive(false);
            mode = Mode.Diary;
            NewGameManager.Instance.SetPaused(true);
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
            SetBackButtonVisible(true);
            ui.SetUIElementsVisible(InterfaceVisibilityFlags.None);
            blur.SetEnabled(true);
            overlayMode = OverlayMode.None;
            dialogSystem.gameObject.SetActive(true);
            dialogSystem.OnOverlayClosed();
            foregroundScene.gameObject.SetActive(true);
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
                    foregroundScene.gameObject.SetActive(false);

                    if (previousScene != null && currentScene.SceneName != previousScene)
                    {
                        // If the scene switched for a dialog temporarily, return to the previous scene.
                        OpenScene(previousScene);
                        previousScene = null;
                    }

                    currentScene.SetPlayableCharacterVisible(true);
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
                blur.SetEnabled(false);

                // Enable the buttons again.
                sceneInteractables.SetActive(true);

                mode = Mode.None;
                NewGameManager.Instance.SetPaused(false);
                UpdateEndOfDayFade();
            }
        }
    }

    private void OnTimeChanged(int hour, int minute)
    {
        if(isEndOfDayFade)
        {
            if (NewGameManager.Instance.RemainingTime > EndOfDayFadeTime)
            {
                // Time was reset -> new day
                isEndOfDayFade = false;
                UpdateEndOfDayFade();
            }
            else if(NewGameManager.Instance.RemainingTime == 0)
            {
                // Reset blur.
                ///@todo Change this when blur is enabled for popups
                isEndOfDayFade = false;
                //blur.SetEnabled(true);
                blur.SetEnabled(false);
            }
            else
            {
                UpdateEndOfDayFade();
            }
        }
        else
        {
            if (NewGameManager.Instance.RemainingTime <= EndOfDayFadeTime)
            {
                // Start fading to end of day.
                isEndOfDayFade = true;
                UpdateEndOfDayFade();
            }
        }
    }

    private void UpdateEndOfDayFade()
    {
        if(mode != Mode.None)
        {
            blur.SetEnabled(true);
        }
        else if (isEndOfDayFade)
        {
            float amount = 1.0f - Mathf.Clamp01(NewGameManager.Instance.RemainingTime / EndOfDayFadeTime);
            blur.SetFadeAmount(amount);
        }
        else
        {
            blur.SetEnabled(false);
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

    private void OnDialogStarted()
    {
        dialogSystem.gameObject.SetActive(true);
        backButton.gameObject.SetActive(true);
        ui.SetUIElementsVisible(InterfaceVisibilityFlags.None);
        mode = Mode.Dialog;
        sceneInteractables.SetActive(false);
        blur.SetEnabled(true);
        NewGameManager.Instance.SetPaused(true);
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

        currentScene.SetPlayableCharacterVisible(false);
        currentHiddenObjects = button.HideObjects;
        foreach(GameObject go in currentHiddenObjects)
        {
            go.SetActive(false);
        }

        StartDialog(button.gameObject);

        // Set foreground scene
        foregroundScene.SetCharacters(button.DialogPrefab, NewGameManager.Instance.PlayableCharacterData.dialogPrefab);
        foregroundScene.gameObject.SetActive(true);

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
        blur.SetEnabled(true);

        if (dialogSystem.gameObject.activeSelf)
        {
            // This is an overlay (a shop was opened during a dialog (the corresponding decision option was selected), so hide the dialog).
            overlayMode = OverlayMode.Shop;
            dialogSystem.gameObject.SetActive(false);
            foregroundScene.gameObject.SetActive(false);

            currentShop.onTradeAccepted += OnTradeAccepted;
        }
        else
        {
            mode = Mode.Shop;
            NewGameManager.Instance.SetPaused(true);
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
        OpenDiary(DiaryPageLink.Map);
    }

    public void OpenDiary(DiaryPageLink type)
    {
        blur.SetEnabled(true);
        ui.SetUIElementsVisible(InterfaceVisibilityFlags.None);

        if (mode == Mode.Dialog)
        {
            // This is an overlay (the travel decision option was selected during a dialog), so hide the dialog system.
            ui.OpenDiaryImmediately(type);
            overlayMode = OverlayMode.Diary;
            dialogSystem.gameObject.SetActive(false);
            foregroundScene.gameObject.SetActive(false);
        }
        else
        {
            ui.SetDiaryOpened(type);
            mode = Mode.Diary;
            sceneInteractables.SetActive(false);
            NewGameManager.Instance.SetPaused(true);
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
                    blur.SetEnabled(false);
                    sceneInteractables.SetActive(true);
                    mode = Mode.None;
                    NewGameManager.Instance.SetPaused(false);
                    UpdateEndOfDayFade();
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

        // Prepare: Render main and ui separately, not as overlay
        var mainCameraData = mainCamera.GetUniversalAdditionalCameraData();
        mainCameraData.cameraStack.Remove(uiCamera);
        var uiCameraData = uiCamera.GetUniversalAdditionalCameraData();
        uiCameraData.renderType = CameraRenderType.Base;

        // Render the camera view to a new render texture
        RenderTexture mainTexture = new RenderTexture(Screen.width, Screen.height, 16);
        mainCamera.targetTexture = mainTexture;
        mainCamera.Render();

        // Render the ui
        RenderTexture uiTexture = new RenderTexture(Screen.width, Screen.height, 16);
        uiCamera.targetTexture = uiTexture;
        uiCamera.Render();

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
        Graphics.Blit(mainTexture, resizedTexture);

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

        // Resize the ui texture to the new target size
        Graphics.Blit(uiTexture, resizedTexture);

        // Read the ui texture into a texture.
        Texture2D renderedUITexture = new Texture2D(outputWidth, outputHeight);
        if (!Mathf.Approximately(sourceAspect, outputAspect))
        {
            // Fill the background transparent
            Color[] renderedTextureColors = renderedUITexture.GetPixels();
            Color backgroundColor = new Color(0, 0, 0, 0);
            for (int i = 0; i < renderedTextureColors.Length; ++i)
            {
                renderedTextureColors[i] = backgroundColor;
            }
            renderedUITexture.SetPixels(renderedTextureColors);
        }
        renderedUITexture.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), destX, destY);
        
        // Blend the textures
        Color[] mainColors = renderedTexture.GetPixels();
        Color[] uiColors = renderedUITexture.GetPixels();
        for(int i = 0; i < mainColors.Length; ++i)
        {
            mainColors[i] = Color.Lerp(mainColors[i], uiColors[i], uiColors[i].a);
        }
        renderedTexture.SetPixels(mainColors);
        
        // Cleanup
        RenderTexture.active = null;
        mainCamera.targetTexture = null;
        uiCamera.targetTexture = null;
        mainTexture.Release();
        uiTexture.Release();
        uiCameraData.renderType = CameraRenderType.Overlay;
        mainCameraData.cameraStack.Add(uiCamera);

        // Show ui elements again
        backButton.gameObject.SetActive(wasBackButtonVisible);

        return renderedTexture;
    }

    public void OnBeginDrag(PointerEventData data, ShopInventorySlot slot)
    {
        Debug.Assert(!IsDragging);
        draggedItem = Instantiate(draggedItemPrefab, canvas.transform).GetComponent<DraggedItem>();
        draggedItem.Slot = slot;
        draggedItem.Shop = currentShop;
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
