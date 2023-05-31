using Articy.TheMigrantsChronicles;
using Articy.TheMigrantsChronicles.Features;
using Articy.Unity;
using Articy.Unity.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DialogChat : MonoBehaviour
{
    class Entry
    {
        public GameObject bubble;
        public bool isSpecial = false;
    }

    [SerializeField]
    private GameObject linePrefab;
    [SerializeField]
    private GameObject answerPrefab;
    [SerializeField, Tooltip("The vertical space between each bubble")]
    private float spacing = 50;
    [SerializeField, Tooltip("The vertical space between answer bubbles")]
    private float spacingOptions = 30;
    [SerializeField, Tooltip("How much space on the bottom should be left for the shadow to display")]
    private float paddingBottom = 8;
    [SerializeField, Tooltip("How much space at the top should be left for the speaker text")]
    private float paddingTop = 8;

    private List<Entry> entries = new List<Entry>();
    private RectTransform rectTransform;
    private List<DialogAnswerBubble> currentAnswers = new List<DialogAnswerBubble>();
    private IFlowObject pausedOn;
    private IList<Branch> availableBranches;
    private Dialog currentDialog;
    private IArticyObject currentSpecialDialog;
    /// This will be true after a template was handled for a fragment which had a template.
    private bool handledTemplate = false;

    public bool IsWaitingForDecision { get { return currentAnswers.Count > 0; } }
    public float Height { get { return rectTransform.sizeDelta.y; } }
    public Dialog CurrentDialog { get { return currentDialog; } }
    public IFlowObject PausedOn { get { return pausedOn; } }
    public bool WantsRestart
    {
        get
        {
            if(!currentDialog)
            {
                return true;
            }

            if(!string.IsNullOrWhiteSpace(currentDialog.restartCondition) && NewGameManager.Instance.conditions.HasCondition(currentDialog.restartCondition))
            {
                return true;
            }

            return false;
        }
    }

    public delegate void OnHeightChangedEvent(float height);
    public event OnHeightChangedEvent OnHeightChanged;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Play(Dialog dialog)
    {
        ResetSpecialDialog();
        if(dialog == currentDialog && !WantsRestart)
        {
            return;
        }

        NewGameManager.Instance.conditions.RemoveCondition(dialog.restartCondition);
        currentDialog = dialog;
        currentAnswers.Clear();
        DialogSystem.Instance.FlowPlayer.StartOn = dialog.ArticyObject;
    }

    public void PlaySpecial(IArticyObject specialDialog)
    {
        if(currentSpecialDialog == specialDialog)
        {
            return;
        }

        currentSpecialDialog = specialDialog;
        DialogSystem.Instance.FlowPlayer.StartOn = specialDialog;
    }

    private void ResetSpecialDialog()
    {
        if(currentSpecialDialog == null)
        {
            return;
        }

        currentSpecialDialog = null;

        float adjustment = 0.0f;
        for(int i = entries.Count - 1; i >= 0; --i)
        {
            Entry entry = entries[i];
            if(entry.isSpecial)
            {
                RectTransform bubbleTransform = entry.bubble.GetComponent<RectTransform>();
                adjustment += bubbleTransform.sizeDelta.y + spacing;
                entry.bubble.transform.SetParent(null, false);
                Destroy(entry.bubble);
            }
            else
            {
                entries.RemoveRange(i + 1, entries.Count - i - 1);
                break;
            }
        }

        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y - adjustment);
        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, -rectTransform.sizeDelta.y / 2);
        OnHeightChanged?.Invoke(rectTransform.sizeDelta.y);
    }

    public void OnFlowPlayerPaused(IFlowObject flowObject)
    {
        if(currentSpecialDialog == null)
        {
            // Don't store the current paused on, if it's a special dialog to be able to continue the normal dialog.
            pausedOn = flowObject;
        }

        GameObject bubbleGO = Instantiate(linePrefab, transform);
        AddToContent(bubbleGO);
        entries.Add(new Entry { bubble = bubbleGO, isSpecial = currentSpecialDialog != null });

        DialogBubble bubble = bubbleGO.GetComponent<DialogBubble>();
        bubble.OnHeightChanged += OnBubbleHeightChanged;
        bubble.AssignFlowObject(flowObject);

        // Reset the flag
        handledTemplate = false;

        DialogSystem.Instance.OnDialogLine(DialogSystem.Instance.GetTechnicalNameOfSpeaker(flowObject));
    }

    public void OnBranchesUpdated(IList<Branch> branches)
    {
        if(currentSpecialDialog == null)
        {
            // Don't store the branches in a special dialog to be able to continue the normal dialog.
            availableBranches = branches;
        }
    }

    public bool OnClosing()
    {
        if(pausedOn != null)
        {
            if(WantsToHandleTemplate())
            {
                // If the current pausedOn has a template, handle it.
                HandleTemplate();
                return false;
            }
            else if(IsDialogFinished())
            {
                // If the dialog is finished, end the dialog.
                OnDialogEnded();
            }
        }

        return true;
    }

    public void OnOverlayClosed()
    {
        OnTemplateHandled();
    }

    private void OnTemplateHandled()
    {
        // The template was handled.
        handledTemplate = true;

        // This is called after a shop / popup was closed after the pausedOn had a template.
        if (IsDialogFinished())
        {
            OnDialogEnded();
        }
    }

    private void OnDialogEnded()
    {
        // Call instructions on the last paused object, if there are any.
        DialogSystem.Instance.FlowPlayer.FinishCurrentPausedObject();
        if(currentDialog != null && currentDialog.setFinishedConditions != null)
        {
            NewGameManager.Instance.conditions.AddConditions(currentDialog.setFinishedConditions);
        }
    }

    private void OnBubbleHeightChanged(DialogBubble bubble, float oldHeight, float newHeight)
    {
        float adjustment = newHeight - oldHeight;
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y + adjustment);
        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, -rectTransform.sizeDelta.y / 2);

        if(bubble.gameObject != entries[entries.Count - 1].bubble)
        {
            // This is not the last bubble, so update every bubble that comes after.
            int bubbleIndex = entries.FindIndex(entry => entry.bubble == bubble.gameObject/*ReferenceEquals(entry.bubble, bubble)*/);
            for(int i = bubbleIndex + 1; i < entries.Count; ++i)
            {
                RectTransform bubbleTransform = entries[i].bubble.GetComponent<RectTransform>();
                bubbleTransform.anchoredPosition = new Vector2(bubbleTransform.anchoredPosition.x, bubbleTransform.anchoredPosition.y - adjustment);
            }
        }

        OnHeightChanged?.Invoke(rectTransform.sizeDelta.y);
    }

    private bool IsDialogFinished()
    {
        bool finished = true;
        foreach (var branch in availableBranches)
        {
            if (branch.Target is IDialogueFragment)
            {
                finished = false;
                break;
            }
        }
        return finished;
    }

    private bool WantsToHandleTemplate()
    {
        if(pausedOn != null)
        {
            return (pausedOn is Destination || 
                pausedOn is ItemAdded || 
                pausedOn is ItemRemoved ||
                pausedOn is Money_Removed ||
                pausedOn is Money_Received ||
                pausedOn is End_of_Game ||
                pausedOn is MoneyExchange ||
                pausedOn is QuestAdded ||
                pausedOn is QuestFinished) && 
                !handledTemplate;
        }

        return false;
    }

    private TransportationMethod ConvertArticyMethodToTransportationMethod(Transportation value)
    {
        TransportationMethod method = TransportationMethod.None;
        switch (value)
        {
            case Transportation.Walking: method = TransportationMethod.Walking; break;
            case Transportation.Cart: method = TransportationMethod.Cart; break;
            case Transportation.Ship: method = TransportationMethod.Ship; break;
            case Transportation.Carriage: method = TransportationMethod.Carriage; break;
            case Transportation.Tram: method = TransportationMethod.Tram; break;
            case Transportation.Train: method = TransportationMethod.Train; break;
        }
        return method;
    }

    private bool OnRouteDiscovered(ArticyObject location, IEnumerable<Transportation> value)
    {
        string currentLocation = LevelInstance.Instance.LocationName;
        string locationName = NewGameManager.Instance.LocationManager.GetLocationByTechnicalName(location.TechnicalName);
        IEnumerable<TransportationMethod> methods = value
            .Where(enumValue => enumValue != 0)
            .Select(enumValue => ConvertArticyMethodToTransportationMethod(enumValue))
            .Where(method => !NewGameManager.Instance.RouteManager.IsRouteDiscovered(currentLocation, locationName, method));
        if(methods != null && methods.Any())
        {
            GameObject popupGO = LevelInstance.Instance.PushPopup(DialogSystem.Instance.DiscoveredRoutePopup);
            DiscoveredRoutePopup popup = popupGO.GetComponent<DiscoveredRoutePopup>();
            popup.Init(locationName, methods);
            methods.ToList().ForEach(method => NewGameManager.Instance.DiscoverRoute(currentLocation, locationName, method));
            popup.OnAccepted += (popup) =>
            {
                LevelInstance.Instance.PopPopup();
                if (locationName == "Luxembourg")
                {
                    LevelInstance.Instance.OpenDiary();
                }
            };

            return true;
        }

        return false;
    }

    private void HandleTemplate()
    {
        if(pausedOn is Destination destination)
        {
            bool hasPopup = false;
            if(destination.Template.Location_Revealed_1.LocationRevealed != null)
            {
                if(destination.Template.Transportation.Transportation != 0 ||
                    destination.Template.Transportation_02.Transportation != 0 ||
                    destination.Template.Transportation_3.Transportation != 0)
                {
                    hasPopup |= OnRouteDiscovered(destination.Template.Location_Revealed_1.LocationRevealed,
                        new Transportation[] {
                        destination.Template.Transportation.Transportation,
                        destination.Template.Transportation_02.Transportation,
                        destination.Template.Transportation_3.Transportation
                        });
                }
                else
                {
                    Debug.LogError($"Invalid transporation methods in {LevelInstance.Instance.LocationName}");
                }
            }

            if(destination.Template.Location_Revealed_2.LocationRevealed != null)
            {
                if (destination.Template.Transportation_4.Transportation != 0 ||
                    destination.Template.Transportation_5.Transportation != 0 ||
                    destination.Template.Transportation_6.Transportation != 0)
                {
                    hasPopup |= OnRouteDiscovered(destination.Template.Location_Revealed_2.LocationRevealed,
                    new Transportation[]
                    {
                        destination.Template.Transportation_4.Transportation,
                        destination.Template.Transportation_5.Transportation,
                        destination.Template.Transportation_6.Transportation
                    });
                }
                else
                {
                    Debug.LogError($"Invalid transporation methods in {LevelInstance.Instance.LocationName}");
                }
            }

            if(destination.Template.Location_Revealed_3.LocationRevealed != null)
            {
                if (destination.Template.Transportation_7.Transportation != 0)
                {
                    hasPopup |= OnRouteDiscovered(destination.Template.Location_Revealed_3.LocationRevealed,
                    new Transportation[]
                    {
                        destination.Template.Transportation_7.Transportation,
                    });
                }
                else
                {
                    Debug.LogError($"Invalid transporation methods in {LevelInstance.Instance.LocationName}");
                }
            }

            if(destination.Template.Location_Revealed_4.LocationRevealed != null)
            {
                if (destination.Template.Transportation_8.Transportation != 0)
                {
                    hasPopup |= OnRouteDiscovered(destination.Template.Location_Revealed_4.LocationRevealed,
                    new Transportation[]
                    {
                        destination.Template.Transportation_8.Transportation,
                    });
                }
                else
                {
                    Debug.LogError($"Invalid transporation methods in {LevelInstance.Instance.LocationName}");
                }
            }

            if(!hasPopup)
            {
                OnTemplateHandled();
            }
        }
        else if(pausedOn is ItemAdded itemAdded)
        {
            if(itemAdded.Template.ItemGiven.ItemName != null)
            {
                string technicalName = itemAdded.Template.ItemGiven.ItemName.TechnicalName;
                Item item = NewGameManager.Instance.ItemManager.GetItemByTechnicalName(technicalName);
                if(item != null)
                {
                    if(itemAdded.Template.Secretly.Secretly)
                    {
                        NewGameManager.Instance.inventory.AddItem(item);
                        OnTemplateHandled();
                    }
                    else
                    {
                        LevelInstance.Instance.OpenShopForItemAdded(item);
                    }
                }
                else
                {
                    Debug.LogError($"Could not find item for {technicalName} for ItemAdded template");
                }
            }
            else
            {
                Debug.LogError($"{(pausedOn as IArticyObject).TechnicalName} has ItemAdded template, but the item is null");
            }
        }
        else if(pausedOn is ItemRemoved itemRemoved)
        {
            if(itemRemoved.Template.ItemTaken.ItemName != null)
            {
                string technicalName = itemRemoved.Template.ItemTaken.ItemName.TechnicalName;
                Item item = NewGameManager.Instance.ItemManager.GetItemByTechnicalName(technicalName);
                if(item != null)
                {
                    LevelInstance.Instance.OpenShopForItemRemoved(item);
                }
                else
                {
                    Debug.LogError($"Could not find item for {technicalName} for ItemRemoved template");
                }
            }
            else
            {
                Debug.LogError($"{(pausedOn as IArticyObject).TechnicalName} has ItemRemoved template, but the item is null");
            }
        }
        else if(pausedOn is Money_Removed moneyRemoved)
        {
            int amount = moneyRemoved.Template.MoneyRemoved.MoneyRemoved;
            NewGameManager.Instance.SetMoney(NewGameManager.Instance.money - amount);
            OnTemplateHandled();
        }
        else if(pausedOn is Money_Received moneyReceived)
        {
            int amount = moneyReceived.Template.MoneyAdded.MoneyAdded;
            NewGameManager.Instance.SetMoney(NewGameManager.Instance.money + amount);
            OnTemplateHandled();
        }
        else if(pausedOn is End_of_Game endOfGame)
        {
            NewGameManager.Instance.OnEndOfGame(endOfGame.Template.EndOfGame.GoodEnding, endOfGame.Template.EndOfGame.EndingName);
            OnTemplateHandled();
        }
        else if(pausedOn is MoneyExchange)
        {
            NewGameManager.Instance.SetCurrency(Currency.Dollar);
            OnTemplateHandled();
        }
        else if(pausedOn is QuestAdded questAdded)
        {
            Quest quest = NewGameManager.Instance.QuestManager.GetQuestById(questAdded.Template.QuestAdded.QuestAdded);
            bool handled = false;
            if(quest)
            {
                if(NewGameManager.Instance.QuestManager.AddQuest(quest))
                {
                    LevelInstance.Instance.OnQuestAdded(quest);
                    handled = true;
                }
            }

            if(!handled)
            {
                OnTemplateHandled();
            }
        }
        else if(pausedOn is QuestFinished questFinished)
        {
            Quest quest = NewGameManager.Instance.QuestManager.GetQuestById(questFinished.Template.QuestFinished.QuestFinished);
            bool handled = false;
            if (quest)
            {
                if (NewGameManager.Instance.QuestManager.FinishQuest(quest))
                {
                    LevelInstance.Instance.OnQuestFinished(quest);
                    handled = true;
                }
            }

            if(!handled)
            {
                OnTemplateHandled();
            }
        }
    }

    public void OnPointerClick()
    {
        if(currentSpecialDialog != null || availableBranches == null)
        {
            return;
        }

        if(WantsToHandleTemplate())
        {
            // Handle the template code
            HandleTemplate();
            // The dialog continues after the overlay was closed.
            // If the dialog is finished, this will be handled there as well.
            return;
        }

        bool isDialogFinished = IsDialogFinished();

        if(!isDialogFinished)
        {
            if (availableBranches.Count == 1)
            {
                // A linear dialog flow, so go to the next line and create a bubble.
                Branch targetBranch = availableBranches[0];
                availableBranches = null;
                DialogSystem.Instance.FlowPlayer.Play(targetBranch);
            }
            else
            {
                // Multiple branches, so it's a decision.
                if (!IsWaitingForDecision)
                {
                    foreach (var branch in availableBranches)
                    {
                        // we filter those out that are not valid
                        if (!branch.IsValid)
                        {
                            continue;
                        }

                        GameObject bubbleGO = Instantiate(answerPrefab, transform);
                        AddToContent(bubbleGO, true);
                        entries.Add(new Entry { bubble = bubbleGO });

                        DialogAnswerBubble bubble = bubbleGO.GetComponent<DialogAnswerBubble>();
                        bubble.AssignBranch(branch);
                        bubble.OnSelected += OnDecisionTaken;
                        currentAnswers.Add(bubble);
                    }

                    DialogSystem.Instance.OnDialogDecision();
                }
            }
        }
        else
        {
            OnDialogEnded();
        }
    }

    private void AddToContent(GameObject bubble, bool isOption = false)
    {
        RectTransform bubbleTransform = bubble.GetComponent<RectTransform>();
        float currentHeight = Mathf.Max(0, rectTransform.sizeDelta.y - paddingBottom);
        float newY = currentHeight + (entries.Count == 0 ? paddingTop : (isOption ? spacingOptions : spacing));
        bubbleTransform.anchoredPosition = new Vector2(bubbleTransform.anchoredPosition.x, -newY);
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, newY + bubbleTransform.sizeDelta.y + paddingBottom);
        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, -rectTransform.sizeDelta.y / 2);
        OnHeightChanged?.Invoke(rectTransform.sizeDelta.y);
    }

    private void OnDecisionTaken(DialogAnswerBubble bubble)
    {
        // Calculate the height reduction after every decision option was removed.
        float adjustment = 0.0f;
        float bubbleAdjustment = 0.0f;
        bool containsBubble = false;
        foreach(var answer in currentAnswers)
        {
            RectTransform answerTransform = answer.GetComponent<RectTransform>();
            adjustment += answerTransform.sizeDelta.y + spacingOptions;
            if(answer != bubble)
            {
                if(!containsBubble)
                {
                    bubbleAdjustment += answerTransform.sizeDelta.y + spacingOptions;
                }

                answer.transform.SetParent(null, false);
                DialogSystem.Instance.UnregisterAnimator(answer);
                entries.RemoveAll(entry => entry.bubble == answer.gameObject);
                Destroy(answer.gameObject);
            }
            else
            {
                containsBubble = true;
            }
        }

        if(!containsBubble)
        {
            // Was an old bubble that was not selected.
            return;
        }

        // Reposition the selected bubble and adjust the chat height.
        RectTransform bubbleTransform = bubble.GetComponent<RectTransform>();
        float newBubbleY = bubbleTransform.anchoredPosition.y + bubbleAdjustment;
        bubbleTransform.anchoredPosition = new Vector2(bubbleTransform.anchoredPosition.x, newBubbleY);
        float newSizeY = rectTransform.sizeDelta.y - adjustment + spacingOptions - paddingBottom;
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, newSizeY + bubbleTransform.sizeDelta.y + paddingBottom);
        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, -rectTransform.sizeDelta.y / 2);
        OnHeightChanged?.Invoke(rectTransform.sizeDelta.y);
        bubble.SetButtonEnabled(false);

        currentAnswers.Clear();
        DialogSystem.Instance.FlowPlayer.Play(bubble.Branch);
    }

    public bool IsCurrentBranch(DialogAnswerBubble bubble)
    {
        if (!availableBranches.Any(branch => branch.BranchId == bubble.Branch.BranchId))
        {
            return false;
        }

        return currentSpecialDialog == null && currentAnswers.Contains(bubble);
    }
}
