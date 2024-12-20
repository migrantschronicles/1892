// Uncomment to enable developer menu to select location
#define ENABLE_DEVELOPER_MENU

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using Articy.Unity;
using Articy.Unity.Interfaces;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.Events;
using Articy.Unity.Constraints;

public enum Mode
{
    None,
    Shop,
    Dialog,
    Diary,
    Popup
}

enum OverlayMode
{
    None,
    Shop,
    Diary,
    Popup
}

public enum LevelInstanceMode
{
    Default,
    Ship
}

/**
 * A prefab to add to each level as the root for every element.
 * This is so that new elements like the new game manager can be easily added and existing elements can be easily changed across levels.
 * It automatically switches scenes, shows/hides the back button, interactables etc.
 * 
 * Each city that you travel to is its own level (Scene asset in Unity).
 * Each level (city) can have multiple scenes (called Scene here, but they are different from the level / scene asset).
 * Each scene has a background, middleground and foreground layer for placing buildings, characters etc.
 * The LevelInstance takes care of switching between those scenes, and exists only for one level (Scene asset), so each city has its own LevelInstance.
 * 
 * If you create a new scene, this is the only prefab you need to add as a root game object in the hierarchy.
 * Everything else is inside of this level instance prefab. 
 * Also you need to delete any preexising cameras (game objects), since a camera is included in the prefab.
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
 * Also add the sprite (container) of the character you are talking to to the HideObjects.
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
 * 
 * ---------------------------
 * SHIP
 * For the ship, the LevelInstanceMode is set to Ship.
 * The level instance should only have one scene, where all the rooms go into.
 * See the documentation of Room for more information.
 * Other than that, you can add elements as usual, so shops, dialogs etc, just make sure to use the buttons with _Ship ending.
 */
public class LevelInstance : MonoBehaviour
{
    [SerializeField]
    private string locationOverride;
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
    private AudioClip[] musicClips;
    [SerializeField]
    private AudioClip auswandererLied;
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
    private LevelInstanceMode levelMode = LevelInstanceMode.Default;
    [SerializeField]
    private PDFGeneratorManager pdfGeneratorManager;
    [SerializeField]
    private GameObject roomButtonPrefab;
    [SerializeField]
    private ArticyRef mainTooHungryDialog;
    [SerializeField]
    private ArticyRef sideTooHungryDialog;
    [SerializeField]
    private ArticyRef dailyDialogLimitDialog;
    [SerializeField]
    private ArticyRef sickDialog;
    [SerializeField]
    private ArticyRef mainHomesickDialog;
    [SerializeField]
    private ArticyRef sideHomesickDialog;
    [SerializeField]
    private ArticyRef michelTooHungryDialog;
    [SerializeField]
    private ArticyRef michelDailyDialogLimitDialog;
    [SerializeField]
    private ArticyRef michelSickDialog;
    [SerializeField]
    private ArticyRef michelHomesickDialog;
    [SerializeField]
    private ArticyRef peterTooHungryDialog;
    [SerializeField]
    private ArticyRef susannaTooHungryDialog;
    [SerializeField]
    private ArticyRef punnelsDailDialogLimitDialog;
    [SerializeField]
    private ArticyRef peterSickDialog;
    [SerializeField]
    private ArticyRef susannaSickDialog;
    [SerializeField]
    private ArticyRef peterHomesickDialog;
    [SerializeField]
    private ArticyRef susannaHomesickDialog;
    [SerializeField]
    private ArticyRef foreignLanguageDialog; 
    [SerializeField]
    private DialogButton hungryIntroductoryDialogButton;
    [SerializeField]
    private GameObject seasicknessScenePrefab;
    [SerializeField, Tooltip("How many seconds (real time) the seasickness scene should be displayed")]
    private float seasicknessSceneTime = 5.0f;
    [SerializeField, Tooltip("How often the seasickness scene should appear (realtime seconds)")]
    private float seasicknessSceneFrequency = 60.0f;
    [SerializeField]
    private DialogButton introductoryDialogButton;
    [SerializeField]
    private DialogCondition introductoryDialogCondition;
    [SerializeField]
    private DialogButton[] daysDialogButtons;
    [SerializeField]
    private int maxDialogsPerDay = -1;
    [SerializeField]
    private GameObject shopPrefab;
    [SerializeField]
    private GameObject overlays;
    [SerializeField]
    private GameObject endDayPopupPrefab;
    [SerializeField]
    private GameObject endDayShipPopupPrefab;
    [SerializeField]
    private GameObject endDayElisIslandPopupPrefab;
    [SerializeField]
    private GameObject nightTransitionPrefab;
    [SerializeField]
    private float nightTransitionTime = 2.0f;
    [SerializeField]
    private GameObject startDayOutsidePrefab;
    [SerializeField]
    private GameObject startDayHostelPrefab;
    [SerializeField]
    private GameObject startDayShipPrefab;
    [SerializeField]
    private GameObject startDayElisIslandPrefab;
    [SerializeField]
    private GameObject visitCityPopupPrefab;
    [SerializeField]
    private GameObject returnFromStopoverPrefab;
    [SerializeField]
    private GameObject shipArrivedPrefab;
    [SerializeField]
    private GameObject endDayStopoverPrefab;
    [SerializeField]
    private GameObject endGameSuccessPrefab;
    [SerializeField]
    private GameObject endGameFailurePrefab;
    [SerializeField]
    private GameObject questAddedPrefab;
    [SerializeField]
    private GameObject questFinishedPrefab;
    [SerializeField]
    private GameObject questFailedPrefab;
    [SerializeField]
    private GameObject cannotTravelAgainTodayPrefab;
    [SerializeField]
    private GameObject brokePrefab;
    [SerializeField]
    private string seasicknessRemedy;
    [SerializeField]
    private HealthState overrideProtagonistAnimState;
    [SerializeField]
    private bool shouldOverrideProtagonistAnimState;
    [SerializeField]
    private GameObject developerLocationPanelPrefab;
    [SerializeField]
    private GameObject endOfGameAnimationSuccessPrefab;
    [SerializeField]
    private GameObject endOfGameAnimationFailurePreab;
    [SerializeField]
    private GameObject endOfGameAnimationNoMoneyPrefab;
    [SerializeField]
    private float endOfGameAnimationTime = 3.0f;

    private GameObject developerLocationPanel;

    private List<Scene> scenes = new List<Scene>();
    private Scene currentScene;
    private Shop currentShop;
    private IEnumerable<GameObject> currentHiddenObjects;
    private string previousScene;
    private OverlayMode overlayMode = OverlayMode.None;
    private bool startedPlayingMusic = false;
    private Mode mode = Mode.None;
    private DraggedItem draggedItem;
    private Room currentRoom;
    private GameObject seasicknessScene;
    private float seasicknessSceneTimer = -1.0f;
    private float nextSeasicknessTimer = -1.0f;
    private bool wantsToShowSeasickness = false;
    private bool hasShownIntroductoryDialog = false;
    private GameObject nightTransition;
    private bool wantsToContinueGame = false;
    private int dialogsToday = 0;
    private int detentionDialogsShown = 0;
    private bool hasShownSpecialIntroductoryDialog = false; // e.g. Hungry

    private static LevelInstance instance;
    public static LevelInstance Instance { get { return instance; } }

    public IngameDiary IngameDiary { get { return ui.IngameDiary; } }
    public Interface UI { get { return ui; } }
    public bool IsDragging { get { return draggedItem != null; } }
    public Shop CurrentShop { get { return currentShop; } }
    public Canvas Canvas { get { return canvas; } }
    public RectTransform CanvasRect { get { return canvas.GetComponent<RectTransform>(); } }
    public Scene CurrentScene { get { return currentScene; } }
    public LevelInstanceMode LevelMode { get { return levelMode; } }
    public Camera MainCamera { get { return mainCamera; } }
    public Camera UICamera { get { return uiCamera; } }
    public int DialogsToday { get { return dialogsToday; } }
    public int MaxDialogsPerDay { get { return maxDialogsPerDay; } }
    public bool IsBlurEnabled { get { return blur.IsEnabled; } }
    public bool AreSceneInteractablesEnabled { get { return sceneInteractables.activeSelf; } }
    public Room CurrentRoom { get { return currentRoom; } }
    public HealthState OverrideProtagonistAnimState { get { return overrideProtagonistAnimState; } }
    public bool ShouldOverrideProtagonistAnimState { get { return shouldOverrideProtagonistAnimState; } }
    public DialogButton IntroductoryDialogButton { get { return introductoryDialogButton; } }
    public PlayableCharacterSpawn PlayableCharacterSpawn
    {
        get
        {
            switch(levelMode)
            {
                case LevelInstanceMode.Default: return currentScene ? currentScene.PlayableCharacterSpawn : null;
                case LevelInstanceMode.Ship: return currentRoom ? currentRoom.PlayableCharacterSpawn : null;
            }

            return null;
        }
    }
    public Mode Mode { get { return mode; } }
    public bool IsShowingSeasickness { get { return seasicknessSceneTimer >= 0.0f; } }
    public string LocationName 
    { 
        get 
        { 
            if(locationOverride != null && !string.IsNullOrWhiteSpace(locationOverride))
            {
                return locationOverride;
            }

            return SceneManager.GetActiveScene().name; 
        } 
    }
    public bool IsStartLocation { get => LocationName == "Pfaffenthal" || LocationName == "Weicherdange" || LocationName == "Wormeldange"; }
    private PopupManager PopupManager { get { return GetComponent<PopupManager>(); } }

    public delegate void OnSceneChangedEvent(Scene scene);
    public event OnSceneChangedEvent onSceneChanged;

    public delegate void OnPlayableCharacterSpawnChangedEvent(PlayableCharacterSpawn spawn);
    public event OnPlayableCharacterSpawnChangedEvent onPlayableCharacterSpawnChanged;

    public delegate void OnDialogsTodayChangedEvent(int dialogsToday);
    public event OnDialogsTodayChangedEvent OnDialogsTodayChanged;

    public delegate void OnCurrentRoomChangedEvent(Room room);
    public event  OnCurrentRoomChangedEvent onCurrentRoomChanged;

    public UnityEvent OnStarted;
    public UnityEvent OnIntroductoryDialogStarted;
    public UnityEvent OnDialogClosed;

    private void Awake()
    {
        instance = this;

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
        if (string.IsNullOrWhiteSpace(defaultScene) && levelMode == LevelInstanceMode.Default)
        {
            Debug.Log("Default scene is empty in level instance");
            return;
        }

        NewGameManager.Instance.onNewDay += OnNewDay;
        backButton.onClick.AddListener(OnBack);
        SetBlurEnabled(false);
        IngameDiary.Diary.onDiaryStatusChanged += OnDiaryStatusChanged;
        foregroundScene.gameObject.SetActive(false);
        dialogSystem.gameObject.SetActive(false);
        dialogSystem.onDialogLine += OnDialogLine;
        dialogSystem.onDialogDecision += OnDialogDecision;

        switch(levelMode)
        { 
            case LevelInstanceMode.Default:
                foreach (Scene scene in scenes)
                {
                    scene.OnActiveStatusChanged(false);
                    scene.gameObject.SetActive(false);
                }

                OpenScene(defaultScene);
                break;

            case LevelInstanceMode.Ship:
                OpenScene(scenes[0]);
                break;
        }

        if(IsStartLocation)
        {
            OpenNewCityDiaryEntry();
        }

        NewGameManager.Instance.HealthStatus.SetIsOnShip(levelMode == LevelInstanceMode.Ship);
        nextSeasicknessTimer = seasicknessSceneFrequency;
        SetDialogsTodayAfterTravel();
        OnStarted?.Invoke();
    }

    private void OnDestroy()
    {
        if(NewGameManager.Instance)
        {
            NewGameManager.Instance.onNewDay -= OnNewDay;
        }
    }

    private void Update()
    {
        if(levelMode == LevelInstanceMode.Ship) { 
            if(IsShowingSeasickness)
            {
                seasicknessSceneTimer += Time.deltaTime;
                if(seasicknessSceneTimer >= seasicknessSceneTime)
                {
                    ShowSeasicknessScene(false);
                }
            }

            if(nextSeasicknessTimer > 0.0f)
            {
                nextSeasicknessTimer -= Time.deltaTime;
                if(nextSeasicknessTimer <= 0.0f)
                {
                    ProtagonistHealthData mainHealthData = NewGameManager.Instance.HealthStatus.GetMainHealthStatus();
                    if (mainHealthData.SeasicknessStatus.IsCurrentlySeasick && !NewGameManager.Instance.conditions.HasCondition(seasicknessRemedy))
                    {
                        nextSeasicknessTimer = -1.0f;
                        ShowSeasicknessScene(true);
                    }
                    else
                    {
                        nextSeasicknessTimer = seasicknessSceneFrequency;
                    }
                }

            }
         }
#if DEBUG && ENABLE_DEVELOPER_MENU
        if (Input.GetKeyDown(KeyCode.F10))
        {
            ToggleDeveloperMenu();
        }
#endif
    }

    public void ToggleDeveloperMenu()
    {
#if DEBUG && ENABLE_DEVELOPER_MENU
        if (developerLocationPanel)
        {
            if (developerLocationPanel.activeSelf)
            {
                developerLocationPanel.SetActive(false);
            }
            else
            {
                developerLocationPanel.SetActive(true);
            }
        }
        else
        {
            developerLocationPanel = Instantiate(developerLocationPanelPrefab, canvas.transform);
        }
#endif
    }

    public void OnBack()
    {
        Dialog dialogToCheckOpenShop = null;

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
                    Destroy(currentShop.gameObject);
                    currentShop = null;
                    break;
                }

                case OverlayMode.Diary:
                {
                    if (NewGameManager.Instance.wantsEndGame)
                    {
                        // The back button was pressed on the last end game diary entry, so return to the main menu.
                        NewGameManager.Instance.EndGameAndReturnToMainMenu();
                        return;
                    }

                    // The map was open, hide it.
                    AudioManager.Instance.PlayFX(ui.IngameDiary.Diary.closeClip);
                    ui.CloseDiaryImmediately();
                    break;
                }

                case OverlayMode.Popup:
                {
                    if(PopupManager.PopPopup())
                    {
                        // There are still more popups
                        return;
                    }

                    if(NewGameManager.Instance.wantsToTravel)
                    {
                        // Game manager was waiting on quest failed popups.
                        NewGameManager.Instance.GoToWantingLocation();
                        return;
                    }

                    // There are no more popups, so revert changes.
                    break;
                }
            }

            overlayMode = OverlayMode.None;

            // Overlay can happen either in the dialogs (then map / shop) or a popup.
            switch (mode)
            {
                case Mode.Dialog:
                {
                    // Readd all the necessary elements for the dialog
                    SetBackButtonVisible(true);
                    ui.SetUIElementsVisible(InterfaceVisibilityFlags.None);
                    SetBlurEnabled(true);
                    dialogSystem.gameObject.SetActive(true);
                    dialogSystem.OnOverlayClosed();
                    foregroundScene.gameObject.SetActive(true);
                    break;
                }

                case Mode.Diary:
                {
                    // An overlay over the diary can only happen if it's a popup.
                    ui.HideDiary(false);
                    ui.SetUIElementsVisible(InterfaceVisibilityFlags.None);
                    SetBackButtonVisible(true);
                    break;
                }

                case Mode.Shop:
                {
                    ui.SetUIElementsVisible(InterfaceVisibilityFlags.StatusInfo);
                    currentShop.gameObject.SetActive(true);
                    break;
                }
            }
        }
        else
        {
            // The back button has been pressed during a dialog, in a shop, in the diary, etc.
            switch (mode)
            {
                case Mode.Dialog:
                    // If the dialog was active, notify it to clear its entries.
                    dialogToCheckOpenShop = dialogSystem.CurrentChat != null ? dialogSystem.CurrentChat.CurrentDialog : null;
                    if(dialogSystem.OnClose())
                    {
                        AudioManager.Instance.PlayFX(dialogSystem.closeClip);
                        dialogSystem.gameObject.SetActive(false);
                        foregroundScene.gameObject.SetActive(false);

                        if (previousScene != null && currentScene.SceneName != previousScene)
                        {
                            // If the scene switched for a dialog temporarily, return to the previous scene.
                            OpenScene(previousScene);
                            previousScene = null;
                        }

                        PlayableCharacterSpawn?.SetCharactersVisible(true);
                        if (currentHiddenObjects != null)
                        {
                            // Reactivate all the characters that were hidden during the dialog.
                            foreach (GameObject go in currentHiddenObjects)
                            {
                                if(go)
                                {
                                    go.SetActive(true);
                                }
                            }
                            currentHiddenObjects = null;
                        }

                        OnDialogClosed?.Invoke();
                    }
                    else
                    {
                        // If the dialog system does not want to close, it opened an overlay for ItemAdded / LocationRevealed etc, so nothing to do yet.
                        return;
                    }

                    break;

                case Mode.Diary:
                    if (NewGameManager.Instance.wantsEndGame)
                    {
                        // The back button was pressed on the last end game diary entry, so return to the main menu.
                        NewGameManager.Instance.EndGameAndReturnToMainMenu();
                        return;
                    }

                    if (ui.IngameDiary.Diary.IsAnimationInProgress)
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

                case Mode.Popup:
                    if (PopupManager.PopPopup())
                    {
                        // There are still more popups
                        return;
                    }

                    if (NewGameManager.Instance.wantsToTravel)
                    {
                        // Game manager was waiting on quest failed popups.
                        NewGameManager.Instance.GoToWantingLocation();
                        return;
                    }

                    if (wantsToContinueGame)
                    {
                        ///@todo ?
                        wantsToContinueGame = true;
                    }

                    // There are no more popups, so revert changes.
                    break;
            }

            backButton.gameObject.SetActive(false);

            if (mode != Mode.Diary)
            {
                // Hide everything
                ui.SetUIElementsVisible(InterfaceVisibilityFlags.All);
                SetBlurEnabled(false);

                // Enable the buttons again.
                SetSceneInteractablesEnabled(true);

                mode = Mode.None;
                UpdateShowSeasickness();
            }
        }

        if(dialogToCheckOpenShop != null && dialogToCheckOpenShop.shopToOpenAfterDialogClose)
        {
            if(NewGameManager.Instance.conditions.HasCondition("Misc.Shop"))
            {
                // Open shop immediately
                OpenShop(dialogToCheckOpenShop.shopToOpenAfterDialogClose);
                if (dialogToCheckOpenShop.clearShopOnOpen)
                {
                    dialogToCheckOpenShop.shopToOpenAfterDialogClose.RemoveBasketItems();
                }
                NewGameManager.Instance.conditions.RemoveCondition("Misc.Shop");
            }
        }
    }

    private void OnNewDay()
    {
        nextSeasicknessTimer = seasicknessSceneFrequency;
        wantsToShowSeasickness = false;
        dialogsToday = 0;
        OnDialogsTodayChanged?.Invoke(dialogsToday);
        hasShownSpecialIntroductoryDialog = false;
    }

    private void SetDialogsTodayAfterTravel()
    {
        int travelCost = NewGameManager.Instance.ActivityPointsTravelCost;
        int remainingPoints = NewGameManager.Instance.LastActivityPointsLeft;

        if (travelCost > 0)
        {
            if(remainingPoints - travelCost < 0)
            {
                var rest = (travelCost - remainingPoints) % 24;
                var days = rest >= 16? (travelCost-remainingPoints)/24 : (travelCost - remainingPoints) / 24 +1;
                NewGameManager.Instance.SetDate(NewGameManager.Instance.date.AddDays(days));
                OnNewDay();
            }
            else
            {
                dialogsToday += travelCost;
            }
            
        }
        OnDialogsTodayChanged?.Invoke(dialogsToday);
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


        OpenScene(scene);
    }

    public void OpenScene(Scene scene)
    {
        if (currentScene)
        {
            currentScene.OnActiveStatusChanged(false);
            currentScene.gameObject.SetActive(false);
        }

        currentScene = scene;
        currentScene.gameObject.SetActive(true);
        currentScene.OnActiveStatusChanged(true);

        onSceneChanged?.Invoke(currentScene);
        if(scene.PlayableCharacterSpawn != null)
        {
            // If it's a normal scene, broadcast the event. If it's a ship, this is null.
            onPlayableCharacterSpawnChanged?.Invoke(currentScene.PlayableCharacterSpawn);
        }

        if(LocationName == "ElisIsland" && CurrentScene.SceneName == "detention")
        {
            detentionDialogsShown = 0;
            StartDialog(daysDialogButtons[0]);
            daysDialogButtons[0].gameObject.SetActive(false);
            SetBackButtonVisible(false);
        }
    }

    private void OnDialogStarted()
    {
        backButton.gameObject.SetActive(true);
        ui.SetUIElementsVisible(InterfaceVisibilityFlags.StatusInfo);
        SetSceneInteractablesEnabled(false);
        SetBlurEnabled(true);
        foregroundScene.SetCharacters(DialogSystem.Instance.LastLeftTechnicalName, DialogSystem.Instance.LastRightTechnicalName);
    }

    private void PrepareDialog(DialogButton button)
    {
        if (!string.IsNullOrWhiteSpace(button.SceneName))
        {
            previousScene = currentScene?.SceneName;
            OpenScene(button.SceneName);
        }

        mode = Mode.Dialog;
        PlayableCharacterSpawn?.SetCharactersVisible(false);
        currentHiddenObjects = button.HideObjects;
        foreach (GameObject go in currentHiddenObjects)
        {
            if(go)
            {
                go.SetActive(false);
            }
        }

        // Set foreground scene
        foregroundScene.gameObject.SetActive(true);

        dialogSystem.gameObject.SetActive(true);
        AudioManager.Instance.PlayFX(dialogSystem.openClip);
    }

    public void StartDialog(DialogButton button)
    {
        PrepareDialog(button);
        if(NewGameManager.Instance.UnderstandsDialogLanguage(button.Language))
        {
            dialogSystem.StartDialog(button, button.Language);
        }
        else
        {
            dialogSystem.StartDialog(button, foreignLanguageDialog.GetObject());
        }

        OnDialogStarted();
    }

    public void StartTooHungryDialog(DialogButton button, ProtagonistData responsibleCharacter)
    {
        PrepareDialog(button);

        IArticyObject specialDialog = null;
        switch(NewGameManager.Instance.PlayerCharacterManager.SelectedCharacter)
        {
            case CharacterType.Elis:
                specialDialog = (responsibleCharacter.isMainProtagonist ? mainTooHungryDialog : sideTooHungryDialog).GetObject();
                break;

            case CharacterType.Michel:
                specialDialog = michelTooHungryDialog.GetObject();
                break;

            case CharacterType.Punnels:
                specialDialog = (responsibleCharacter.isMainProtagonist ? peterTooHungryDialog : susannaTooHungryDialog).GetObject();
                break;
        }
        
        dialogSystem.StartDialog(button, specialDialog);
        OnDialogStarted();
    }

    public void StartSickDialog(DialogButton button, ProtagonistData responsibleCharacter)
    {
        PrepareDialog(button);

        IArticyObject specialDialog = null;
        switch(NewGameManager.Instance.PlayerCharacterManager.SelectedCharacter)
        {
            case CharacterType.Elis:
                specialDialog = sickDialog.GetObject();
                break;

            case CharacterType.Michel:
                specialDialog = michelSickDialog.GetObject();
                break;

            case CharacterType.Punnels:
                specialDialog = (responsibleCharacter.isMainProtagonist ? peterSickDialog : susannaSickDialog).GetObject();
                break;
        }

        dialogSystem.StartDialog(button, specialDialog);
        OnDialogStarted();
    }

    public void StartHomesickDialog(DialogButton button, ProtagonistData responsibleCharacter)
    {
        PrepareDialog(button);

        IArticyObject specialDialog = null;
        switch (NewGameManager.Instance.PlayerCharacterManager.SelectedCharacter)
        {
            case CharacterType.Elis:
                specialDialog = (responsibleCharacter.isMainProtagonist ? mainHomesickDialog : sideHomesickDialog).GetObject();
                break;

            case CharacterType.Michel:
                specialDialog = michelHomesickDialog.GetObject();
                break;

            case CharacterType.Punnels:
                specialDialog = (responsibleCharacter.isMainProtagonist ? peterHomesickDialog: susannaHomesickDialog).GetObject();
                break;
        }

        dialogSystem.StartDialog(button, specialDialog);
        OnDialogStarted();
    }

    public void StartDailyDialogLimitDialog(DialogButton button)
    {
        PrepareDialog(button);

        IArticyObject specialDialog = null;
        switch(NewGameManager.Instance.PlayerCharacterManager.SelectedCharacter)
        {
            case CharacterType.Elis:
                specialDialog = dailyDialogLimitDialog.GetObject();
                break;

            case CharacterType.Michel:
                specialDialog = michelDailyDialogLimitDialog.GetObject();
                break;

            case CharacterType.Punnels:
                specialDialog = punnelsDailDialogLimitDialog.GetObject();
                break;
        }

        dialogSystem.StartDialog(button, specialDialog);
        OnDialogStarted();
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
        SetSceneInteractablesEnabled(false);
        SetBlurEnabled(true);

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
        }

        currentShop.OnOpened();
        AudioManager.Instance.PlayFX(shop.openClip);
    }

    public void OpenShopForItemAdded(Item item)
    {
        Debug.Assert(overlayMode == OverlayMode.None && mode == Mode.Dialog);
        GameObject shopGO = Instantiate(shopPrefab, overlays.transform);
        Shop shop = shopGO.GetComponent<Shop>();
        shop.InitItemAdded(item);
        OpenShop(shop);
    }

    public void OpenShopForItemRemoved(Item item)
    {
        Debug.Assert(overlayMode == OverlayMode.None && mode == Mode.Dialog);
        GameObject shopGO = Instantiate(shopPrefab, overlays.transform);
        Shop shop = shopGO.GetComponent<Shop>();
        shop.InitItemRemoved(item);
        OpenShop(shop);
    }

    private void OnTradeAccepted(Dictionary<Item, int> transfers)
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
        SetBlurEnabled(true);
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
            SetSceneInteractablesEnabled(false);
        }

        AudioManager.Instance.PlayFX(ui.IngameDiary.Diary.openClip);
    }

    private void OnDiaryStatusChanged(OpenStatus status)
    {
        switch (status)
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
                    SetBlurEnabled(false);
                    SetSceneInteractablesEnabled(true);
                    mode = Mode.None;
                    UpdateShowSeasickness();

                    bool bIntroductoryDialog = false;
                    if(introductoryDialogButton != null && !hasShownIntroductoryDialog)
                    {
                        hasShownIntroductoryDialog = true;
                        introductoryDialogButton.gameObject.SetActive(false);

                        if (introductoryDialogCondition.Test())
                        {
                            StartDialog(introductoryDialogButton);
                            OnIntroductoryDialogStarted?.Invoke();
                            bIntroductoryDialog = true;
                        }
                    }

                    if (!bIntroductoryDialog)
                    {
                        if (LocationName == "ElisIsland" && CurrentScene.SceneName == "detention")
                        {
                            if (detentionDialogsShown < CurrentScene.DaysInScene)
                            {
                                StartDialog(daysDialogButtons[CurrentScene.DaysInScene]);
                                daysDialogButtons[CurrentScene.DaysInScene].gameObject.SetActive(false);
                                ++detentionDialogsShown;
                                SetBackButtonVisible(false);
                            }
                        }
                        else if (NewGameManager.Instance.HealthStatus.Characters.Any(character => character.HungryStatus.DaysWithoutEnoughFood >= 2))
                        {
                            if (hungryIntroductoryDialogButton != null && !hasShownSpecialIntroductoryDialog)
                            {
                                if (hungryIntroductoryDialogButton.Chat != null)
                                {
                                    Destroy(hungryIntroductoryDialogButton.Chat.gameObject);
                                    hungryIntroductoryDialogButton.Chat = null;
                                }

                                StartDialog(hungryIntroductoryDialogButton);
                                hungryIntroductoryDialogButton.gameObject.SetActive(false);
                                hasShownSpecialIntroductoryDialog = true;
                            }
                        }
                    }
                }
                break;
        }
    }

    private void UpdateShowSeasickness()
    {
        if (wantsToShowSeasickness)
        {
            if(NewGameManager.Instance.wantsEndOfDay)
            {
                wantsToShowSeasickness = false;
                return;
            }

            ShowSeasicknessScene(true);
        }
    }

    public void SetBackButtonVisible(bool visible)
    {
        backButton.gameObject.SetActive(visible);
    }

    public Texture2D TakeDiaryScreenshot(DiaryEntryData entry)
    {
        return pdfGeneratorManager.TakeDiaryScreenshot(entry);
    }

    /**
     * Takes a screenshot of the map and travel routes.
     * Should only be called from PDFBuilder, not manually.
     */
    public Texture2D TakeMapScreenshot()
    {
        return pdfGeneratorManager.TakeMapScreenshot();
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

    public void InstantiateRoomButton(Room room)
    {
        GameObject roomButtonGO = Instantiate(roomButtonPrefab, sceneInteractables.transform);
        RoomButton roomButton = roomButtonGO.GetComponent<RoomButton>();
        roomButton.Room = room;
        room.RoomButton = roomButton;
    }

    public bool GoToRoom(Room room)
    {
        if(!room.IsAccessible)
        {
            return false;
        }

        if(currentRoom)
        {
            currentRoom.SetVisited(false);
        }

        currentRoom = room;
        if(currentRoom)
        {
            currentRoom.SetVisited(true);
            onPlayableCharacterSpawnChanged?.Invoke(currentRoom.PlayableCharacterSpawn);
        }

        onCurrentRoomChanged?.Invoke(currentRoom);

        return true;
    }

    private void OnDialogLine(string speakerTechnicalName)
    {
        if(mode == Mode.Dialog && overlayMode == OverlayMode.None)
        {
            foregroundScene.OnDialogLine(speakerTechnicalName);
        }
    }

    private void OnDialogDecision(string speakerTechnicalName)
    {
        if(mode == Mode.Dialog && overlayMode == OverlayMode.None)
        {
            foregroundScene.OnDialogDecision(speakerTechnicalName);
        }
    }

    private void ShowSeasicknessScene(bool show)
    {
        if(IsShowingSeasickness == show)
        {
            return;
        }

        if(mode != Mode.None)
        {
            wantsToShowSeasickness = true;
            return;
        }

        if(show)
        {
            if(!seasicknessScene)
            {
                seasicknessScene = Instantiate(seasicknessScenePrefab, canvas.transform);
            }
            else
            {
                seasicknessScene.SetActive(true);
            }

            seasicknessSceneTimer = 0.0f;
        }
        else
        {
            seasicknessScene.SetActive(false);
            seasicknessSceneTimer = -1.0f;
            nextSeasicknessTimer = seasicknessSceneFrequency;
        }

        wantsToShowSeasickness = false;
    }

    public bool HasRightForegroundCharacter(string technicalName)
    {
        return foregroundScene.HasRightCharacter(technicalName);
    }

    /**
     * Shows a popup.
     * Adds it to the list, so you can go back to the previous one.
     */
    public GameObject PushPopup(GameObject prefab)
    {
        if(overlayMode != OverlayMode.None && overlayMode != OverlayMode.Popup)
        {
            Debug.LogError("There is already an overlay, and there may not be popups over overlays.");
            return null;
        }

        if(mode != Mode.Popup)
        {
            if (overlayMode == OverlayMode.None)
            {
                if (mode != Mode.None)
                {
                    // A shop / dialog etc is open, we need to hide it.
                    switch (mode)
                    {
                        case Mode.Shop:
                            ui.SetUIElementsVisible(InterfaceVisibilityFlags.None);
                            currentShop.gameObject.SetActive(false);
                            break;

                        case Mode.Diary:
                            ui.HideDiary(true);
                            break;

                        case Mode.Dialog:
                            dialogSystem.gameObject.SetActive(false);
                            foregroundScene.gameObject.SetActive(false);
                            break;
                    }

                    overlayMode = OverlayMode.Popup;
                }
                else
                {
                    // Nothing is open yet.
                    ui.SetUIElementsVisible(InterfaceVisibilityFlags.None);
                    SetSceneInteractablesEnabled(false);
                    SetBlurEnabled(true);

                    mode = Mode.Popup;
                }
            }
        }

        GameObject popupGO = Instantiate(prefab, overlays.transform);
        PopupManager.PushPopup(popupGO);
        return popupGO;
    }

    /**
     * Shows a popup.
     * Replaces all popups that may have been there before, so you can't go back.
     */
    public GameObject ShowPopup(GameObject prefab)
    {
        // Clear all the popups 
        ClearPopups();
        return PushPopup(prefab);
    }

    public void PopPopup()
    {
        Debug.Assert(mode == Mode.Popup || overlayMode == OverlayMode.Popup);
        OnBack();
    }

    public void ClearPopups()
    {
        // Clear all the popups 
        if (mode == Mode.Popup || overlayMode == OverlayMode.Popup)
        {
            while (PopupManager.Count > 0)
            {
                OnBack();
            }
        }
    }

    public void OpenEndDayPopup()
    {
        GameObject popup;
        if(LocationName == "ElisIsland")
        {
            popup = endDayElisIslandPopupPrefab;
        }
        else if(levelMode == LevelInstanceMode.Ship)
        {
            popup = endDayShipPopupPrefab;
        }
        else if(NewGameManager.Instance.ShipManager.IsStopoverDay)
        {
            popup = endDayStopoverPrefab;
        }
        else
        {
            popup = endDayPopupPrefab;
        }

        ShowPopup(popup);
        wantsToContinueGame = true;
    }

    private void OpenNewDiaryEntry(DiaryEntryData data)
    {
        SetBlurEnabled(true);
        NewGameManager.Instance.AddDiaryEntry(data);
        backButton.gameObject.SetActive(true);
        ui.SetUIElementsVisible(InterfaceVisibilityFlags.None);
        ui.OpenDiaryImmediately(DiaryPageLink.Diary);
        SetSceneInteractablesEnabled(false);
        mode = Mode.Diary;
    }

    public void OpenNewDayDiaryEntry()
    {
        DiaryEntryInfo info = NewGameManager.Instance.CollectDiaryEntryInfo(GeneratedDiaryEntryPurpose.NewDay);
        DiaryEntryData newDayEntry = NewGameManager.Instance.DiaryEntryManager.GenerateEntry(info);
        if(newDayEntry == null)
        {
            return;
        }

        OpenNewDiaryEntry(newDayEntry);
    }

    public void OpenNewCityDiaryEntry()
    {
        DiaryEntryInfo info = NewGameManager.Instance.CollectDiaryEntryInfo(GeneratedDiaryEntryPurpose.NewCity);
        DiaryEntryData newEntry = NewGameManager.Instance.DiaryEntryManager.GenerateEntry(info);
        if(newEntry == null)
        {
            return;
        }

        OpenNewDiaryEntry(newEntry);
    }

    public void OpenEndGameDiaryEntry(string technicalName)
    {
        ClearPopups();

        DiaryEntryInfo info = NewGameManager.Instance.CollectDiaryEntryInfo(GeneratedDiaryEntryPurpose.EndGame);
        info.endGameEntryTechnicalName = technicalName;
        DiaryEntryData newEntry = NewGameManager.Instance.DiaryEntryManager.GenerateEntry(info);
        if(newEntry == null)
        {
            return;
        }

        OpenNewDiaryEntry(newEntry);
    }

    public void OnSleepOutside(List<EndOfDayHealthData> endOfDayHealthData)
    {
        wantsToContinueGame = false;
        StartCoroutine(SleepOutsideTransition(endOfDayHealthData));
    }

    private IEnumerator SleepOutsideTransition(List<EndOfDayHealthData> endOfDayHealthData)
    {
        nightTransition = Instantiate(nightTransitionPrefab, canvas.transform);
        yield return new WaitForSeconds(nightTransitionTime);
        Destroy(nightTransition);

        List<StolenItemInfo> stolenItems = NewGameManager.Instance.OnSleepOutside(endOfDayHealthData);
        GameObject popupGO = ShowPopup(startDayOutsidePrefab);
        StartDayOutsidePopup popup = popupGO.GetComponent<StartDayOutsidePopup>();
        popup.Init(stolenItems);
        popup.OnStartDay += (p) => { PopPopup(); OpenNewDayDiaryEntry(); };
    }

    public void OnSleepInHostel(List<EndOfDayHealthData> endOfDayHealthData, int cost, int boughtFoodAmount)
    {
        wantsToContinueGame = false;
        StartCoroutine(SleepInHostelTransition(endOfDayHealthData, cost, boughtFoodAmount));
    }

    private IEnumerator SleepInHostelTransition(List<EndOfDayHealthData> endOfDayHealthData, int cost, int boughtFoodAmount)
    {
        nightTransition = Instantiate(nightTransitionPrefab, canvas.transform);
        yield return new WaitForSeconds(nightTransitionTime);
        Destroy(nightTransition);

        NewGameManager.Instance.OnSleepInHostel(endOfDayHealthData, cost, boughtFoodAmount);
        GameObject popupGO = ShowPopup(startDayHostelPrefab);
        StartDayHostelPopup popup = popupGO.GetComponent<StartDayHostelPopup>();
        popup.OnStartDay += (p) => { PopPopup(); OpenNewDayDiaryEntry(); };
    }

    public void OnSleepInElisIsland(List<EndOfDayHealthData> endOfDayHealthData)
    {
        wantsToContinueGame = false;
        StartCoroutine(SleepInElisIslandTransition(endOfDayHealthData));
    }

    private IEnumerator SleepInElisIslandTransition(List<EndOfDayHealthData> endOfDayHealthData)
    {
        nightTransition = Instantiate(nightTransitionPrefab, canvas.transform);
        yield return new WaitForSeconds(nightTransitionTime);
        Destroy(nightTransition);

        NewGameManager.Instance.OnSleepInElisIsland(endOfDayHealthData);
        GameObject popupGO = ShowPopup(startDayElisIslandPrefab);
        StartDayElisIslandPopup popup = popupGO.GetComponent<StartDayElisIslandPopup>();
        popup.OnStartDay += (p) => { PopPopup(); OpenNewDayDiaryEntry(); };
    }

    public void OnSleepInShip(List<EndOfDayHealthData> endOfDayHealthData)
    {
        wantsToContinueGame = false;
        nextSeasicknessTimer = -1.0f;
        seasicknessSceneTimer = -1.0f;
        wantsToShowSeasickness = false;
        StartCoroutine(SleepInShipTransition(endOfDayHealthData));
    }

    private IEnumerator SleepInShipTransition(List<EndOfDayHealthData> endOfDayHealthData)
    {
        nightTransition = Instantiate(nightTransitionPrefab, canvas.transform);
        yield return new WaitForSeconds(nightTransitionTime);
        Destroy(nightTransition);

        if(NewGameManager.Instance.OnSleepInShip(endOfDayHealthData))
        {
            yield break;
        }

        if (NewGameManager.Instance.ShipManager.IsStopoverDay)
        {
            // Can visit city.
            GameObject popupGO = ShowPopup(visitCityPopupPrefab);
            VisitCityPopup popup = popupGO.GetComponent<VisitCityPopup>();
            popup.SetDestinationCity(NewGameManager.Instance.ShipManager.StopoverLocation);
            popup.OnStayOnBoard += (_) => { PopPopup(); OpenNewDayDiaryEntry(); };
            popup.OnVisit += (_) =>
            {
                NewGameManager.Instance.VisitStopover();
                PopPopup();
            };
        }
        else
        {
            // Normal day on ship.
            GameObject popupGO = ShowPopup(startDayShipPrefab);
            StartDayShipPopup popup = popupGO.GetComponent<StartDayShipPopup>();
            popup.OnStartDay += (p) => { PopPopup(); OpenNewDayDiaryEntry(); };
        }
    }

    public void OnReturnFromStopover(bool wantsEndOfDay)
    {
        NewGameManager.Instance.ReturnToShip(wantsEndOfDay);
    }

    public void SetInterfaceVisibilityFlags(InterfaceVisibilityFlags flags)
    {
        ui.SetUIElementsVisible(flags);
    }

    public void OnShipArrived()
    {
        GameObject popupGO = ShowPopup(shipArrivedPrefab);
        ShipArrivedPopup popup = popupGO.GetComponent<ShipArrivedPopup>();
        popup.OnLeaveShip += (_) =>
        {
            PopPopup();
            NewGameManager.Instance.OnLeaveShip();
        };
    }

    public void OnEndOfGame(bool success, string technicalName)
    {
        StartCoroutine(EndOfGameTransition(success, technicalName));
    }

    private IEnumerator EndOfGameTransition(bool success, string technicalName)
    {
        if (mode != Mode.None)
        {
            // A shop / dialog etc is open, we need to hide it.
            switch (mode)
            {
                case Mode.Shop:
                    currentShop.gameObject.SetActive(false);
                    break;

                case Mode.Diary:
                    ui.HideDiary(true);
                    break;

                case Mode.Dialog:
                    dialogSystem.gameObject.SetActive(false);
                    foregroundScene.gameObject.SetActive(false);
                    break;
            }

            ui.SetUIElementsVisible(InterfaceVisibilityFlags.None);
            SetBackButtonVisible(false);
        }
        else
        {
            // Nothing is open yet.
            ui.SetUIElementsVisible(InterfaceVisibilityFlags.None);
            SetSceneInteractablesEnabled(false);
            SetBlurEnabled(true);
        }

        GameObject prefab = success ? endOfGameAnimationSuccessPrefab : endOfGameAnimationFailurePreab;
        GameObject go = Instantiate(prefab, canvas.transform);
        yield return new WaitForSeconds(endOfGameAnimationTime);
        Destroy(go);

        if (success)
        {
            GameObject popupGO = ShowPopup(endGameSuccessPrefab);
            EndGameSuccessPopup popup = popupGO.GetComponent<EndGameSuccessPopup>();
            popup.TechnicalName = technicalName;
        }
        else
        {
            GameObject popupGO = ShowPopup(endGameFailurePrefab);
            EndGamePopup popup = popupGO.GetComponent<EndGamePopup>();
            popup.TechnicalName = technicalName;
        }
    }

    public void OnQuestAdded(Quest quest)
    {
        GameObject popupGO = PushPopup(questAddedPrefab);
        NewQuestPopup popup = popupGO.GetComponent<NewQuestPopup>();
        popup.OnAccept += (_) => { PopPopup(); };
    }

    public void OnQuestFinished(Quest quest)
    {
        GameObject popupGO = PushPopup(questFinishedPrefab);
        QuestFinishedPopup popup = popupGO.GetComponent<QuestFinishedPopup>();
        popup.OnAccept += (_) => { PopPopup(); };
    }

    public void OnQuestFailed(Quest quest)
    {
        GameObject popupGO = PushPopup(questFailedPrefab);
        QuestFailedPopup popup = popupGO.GetComponent<QuestFailedPopup>();
        popup.OnAccept += (_) => { PopPopup(); };
    }

    public bool TryCanStartDialog()
    {
        if(maxDialogsPerDay >= 0 && dialogsToday >= maxDialogsPerDay)
        {
            return false;
        }

        ++dialogsToday;
        OnDialogsTodayChanged?.Invoke(dialogsToday);
        return true;
    }

    public void OnCannotTravelAgainToday()
    {
        GameObject popupGO = PushPopup(cannotTravelAgainTodayPrefab);
        CannotTravelAgainTodayPopup popup = popupGO.GetComponent<CannotTravelAgainTodayPopup>();
        popup.OnAccept += (_) =>
        {
            PopPopup();
        };
    }

    public void SetBlurEnabled(bool enabled)
    {
        blur.SetEnabled(enabled);
    }

    public void SetSceneInteractablesEnabled(bool enabled)
    {
        sceneInteractables.SetActive(enabled);
    }

    public void ShowBrokePopup()
    {
        GameObject popupGO = PushPopup(brokePrefab);
        BrokePopup popup = popupGO.GetComponent<BrokePopup>();
        popup.OnDownloadPDF += (_) =>
        {
            NewGameManager.Instance.GeneratePDF();
        };
        popup.OnMainMenu += (_) =>
        {
            NewGameManager.Instance.ReturnToMainMenu();
        };
        popup.OnContinue += (_) =>
        {
            PopPopup();
        };
    }
}
