using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum ToolboxTool { 
    WALL,
    ENEMY,
    HERO,
    EXIT,
    WATER,
}

public class HUD : MonoBehaviour
{
    [SerializeField] private Button _saveButton;
    [SerializeField] private Button _loadButton;
    [SerializeField] private TMP_Text _runButtonText;
    
    [SerializeField] private TMP_Text _mainMessage;
    [SerializeField] private TMP_Text _smallMessage;

    [SerializeField] private GameObject _toolbox;
    [SerializeField] private GameObject _toolboxButtonPrefab;

    private List<Button> _toolboxButtons = new List<Button>();
    private List<bool> _toolboxButtonsSelected = new List<bool>();
    private int _selectedToolIdx = 0;

    public ToolboxTool GetSelectedTool() { return (ToolboxTool)_selectedToolIdx; }
    public void SelectTool(ToolboxTool tool) { ToolClicked((int)tool); }

    public void Start()
    {
        SetSmallMessage("");
        SetMainMessage("");

        ClearToolbox();
        AddToolbox("Wall");
        AddToolbox("Enemy");
        AddToolbox("Hero");
        AddToolbox("Exit");
        AddToolbox("Water");
        ToggleToolboxButton(_selectedToolIdx);
    }

    private void ClearToolbox()
    {
        foreach (Transform child in _toolbox.transform) {
            Destroy(child.gameObject);
        }
        _toolboxButtons.Clear();
        _toolboxButtonsSelected.Clear();
    }

    private void AddToolbox(string name)
    {
        var bgo = Instantiate(_toolboxButtonPrefab, _toolbox.transform);
        bgo.GetComponentInChildren<TMP_Text>().text = name;
        var b = bgo.GetComponent<Button>();
        var buttonIdx = _toolboxButtons.Count;
        b.onClick.AddListener(() => ToolClicked(buttonIdx));
        _toolboxButtons.Add(b);
        _toolboxButtonsSelected.Add(false);
    }

    public void ToolClicked(int i)
    {
        if (_selectedToolIdx != i) {
            ToggleToolboxButton(_selectedToolIdx);
            ToggleToolboxButton(i);
            _selectedToolIdx = i;
        }
    }

    private void ToggleToolboxButton(int i)
    {
        var selected = !_toolboxButtonsSelected[i];
        _toolboxButtonsSelected[i] = selected;
        var c = _toolboxButtons[i].colors;
        c.normalColor = selected ? Color.red : Color.white;
        c.selectedColor = selected ? Color.red : Color.white;
        _toolboxButtons[i].colors = c;
    }

    public void StartPlay() {
        _runButtonText.text = "Quit play";
        _saveButton.gameObject.SetActive(false);
        _loadButton.gameObject.SetActive(false);
        _toolbox.SetActive(false);
        SetSmallMessage("");
        SetMainMessage("");
    }

    public void EndPlay() {
        _runButtonText.text = "Play";
        _saveButton.gameObject.SetActive(true);
        _loadButton.gameObject.SetActive(true);
        _toolbox.SetActive(true);
        SetSmallMessage("");
        SetMainMessage("");
    }

    public void SetSmallMessage(string text) {
        _smallMessage.text = text;
        _smallMessage.enabled = !string.IsNullOrWhiteSpace(text);
    }

    public void SetMainMessage(string text)
    {
        _mainMessage.text = text;
        _mainMessage.enabled = !string.IsNullOrWhiteSpace(text);
    }

}