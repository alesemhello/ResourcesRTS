using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    private BuildingPlacer _buildingPlacer;

    public Transform buildingMenu;
    public GameObject buildingButtonPrefab;
    public Transform resourcesUIParent;
    public GameObject gameResourceDisplayPrefab;
    public Transform selectedUnitsListParent;
    public GameObject selectedUnitDisplayPrefab;
    public Transform selectionGroupsParent;

    public Color invalidTextColor;

    private Dictionary<string, TMP_Text> _resourceTexts;
    private Dictionary<string, Button> _buildingButtons;

    public GameObject infoPanel;
    private TMP_Text _infoPanelTitleText;
    private TMP_Text _infoPanelDescriptionText;
    private Transform _infoPanelResourcesCostParent;
    public GameObject gameResourceCostPrefab;
    public GameObject selectedUnitMenu;
    private RectTransform _selectedUnitContentRectTransform;
    private RectTransform _selectedUnitButtonsRectTransform;
    private TMP_Text _selectedUnitTitleText;
    private TMP_Text _selectedUnitLevelText;
    private Transform _selectedUnitResourcesProductionParent;
    private Transform _selectedUnitActionButtonsParent;


    private void Awake()
    {
        // create texts for each in-game resource (gold, wood, stone...)
        _resourceTexts = new Dictionary<string, TMP_Text>();
        foreach (KeyValuePair<string, GameResource> pair in Globals.GAME_RESOURCES)
        {
            GameObject display = Instantiate(gameResourceDisplayPrefab, resourcesUIParent);
            display.name = pair.Key;
            _resourceTexts[pair.Key] = display.transform.Find("Text").GetComponent<TMP_Text>();
            SetResourceText(pair.Key, (pair.Value.Amount + 99));
        }

        _buildingPlacer = GetComponent<BuildingPlacer>();
        _buildingButtons = new Dictionary<string, Button>();
        for (int i = 0; i < Globals.BUILDING_DATA.Length; i++)
        {
            BuildingData data = Globals.BUILDING_DATA[i];
            GameObject button = Instantiate(buildingButtonPrefab, buildingMenu);
            button.name = data.unitName;
            button.transform.Find("Text").GetComponent<TMP_Text>().text = data.unitName;
            Button b = button.GetComponent<Button>();
            _AddBuildingButtonListener(b, i);
            _buildingButtons[data.code] = b;
            button.GetComponent<BuildingButton>().Initialize(Globals.BUILDING_DATA[i]);
        }

        Transform infoPanelTransform = infoPanel.transform;
        _infoPanelTitleText = infoPanelTransform.Find("Title").GetComponent<TMP_Text>();
        _infoPanelDescriptionText = infoPanelTransform.Find("Description").GetComponent<TMP_Text>();
        _infoPanelResourcesCostParent = infoPanelTransform.Find("ResourcesCost");
        ShowInfoPanel(false);

        // hide all selection group buttons
        for (int i = 1; i <= 9; i++)
            ToggleSelectionGroupButton(i, false);

        Transform selectedUnitMenuTransform = selectedUnitMenu.transform;
        _selectedUnitContentRectTransform = selectedUnitMenuTransform.Find("Content").GetComponent<RectTransform>();
        _selectedUnitButtonsRectTransform = selectedUnitMenuTransform.Find("Buttons").GetComponent<RectTransform>();
        _selectedUnitTitleText = selectedUnitMenuTransform.Find("Content/Title").GetComponent<TMP_Text>();
        _selectedUnitLevelText = selectedUnitMenuTransform.Find("Content/Level").GetComponent<TMP_Text>();
        _selectedUnitResourcesProductionParent = selectedUnitMenuTransform.Find("Content/ResourcesProduction");
        _selectedUnitActionButtonsParent = selectedUnitMenuTransform.Find("Buttons/SpecificActions");

        _ShowSelectedUnitMenu(false);
    }


    private void OnEnable()
    {
        EventManager.AddListener("UpdateResourceTexts", _OnUpdateResourceTexts);
        EventManager.AddListener("CheckBuildingButtons", _OnCheckBuildingButtons);

        EventManager.AddTypedListener("HoverBuildingButton", _OnHoverBuildingButton);
        EventManager.AddListener("UnhoverBuildingButton", _OnUnhoverBuildingButton);

        EventManager.AddTypedListener("SelectUnit", _OnSelectUnit);
        EventManager.AddTypedListener("DeselectUnit", _OnDeselectUnit);
    }


    private void OnDisable()
    {
        EventManager.RemoveListener("UpdateResourceTexts", _OnUpdateResourceTexts);
        EventManager.RemoveListener("CheckBuildingButtons", _OnCheckBuildingButtons);

        EventManager.RemoveTypedListener("HoverBuildingButton", _OnHoverBuildingButton);
        EventManager.RemoveListener("UnhoverBuildingButton", _OnUnhoverBuildingButton);

        EventManager.RemoveTypedListener("SelectUnit", _OnSelectUnit);
        EventManager.RemoveTypedListener("DeselectUnit", _OnDeselectUnit);
    }


    private void _AddBuildingButtonListener(Button b, int i)
    {
        b.onClick.AddListener(() => _buildingPlacer.SelectPlacedBuilding(i));
    }


    private void SetResourceText(string resource, int value)
    {
        _resourceTexts[resource].text = value.ToString();
    }


    private void _OnUpdateResourceTexts()
    {
        foreach (KeyValuePair<string, GameResource> pair in Globals.GAME_RESOURCES)
            SetResourceText(pair.Key, pair.Value.Amount);
    }


    private void _OnCheckBuildingButtons()
    {
        foreach (BuildingData data in Globals.BUILDING_DATA)
        { 
            _buildingButtons[data.code].interactable = data.CanBuy();
        }
    }


    private void _OnSelectUnit(EventManager.CustomEventData data)
    {
        _AddSelectedUnitToUIList(data.unit);
        _SetSelectedUnitMenu(data.unit);
        _ShowSelectedUnitMenu(true);
    }


    private void _OnDeselectUnit(EventManager.CustomEventData data)
    {
        _RemoveSelectedUnitFromUIList(data.unit.Code);
        if (Globals.SELECTED_UNITS.Count == 0)
            _ShowSelectedUnitMenu(false);
        else
            _SetSelectedUnitMenu(Globals.SELECTED_UNITS[Globals.SELECTED_UNITS.Count - 1].Unit);
    }


    private void _SetSelectedUnitMenu(Unit unit)
    {
        /*/ adapt content panel heights to match info to display
        int contentHeight = 60 + unit.Production.Count * 16;
        _selectedUnitContentRectTransform.sizeDelta = new Vector2(64, contentHeight);
        _selectedUnitButtonsRectTransform.anchoredPosition = new Vector2(0, -contentHeight - 20);
        _selectedUnitButtonsRectTransform.sizeDelta = new Vector2(70, Screen.height - contentHeight - 20);*/
        // update texts
        _selectedUnitTitleText.text = unit.Data.unitName;
        _selectedUnitLevelText.text = $"Level {unit.Level}";
        // clear resource production and reinstantiate new one
        foreach (Transform child in _selectedUnitResourcesProductionParent)
            Destroy(child.gameObject);
        if (unit.Production.Count > 0)
        {
            GameObject g; Transform t;
            foreach (ResourceValue resource in unit.Production)
            {
                g = GameObject.Instantiate(
                    gameResourceCostPrefab, _selectedUnitResourcesProductionParent);
                t = g.transform;
                t.Find("Text").GetComponent<Text>().text = $"+{resource.amount}";
                t.Find("Icon").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Textures/GameResources/{resource.code}");
            }
        }
    }


    private void _ShowSelectedUnitMenu(bool show)
    {
        selectedUnitMenu.SetActive(show);
    }


    private void _OnHoverBuildingButton(EventManager.CustomEventData data) 
    {
        SetInfoPanel(data.unitData);
        ShowInfoPanel(true);
    }


    public void _AddSelectedUnitToUIList(Unit unit)
    {
        // if there is another unit of the same type already selected,
        // increase the counter
        Transform alreadyInstantiatedChild = selectedUnitsListParent.Find(unit.Code);
        if (alreadyInstantiatedChild != null)
        {
            Debug.Log("_AddSelectedUnitToUIList null!");
            TMP_Text t = alreadyInstantiatedChild.Find("Count").GetComponent<TMP_Text>();
            int count = int.Parse(t.text);
            t.text = (count + 1).ToString();
        }
        // else create a brand new counter initialized with a count of 1
        else
        {
            Debug.Log("_AddSelectedUnitToUIList true!");
            GameObject g = Instantiate(selectedUnitDisplayPrefab, selectedUnitsListParent);
            g.name = unit.Code;
            Debug.Log("Unit: " + g.name);
            Transform t = g.transform;
            t.Find("Count").GetComponent<TMP_Text>().text = "1";
            t.Find("Name").GetComponent<TMP_Text>().text = unit.Data.unitName;
        }
    }


    public void _RemoveSelectedUnitFromUIList(string code)
    {
        Transform listItem = selectedUnitsListParent.Find(code);
        if (listItem == null) return;
        TMP_Text t = listItem.Find("Count").GetComponent<TMP_Text>();
        int count = int.Parse(t.text);
        count -= 1;
        if (count == 0)
        {
            DestroyImmediate(listItem.gameObject);
            Debug.Log("_RemoveSelectedUnitFromUIList:Destroyed!");
        }
        else
            t.text = count.ToString();
    }


    private void _OnUnhoverBuildingButton()
    {
        ShowInfoPanel(false);
    }


    public void SetInfoPanel(UnitData data)
    {
        // update texts
        if (data.code != "") { 
            _infoPanelTitleText.text = data.unitName;
        }
        if (data.description != "")
            _infoPanelDescriptionText.text = data.description;

        // clear resource costs and reinstantiate new ones
        foreach (Transform child in _infoPanelResourcesCostParent)
            Destroy(child.gameObject);

        if (data.cost.Count > 0)
        {
            GameObject g; Transform t;
            foreach (ResourceValue resource in data.cost)
            {
                g = Instantiate(gameResourceCostPrefab, _infoPanelResourcesCostParent);
                t = g.transform;
                t.Find("Text").GetComponent<TMP_Text>().text = resource.amount.ToString();
                t.Find("Icon").GetComponent<Image>().sprite = Resources.Load<Sprite>(
                    $"Textures/GameResources/{resource.code}");

                // check to see if resource requirement is not
                // currently met - in that case, turn the text into the "invalid" color
                if (Globals.GAME_RESOURCES[resource.code].Amount < resource.amount)
                    t.Find("Text").GetComponent<TMP_Text>().color = invalidTextColor;
            }
        }
    }


    public void ShowInfoPanel(bool show)
    {
        infoPanel.SetActive(show);
    }


    public void ToggleSelectionGroupButton(int groupIndex, bool on)
    {
        selectionGroupsParent.Find(groupIndex.ToString()).gameObject.SetActive(on);
    }
}