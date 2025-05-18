#region

using Imperium.Console;
using Imperium.Types;
using Imperium.Util;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

#endregion

namespace Imperium.Interface.ConsoleUI;

public class ConsoleUI : BaseUI
{
    private const int MaxItemsShown = 12;

    private TMP_InputField input;

    private CanvasGroup containerGroup;

    private GameObject moreItems;
    private TMP_Text moreItemsText;

    private GameObject noResults;

    private Transform entryContainer;
    private GameObject template;

    private readonly ConsoleEntry[] entries = new ConsoleEntry[MaxItemsShown];

    private string previousInput = "";

    private int selectedIndex = -1;

    // State tracker to make sure the selection persists through query updates
    private bool selectionChangedThisFrame;

    // Whether the selection has been moved with arrow keys to move the input field's caret to the end in the next frame
    private bool hasSelectionMoved;

    protected override void InitUI()
    {
        entryContainer = container.Find("Results");
        template = container.Find("Results/Template").gameObject;
        template.SetActive(false);
        input = container.Find("Input").GetComponent<TMP_InputField>();

        containerGroup = container.GetComponent<CanvasGroup>();

        moreItems = container.Find("Results/MoreItems").gameObject;
        moreItemsText = container.Find("Results/MoreItems/Label").GetComponent<TMP_Text>();
        moreItems.SetActive(false);

        noResults = container.Find("NoResults").gameObject;
        noResults.SetActive(false);

        input.onValueChanged.AddListener(_ => OnInput(input.text));
        input.onSubmit.AddListener(_ => OnSubmit());

        Imperium.InputBindings.BaseMap.PreviousItem.performed += OnSelectPrevious;
        Imperium.InputBindings.BaseMap.NextItem.performed += OnSelectNext;
        Imperium.InputBindings.BaseMap.SelectItem.performed += OnSelectNext;

        GenerateItems();
    }

    private void GenerateItems()
    {
        for (var i = 0; i < MaxItemsShown; i++)
        {
            var currentIndex = i;
            var commandEntryObj = Instantiate(template, entryContainer);
            var commandEntry = commandEntryObj.AddComponent<ConsoleEntry>();
            commandEntry.Init(theme, _ =>
            {
                if (!selectionChangedThisFrame) SelectItemAndDeselectOthers(currentIndex);
            });

            entries[i] = commandEntry;
        }
    }

    private void SelectItemAndDeselectOthers(int index)
    {
        selectedIndex = index;
        SelectItemAndDeselectOthers();
    }

    private void SelectItemAndDeselectOthers()
    {
        for (var i = 0; i < entries.Length; i++) entries[i].SetSelected(i == selectedIndex);
    }

    private void OnSelectNext(InputAction.CallbackContext callbackContext)
    {
        if (!IsOpen) return;

        var traverseCounter = 0;
        do
        {
            if (selectedIndex == entries.Length - 1)
            {
                selectedIndex = 0;
            }
            else
            {
                selectedIndex++;
            }

            traverseCounter++;
            if (traverseCounter == entries.Length)
            {
                selectedIndex = -1;
                break;
            }
        } while (!entries[selectedIndex].gameObject.activeSelf);

        if (selectedIndex > -1) SelectItemAndDeselectOthers();

        // Put caret at the end since arrow keys move it
        // if (input.text.Length > 0)
        // {
        //     input.caretPosition = input.text.Length;
        // }
        hasSelectionMoved = true;
    }

    private void OnSelectPrevious(InputAction.CallbackContext callbackContext)
    {
        if (!IsOpen) return;

        var traverseCounter = 0;
        do
        {
            if (selectedIndex == 0)
            {
                selectedIndex = entries.Length - 1;
            }
            else
            {
                selectedIndex--;
            }

            traverseCounter++;
            if (traverseCounter == entries.Length)
            {
                selectedIndex = -1;
                break;
            }
        } while (!entries[selectedIndex].gameObject.activeSelf);

        if (selectedIndex > -1) SelectItemAndDeselectOthers();

        // Put caret at the end since arrow keys move it
        // if (input.text.Length > 0)
        // {
        //     input.caretPosition = input.text.Length;
        // }
        hasSelectionMoved = true;
    }

    private void LateUpdate()
    {
        if (hasSelectionMoved && input.text.Length > 0)
        {
            input.caretPosition = input.text.Length;
            hasSelectionMoved = false;
        }

        if (selectionChangedThisFrame) selectionChangedThisFrame = false;
    }

    private void SetMoreItemsText(int amount)
    {
        if (amount < 1)
        {
            moreItems.gameObject.SetActive(false);
            return;
        }

        moreItems.SetActive(true);
        moreItemsText.text = $"{amount} more results...";
        moreItems.transform.SetAsLastSibling();
    }

    private void OnInput(string inputText)
    {
        selectionChangedThisFrame = true;

        if (inputText.Length == 0)
        {
            for (var i = 0; i < MaxItemsShown; i++) entries[i].HideEntry();

            noResults.SetActive(false);
            moreItems.gameObject.SetActive(false);

            SelectItemAndDeselectOthers();
            return;
        }

        var currentlySelected = selectedIndex > 0 && selectedIndex < entries.Length
            ? entries[selectedIndex].Command
            : null;

        var query = ConsoleManager.ParseQuery(input.text);
        var shownCommands = Imperium.ConsoleManager.Search(query);

        // Set the newly selected index to 0 to select the first entry
        var newlySelectedIndex = 0;

        for (var i = 0; i < MaxItemsShown; i++)
        {
            if (i < shownCommands.Length)
            {
                entries[i].SetEntry(shownCommands[i], query, OnSubmit);

                // If the selected command is still in the list, move selection to that entry
                if (currentlySelected != null && currentlySelected == shownCommands[i]) newlySelectedIndex = i;
            }
            else
            {
                entries[i].HideEntry();
            }
        }

        // Select the newly selected entry
        if (shownCommands.Length > 0)
        {
            SelectItemAndDeselectOthers(newlySelectedIndex);
            noResults.SetActive(false);
        }
        else
        {
            selectedIndex = -1;
            noResults.SetActive(true);
        }

        SetMoreItemsText(shownCommands.Length - MaxItemsShown);
    }

    internal void OnSubmit()
    {
        if (selectedIndex < 0) return;

        var command = entries[selectedIndex].Command;
        var query = ConsoleManager.ParseQuery(input.text);

        if (command != null && command.Execute(query))
        {
            previousInput = input.text;
            Close();
        }
        else
        {
            // Re-focus the input field if the command did not succeed
            input.ActivateInputField();
        }
    }

    protected override void OnOpen(bool wasOpen)
    {
        if (rect && !wasOpen)
        {
            StartCoroutine(ImpUtils.Interface.SlideInAnimation(rect, Vector2.down, 20, containerGroup));
        }

        SelectItemAndDeselectOthers(0);

        input.text = previousInput;
        input.ActivateInputField();
        input.SelectAll();
    }

    protected override void OnThemePrimaryUpdate(ImpTheme themeUpdate)
    {
        ImpThemeManager.Style(
            themeUpdate,
            container,
            new StyleOverride("Input", Variant.BACKGROUND),
            new StyleOverride("Input/Border", Variant.DARKER),
            new StyleOverride("Input/Indicator", Variant.DARKER),
            new StyleOverride("Input/Indicator/Icon", Variant.DARKER),
            new StyleOverride("Results/MoreItems", Variant.DARKEST),
            new StyleOverride("Results/Template", Variant.DARKER),
            new StyleOverride("NoResults", Variant.DARKEST)
        );
    }
}