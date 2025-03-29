using System;
using System.Collections.Generic;
using System.IO;
using DataFormats;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Visualisation.TileVisualisation;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Visualisation.HUD
{
    public enum ToolboxTool { 
        WALL,
        ENEMY,
        HERO,
        EXIT,
        WATER,
        DOOR,
        GATE,
        RAINBOWGATE,
        BUTTON,
        STONE,
        PORTAL,
    }

    public class HUD : MonoBehaviour
    {
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _loadButton;
        [SerializeField] private TMP_Dropdown _levelsDropdown;
        [SerializeField] private TMP_Text _runButtonText;
    
        [SerializeField] private TMP_Text _mainMessage;
        [SerializeField] private TMP_Text _smallMessage;

        [SerializeField] private GameObject _toolbox;
        [SerializeField] private GameObject _toolboxButtonPrefab;
    
        [SerializeField] private RawImage _doorColorPicker;

        [SerializeField] private Toggle _toggle3dMap;

        private List<Button> _toolboxButtons = new List<Button>();
        private List<bool> _toolboxButtonsSelected = new List<bool>();
        private int _selectedToolIdx = 0;
        private DoorColor _selectedDoorColor;

        public event Action<bool> OnToggle3dMap;

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
            AddToolbox("Door");
            AddToolbox("Gate");
            AddToolbox("Rainbow Gate");
            AddToolbox("Button");
            AddToolbox("Stone");
            AddToolbox("Portal");

            ToggleToolboxButton(_selectedToolIdx);

            _doorColorPicker.color = Wall2d.FromColor(GetSelectedDoorColor());

            var levelFiles = GameManager.Instance.LevelFiles;
            _levelsDropdown.options.Clear();
            foreach (var f in levelFiles)
            {
                var option = new TMP_Dropdown.OptionData(f.name);
                _levelsDropdown.options.Add(option);
            }
            
#if ! UNITY_EDITOR
            _saveButton.interactable = false;
#endif
        }

        private void ClearToolbox()
        {
            foreach (Transform child in _toolbox.transform) {
                Destroy(child.gameObject);
            }
            _toolboxButtons.Clear();
            _toolboxButtonsSelected.Clear();
        }

        public void Toggle3dMap()
        {
            OnToggle3dMap?.Invoke(_toggle3dMap.isOn);
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

        public void OnPlayClicked()
        {
            GameManager.Instance.StartRun();
        }
        
        public void OnSaveClicked()
        {
#if UNITY_EDITOR
            var idx = _levelsDropdown.value;
            var levelJsonFile = GameManager.Instance.LevelFiles[idx];
            var data = JsonUtility.ToJson(GameManager.Instance.SerializeCurrentLevel());
            File.WriteAllText(Application.dataPath+"/levels/"+levelJsonFile.name+".json", data);
            AssetDatabase.Refresh();
#endif
        }
        
        public void OnLoadClicked()
        {
            var idx = _levelsDropdown.value;
            var level = GameManager.Instance.LevelFiles[idx];
            var data = JsonUtility.FromJson<LevelData>(level.text);
            GameManager.Instance.LoadLevelData(data);
        }
        
        //
        // public void SaveLevel()
        // {
        //     if (isPlaying) return;
        //     
        //     SimpleFileBrowser.FileBrowser.ShowSaveDialog(
        //         (path) => {
        //             LevelData data = serializeCurrentLevel();
        //             File.WriteAllText(path[0], JsonUtility.ToJson(data));
        //         },
        //         onCancel: null,
        //         pickMode: SimpleFileBrowser.FileBrowser.PickMode.Files,
        //         allowMultiSelection: false,
        //         initialPath: LevelsPath,
        //         initialFilename:"level.json",
        //         title:"Select File", saveButtonText:"Save");
        // }
        
        // public void LoadLevel()
        // {
        //     if (isPlaying) return;
        //
        //     SimpleFileBrowser.FileBrowser.ShowLoadDialog(
        //         (path) => {
        //             PlayerPrefs.SetString("LastLevelOpened", path[0]);
        //             var fileText = File.ReadAllText(path[0]);
        //             var data = JsonUtility.FromJson<LevelData>(fileText);
        //             loadLevelData(data);
        //         },
        //         onCancel: null,
        //         pickMode: SimpleFileBrowser.FileBrowser.PickMode.Files,
        //         allowMultiSelection: false,
        //         initialPath: LevelsPath,
        //         initialFilename: "level.json",
        //         title: "Select File", loadButtonText: "Select");
        // }

        public void StartPlay() {
            _runButtonText.text = "Quit play";
            _saveButton.gameObject.SetActive(false);
            _loadButton.gameObject.SetActive(false);
            _levelsDropdown.gameObject.SetActive(false);
            _doorColorPicker.gameObject.SetActive(false);
            _toolbox.SetActive(false);
            SetSmallMessage("");
            SetMainMessage("");
        }

        public void EndPlay() {
            _runButtonText.text = "Play";
            _saveButton.gameObject.SetActive(true);
            _loadButton.gameObject.SetActive(true);
            _levelsDropdown.gameObject.SetActive(true);
            _doorColorPicker.gameObject.SetActive(true);
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

        public DoorColor GetSelectedDoorColor()
        {
            return _selectedDoorColor;
        }

        public void ToggleSelectedButtonColor(bool forward)
        {
            _selectedDoorColor = forward ? Wall2d.Next(_selectedDoorColor) : Wall2d.Prev(_selectedDoorColor);
            _doorColorPicker.color = Wall2d.FromColor(_selectedDoorColor);
        }
    }
}