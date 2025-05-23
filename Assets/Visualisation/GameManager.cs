﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using CoreLogic;
using CoreLogic.Grid;
using DataFormats;
using DataFormats.Helper;
using Settings;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Visualisation.HUD;
using Visualisation.Map3D;
using Visualisation.TileVisualisation;

namespace Visualisation
{
    public class GameManager :  MonoBehaviour
    {
        public static GameManager Instance;
        public GameConfigSO gameConfig;
        public GameObject LevelParent;

        public int mapWidth = 10;
        public int mapHeight = 10;

        private Grid<Tile2d> _grid = new Grid<Tile2d>();

        private Vector2Int? _selectedTileCoord;
        private int _selectedPlayerIndex;
        public bool isPlaying { get; private set; }
        private SingleGameRun _singleGameRun;

        [SerializeField] private HUD.HUD _hud;
        private Map3dManager _map3d;

        private string _cleanLevelCopy;

        public bool Is3dMapOn { get; private set; }

        private string LevelsPath => Application.dataPath + "Levels";

        public List<TextAsset> LevelFiles = new();

        private void Awake()
        {
            Instance = this;
        }

        public void Start()
        {
            var lastLevelPath = PlayerPrefs.GetString("LastLevelOpened");
            if (!string.IsNullOrEmpty(lastLevelPath) && File.Exists(lastLevelPath)) {
                var fileText = File.ReadAllText(lastLevelPath);
                var data = JsonUtility.FromJson<LevelData>(fileText);
                LoadLevelData(data);
            }
            else
            {
                genEmptyMap10x10();
            }

            _hud.OnToggle3dMap += OnToggle3dMap;
        }

        private void genEmptyMap10x10()
        {
            for (int x = 0; x < mapWidth; ++x)
            {
                for (int y = 0; y < mapHeight; ++y)
                {
                    var coord = new Vector2Int(x, y);
                    var go = Instantiate(gameConfig.Tile2DPrefab, new Vector2(coord.x, coord.y), Quaternion.identity, transform);
                    var tile = go.GetComponent<Tile2d>();
                    tile.Register(this, coord);
                    _grid.SetTile(coord, tile);
                }
            }
        }


        public Grid<Tile2d> GetGrid() { return _grid; }

        private void Update()
        {
            if (!isPlaying)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    _hud.SelectTool(_hud.GetSelectedTool() == ToolboxTool.EXIT ? ToolboxTool.WALL : ToolboxTool.EXIT);
                }
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.Tab)) SelectNextPlayer();
            }
        }

        public void TileClicked(Vector2Int coord, PointerEventData.InputButton button)
        {

            if (isPlaying)
            {
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
                var type = tile.GetOccupierTileType();
                tile.SetTileOccupierType((TileOccupierType)(((int)type + 1) % 3), null);
            }
        }

        public void OnToggle3dMap(bool turn3dOn)
        {
            Debug.Log($"GM 3d toggle on {Is3dMapOn}");
            Is3dMapOn = turn3dOn;
            if (isPlaying)
            {
                Toggle3dMap(turn3dOn);
            }
        }

        public void OnMove(InputValue moveValue) {
           
            var input = moveValue.Get<Vector2>();
            var dir = InputToDirection(input);
            if (dir == null){ return; }

            if (isPlaying)
            {
                var selectedTile = _grid.GetTile(_selectedTileCoord.Value);
                selectedTile.SetSelected(false);
                
                _singleGameRun.MakeMove(_selectedPlayerIndex, dir);
                
                if (_singleGameRun.GameState == GameState.WON)
                {
                    _hud.SetMainMessage("WON");
                    return;
                }
                if (_singleGameRun.GameState == GameState.LOST)
                {
                    _hud.SetMainMessage("LOST");
                    return;
                }
                if (_singleGameRun.GameState == GameState.ERROR)
                {
                    _hud.SetMainMessage("ERROR\n"+_singleGameRun.Errors.First());
                    return;
                }

                updateDoors();

                _selectedPlayerIndex = _selectedPlayerIndex % _singleGameRun.GetPlayerCount();
                var newPlayerCoord = _singleGameRun.GetPlayerCoords(_selectedPlayerIndex);
                _grid.GetTile(newPlayerCoord).SetSelected(true);
                _selectedTileCoord = newPlayerCoord;

                return;
            }
            // EDITOR
            if (_selectedTileCoord != null)
            {
                var tile = _grid.GetTile(_selectedTileCoord.Value);
                switch (_hud.GetSelectedTool()) {
                    case ToolboxTool.ENEMY:
                        if (dir.Value == Direction.Up) { 
                            tile.SetTileOccupierType(TileOccupierType.ENEMY, null);
                        }
                        else if (dir.Value == Direction.Down)
                        {
                            if (tile.GetOccupierTileType() == TileOccupierType.ENEMY){
                                tile.SetTileOccupierType(TileOccupierType.EMPTY, null);
                            }
                        }
                        break;
                    case ToolboxTool.HERO:
                        if (dir.Value == Direction.Up)
                        {
                            tile.SetTileOccupierType(TileOccupierType.HERO, null);
                        }
                        else if (dir.Value == Direction.Down)
                        {
                            if (tile.GetOccupierTileType() == TileOccupierType.HERO)
                            {
                                tile.SetTileOccupierType(TileOccupierType.EMPTY, null);
                            }
                        }
                        break;
                    case ToolboxTool.STONE:
                        if (dir.Value == Direction.Up)
                        {
                            tile.SetTileOccupierType(TileOccupierType.STONE, null);
                        }
                        else if (dir.Value == Direction.Down)
                        {
                            if (tile.GetOccupierTileType() == TileOccupierType.STONE)
                            {
                                tile.SetTileOccupierType(TileOccupierType.EMPTY, null);
                            }
                        }
                        break;             
                     case ToolboxTool.WATER:
                        if (dir.Value == Direction.Up)
                        {
                            tile.FloorType = TileFloorType.WATER;
                        }
                        else if (dir.Value == Direction.Down)
                        {
                            if (tile.FloorType == TileFloorType.WATER)
                            {
                                tile.FloorType = TileFloorType.EMPTY;
                            }
                        }
                        break;
                    case ToolboxTool.WALL:
                        toggleWall(_selectedTileCoord.Value, dir.Value);
                        break;
                    case ToolboxTool.EXIT:
                        tile.ToggleExit(dir.Value);
                        break;
                     case ToolboxTool.DOOR:
                        tile.ToggleDoor(dir.Value, _hud.GetSelectedDoorColor());
                        _grid.GetAdjacentTile(_selectedTileCoord.Value, dir.Value)?.ToggleDoor(dir.Value.Opposite(),  _hud.GetSelectedDoorColor());
                        break;
                     case ToolboxTool.GATE:
                        tile.ToggleGate(dir.Value,_hud.GetSelectedDoorColor());
                        _grid.GetAdjacentTile(_selectedTileCoord.Value, dir.Value)?.ToggleGate(dir.Value.Opposite(),  _hud.GetSelectedDoorColor());
                        break;
                    case ToolboxTool.RAINBOWGATE:
                        tile.ToggleRainbowGate(dir.Value);
                        _grid.GetAdjacentTile(_selectedTileCoord.Value, dir.Value)?.ToggleRainbowGate(dir.Value.Opposite());
                        break;
                    case ToolboxTool.BUTTON:
                        if (dir.Value == Direction.Up || dir.Value == Direction.Down)
                        {
                            tile.ToggleButton(_hud.GetSelectedDoorColor());
                        }
                        else
                        {
                            _hud.ToggleSelectedButtonColor(dir.Value == Direction.Right);
                        }
                        break;
                    case ToolboxTool.PORTAL:
                        if (dir.Value == Direction.Up)
                        {
                            tile.TogglePortal(_hud.GetSelectedDoorColor());
                        }
                        else if (dir.Value == Direction.Down)
                        {
                            _hud.ToggleSelectedButtonColor(dir.Value == Direction.Right);
                        }
                        break;      
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

        public LevelData SerializeCurrentLevel()
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


        private void ClearGrid()
        {
            foreach (var f in _grid)
            {
                Destroy(f.gameObject);
            }
            _grid.Clear();
        }


        public void LoadLevelData(LevelData data)
        {
            data.Migrate();

            _selectedTileCoord = null;
            ClearGrid();

            for (int x = 0; x < data.Size.x; ++x)
            {
                for (int y = 0; y < data.Size.y; ++y)
                {
                    var coord = new Vector2Int(x, y);
                    var go = Instantiate(gameConfig.Tile2DPrefab, new Vector2(coord.x, coord.y), Quaternion.identity, transform);
                    var tile = go.GetComponent<Tile2d>();
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

                var levelData = SerializeCurrentLevel();
                _cleanLevelCopy = JsonUtility.ToJson(levelData);
                _singleGameRun = SingleGameRun.Create(levelData);

                if (_singleGameRun.Errors.Any())
                {
                    _hud.SetMainMessage(_singleGameRun.Errors.First());
                }

                isPlaying = true;
                
                Debug.Log($"GM StartRun 3d  on {Is3dMapOn}");
                _map3d = new Map3dManager(this, LevelParent, gameConfig);
                _map3d.Generate();
                Toggle3dMap(Is3dMapOn);
                    
                var newPlayerCoord = _singleGameRun.GetPlayerCoords(_selectedPlayerIndex);
                _grid.GetTile(newPlayerCoord).SetSelected(true);
                _selectedTileCoord = newPlayerCoord;

                _singleGameRun.OnOccupierMoved += OccupierMoved;

                _hud.StartPlay();
            } else {
                isPlaying = false;
                _hud.EndPlay();
                Quit();
                _singleGameRun = null;
                var cleanLevel = JsonUtility.FromJson<LevelData>(_cleanLevelCopy);
                LoadLevelData(cleanLevel);
            }
        }

        private void OccupierMoved(Vector2Int from, Vector2Int to)
        {
            var occupierType = _singleGameRun.GetGrid().GetTile(to).GetOccupierTileType();
            Debug.Log($"GM OccupierMoved {from} {to} TYPE:{occupierType}");
            var fromTile = _grid.GetTile(from);
            var toTile = _grid.GetTile(to);
            fromTile.MoveObjectToTile(toTile);
            fromTile.SetTileOccupierType(TileOccupierType.EMPTY, null);
            toTile.SetTileOccupierType(occupierType, null);
        }

        internal void Quit()
        {
            _map3d.Clear();
            ShowEditorMap();
        }

        public void Toggle3dMap(bool turn3dOn)
        {
            Debug.Log("OnToggle3dMap" + turn3dOn);
            if (turn3dOn)
            {
                HideEditorMap();
                _map3d.Show();
            }
            else
            {
                ShowEditorMap();
                _map3d.Hide();
            }
        }

        private void HideEditorMap()
        {
            foreach (var t in GetGrid())
            {
                t.gameObject.SetActive(false);
            }
        }

        private void ShowEditorMap()
        {
            foreach (var t in GetGrid())
            {
                t.gameObject.SetActive(true);
            }
        }

        private void updateDoors()
        {
            foreach (var tile in GetGrid())
            {
                tile.UpdateDoors(_singleGameRun.GetButtonsState());
            }
        }

        public void SelectNextPlayer()
        {
            var selectedTile = _grid.GetTile(_selectedTileCoord.Value);
            selectedTile.SetSelected(false);
                
            _selectedPlayerIndex = (_selectedPlayerIndex + 1) % _singleGameRun.GetPlayerCount();

            var newPlayerCoord = _singleGameRun.GetPlayerCoords(_selectedPlayerIndex);
            _grid.GetTile(newPlayerCoord).SetSelected(true);
            _selectedTileCoord = newPlayerCoord;
        }
    }
}