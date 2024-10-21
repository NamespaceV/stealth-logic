using Assets.Common.Scripts;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Assets.Gameplay.Manager
{
    [RequireComponent(typeof(SingleGameRun))]
    public class GameManager :  MonoBehaviour
    {
        public static GameManager Instance;

        public int mapWidth = 10;
        public int mapHeight = 10;

        public GameObject TilePrefab;
        private Grid<Tile> _grid = new Grid<Tile>();

        private Vector2Int? _selectedTileCoord;
        private bool _placingExit;
        public bool isPlaying { get; private set; }
        private SingleGameRun _singleGameRun;

        [SerializeField] private TMP_Text _runButtonText;
        private string _cleanLevelCopy;

        private string LevelsPath => Application.dataPath + "../Levels";

        private void Awake()
        {
            Instance = this;
            _singleGameRun = GetComponent<SingleGameRun>();
        }

        public void Start()
        {
            var lastLevelPath = PlayerPrefs.GetString("LastLevelOpened");
            if (!string.IsNullOrEmpty(lastLevelPath)) {
                var fileText = File.ReadAllText(lastLevelPath);
                var data = JsonUtility.FromJson<LevelData>(fileText);
                loadLevelData(data);
            }
            else
            {
                genEmptyMap10x10();
            }
        }

        private void genEmptyMap10x10()
        {
            for (int x = 0; x < mapWidth; ++x)
            {
                for (int y = 0; y < mapHeight; ++y)
                {
                    var coord = new Vector2Int(x, y);
                    var go = Instantiate(TilePrefab, new Vector2(coord.x, coord.y), Quaternion.identity, transform);
                    var tile = go.GetComponent<Tile>();
                    tile.Register(this, coord);
                    _grid.SetTile(coord, tile);
                }
            }
        }

        private void Update()
        {
            if(!isPlaying)
            {
                if (Input.GetKeyDown(KeyCode.E)) _placingExit = !_placingExit;
            }
        }

        public Grid<Tile> GetGrid() { return _grid; }

        public void TileClicked(Vector2Int coord, PointerEventData.InputButton button)
        {

            if (isPlaying)
            {
                _singleGameRun.HandleClick(coord, button);
                return;
            }


            if (button == PointerEventData.InputButton.Left)
            {
                if (_selectedTileCoord != null)
                {
                    _grid.GetTile(_selectedTileCoord.Value).SetSelected(false);
                }
                _selectedTileCoord = coord;
                _grid.GetTile(coord).SetSelected(true);

            }
            else if (button == PointerEventData.InputButton.Right)
            {
                var tile = _grid.GetTile(coord);
                var type = tile.GetTileType();
                tile.SetTileType((TileType)(((int)type + 1) % 3));
            }
        }

        public void OnMove(InputValue moveValue) {
           
            var input = moveValue.Get<Vector2>();
            var dir = InputToDirection(input);
            if (dir == null){ return; }

            if (isPlaying)
            {
                _singleGameRun.MakeMove(dir);
                return;
            }
            // EDITOR
            if (_selectedTileCoord != null)
            {
                if (_placingExit && _grid.GetTile(_selectedTileCoord.Value).CheckWall(dir.Value))
                {
                    _grid.GetTile(_selectedTileCoord.Value).GetWall(dir.Value).ToggleExit();
                    _placingExit = false;
                }
                else
                {
                    toggleWall(_selectedTileCoord.Value, dir.Value);
                }
            }
        }

        private void toggleWall(Vector2Int tileCoord, Direction dir)
        {
            Debug.Log($"Changing wall {tileCoord} {dir}");
            _grid.GetTile(tileCoord).ToggleWall(dir);
            _grid.GetAdjacentTile(tileCoord, dir)?.ToggleWall(dir.Opposite());
        }

        static Direction? InputToDirection(Vector2 input)
        {
            if (input.x > 0.25) { return Direction.Right; }
            if (input.x < -0.25) { return Direction.Left; }
            if (input.y > 0.25) { return Direction.Up; }
            if (input.y < -0.25) { return Direction.Down; }
            return null;
        }

        public void SaveLevel()
        {
            if (isPlaying) return;
            
            SimpleFileBrowser.FileBrowser.ShowSaveDialog(
                (path) => {
                    LevelData data = serializeCurrentLevel();
                    File.WriteAllText(path[0], JsonUtility.ToJson(data));
                },
                onCancel: null,
                pickMode: SimpleFileBrowser.FileBrowser.PickMode.Files,
                allowMultiSelection: false,
                initialPath: LevelsPath,
                initialFilename:"level.json",
                title:"Select File", saveButtonText:"Save");

        }

        private LevelData serializeCurrentLevel()
        {
            var data = new LevelData();
            data.Size = _grid.GetSize();
            for (var x = 0; x < data.Size.x; ++x)
            {
                data.Tiles.Add(new ListWrapper<TileData>());
                for (var y = 0; y < data.Size.y; ++y)
                {
                    data.Tiles[x].Add(_grid.GetTile(new Vector2Int(x, y)).ToData());
                }
            }
            return data;
        }

        public void LoadLevel()
        {
            if (isPlaying) return;

            SimpleFileBrowser.FileBrowser.ShowLoadDialog(
               (path) => {
                   PlayerPrefs.SetString("LastLevelOpened", path[0]);
                   var fileText = File.ReadAllText(path[0]);
                   var data = JsonUtility.FromJson<LevelData>(fileText);
                   loadLevelData(data);
               },
               onCancel: null,
               pickMode: SimpleFileBrowser.FileBrowser.PickMode.Files,
               allowMultiSelection: false,
               initialPath: LevelsPath,
               initialFilename: "level.json",
               title: "Select File", loadButtonText: "Select");
           
        }

        private void ClearGrid()
        {
            foreach (var f in _grid)
            {
                Destroy(f.gameObject);
            }
            _grid.Clear();
        }

        private void HideGrid()
        {
            foreach (var f in _grid)
            {
                f.gameObject.SetActive(false);
            }
        }

        private void loadLevelData(LevelData data)
        {
            _selectedTileCoord = null;
            ClearGrid();

            for (int x = 0; x < data.Size.x; ++x)
            {
                for (int y = 0; y < data.Size.y; ++y)
                {
                    var coord = new Vector2Int(x, y);
                    var go = Instantiate(TilePrefab, new Vector2(coord.x, coord.y), Quaternion.identity, transform);
                    var tile = go.GetComponent<Tile>();
                    tile.ReadFromData(data.Tiles[x][y]);
                    tile.Register(this, coord);
                    _grid.SetTile(coord, tile);
                }
            }
        }

        public void StartRun()
        {
            if (!isPlaying)
            {
                if (_selectedTileCoord.HasValue)
                {
                    _grid.GetTile(_selectedTileCoord.Value).SetSelected(false);
                    _selectedTileCoord = null;
                }
                _cleanLevelCopy = JsonUtility.ToJson(serializeCurrentLevel());

                isPlaying = true;
                _singleGameRun.Init();

                HideGrid();

                _runButtonText.text = "Quit play";
            } else {
                isPlaying = false;
                _singleGameRun.Quit();
                var cleanLevel = JsonUtility.FromJson<LevelData>(_cleanLevelCopy);
                loadLevelData(cleanLevel);
                _runButtonText.text = "Play";
            }
        }
    }
}