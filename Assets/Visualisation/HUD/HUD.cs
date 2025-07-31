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
        [SerializeField] private TMP_Text _runButtonText;
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _prevLevelButton;
        [SerializeField] private TMP_Dropdown _levelsDropdown;
        [SerializeField] private Button _nextLevelButton;
    
        [SerializeField] private TMP_Text _mainMessage;
        [SerializeField] private TMP_Text _smallMessage;

        [SerializeField] private GameObject _toolbox;
        [SerializeField] private GameObject _toolboxButtonPrefab;
    
        [SerializeField] private RawImage _doorColorPicker;

        [SerializeField] private Toggle _toggle3dMap;

        [SerializeField] private List<Sprite> _buttonSprites ;

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
            AddToolbox("Wall", _buttonSprites[0]);
            AddToolbox("Enemy", _buttonSprites[1]);
            AddToolbox("Hero",  _buttonSprites[2]);
            AddToolbox("Exit",  _buttonSprites[3]);
            AddToolbox("Water", _buttonSprites[4]);
            AddToolbox("Door", _buttonSprites[5]);
            AddToolbox("Gate", _buttonSprites[6]);
            AddToolbox("Rainbow Gate", _buttonSprites[7]);
            AddToolbox("Button", _buttonSprites[8]);
            AddToolbox("Stone", _buttonSprites[9]);
            AddToolbox("Portal", _buttonSprites[10]);

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

        private void AddToolbox(string button_name, Sprite sprite)
        {
            var bgo = Instantiate(_toolboxButtonPrefab, _toolbox.transform);
            bgo.GetComponentInChildren<TMP_Text>().text = button_name;
            bgo.GetComponentsInChildren<Image>()[1].sprite = sprite;
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

        public void OnNextLevel()
        {
            _levelsDropdown.value = (_levelsDropdown.value + 1) %  _levelsDropdown.options.Count;
            OnLoadClicked();
        }

        public void OnPreviousLevel()
        {
            var v = _levelsDropdown.value - 1;
            if (v < 0)
            {
                v += _levelsDropdown.options.Count;
            }
            _levelsDropdown.value = v;
            OnLoadClicked();
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
            _nextLevelButton.gameObject.SetActive(false);
            _prevLevelButton.gameObject.SetActive(false);
            _levelsDropdown.gameObject.SetActive(false);
            _doorColorPicker.gameObject.SetActive(false);
            _toolbox.SetActive(false);
            SetSmallMessage("");
            SetMainMessage("");
        }

        public void EndPlay() {
            _runButtonText.text = "Play";
            _saveButton.gameObject.SetActive(true);
            _nextLevelButton.gameObject.SetActive(true);
            _prevLevelButton.gameObject.SetActive(true);
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