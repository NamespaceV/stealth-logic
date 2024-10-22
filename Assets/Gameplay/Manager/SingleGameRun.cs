using Assets.Common.Scripts;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Gameplay.Manager
{
    public class SingleGameRun : MonoBehaviour
    {
        private GameManager _mgr;

        private bool _gameEnded;

        private List<Vector2Int> _playerCoords = new List<Vector2Int>();
        private int _selectedPlayerIdx = 0;
        private List<EnemyState> _enemies = new List<EnemyState>();

        [Header("Map")]
        [SerializeField] private GameObject LevelParent;
        [SerializeField] private GameObject FloorTilePrefab;
        [SerializeField] private GameObject FloorWaterTilePrefab;
        [SerializeField] private GameObject WallTilePrefab;
        [SerializeField] private GameObject PlayerPrefab;
        [SerializeField] private GameObject EnemyPrefab;
        [SerializeField] private Sprite ExitSprite;

        private HUD _hud;

        private void Start()
        {
            _mgr = GameManager.Instance;
        }

        internal void SetHud(HUD hud)
        {
            _hud = hud;
        }

        public void Init()
        {
            _gameEnded = false;
            _enemies.Clear();
            _playerCoords.Clear();
            _selectedPlayerIdx = 0;

            foreach (var tile in _mgr.GetGrid())
            {
                if (tile.GetOccupierTileType() == TileOccupierType.HERO)
                {
                    _playerCoords.Add(tile.GetCoords());
                }
                else if (tile.GetOccupierTileType() == TileOccupierType.ENEMY)
                {
                    _enemies.Add(new EnemyState(_mgr, this, tile.GetCoords()));
                }
            }

            GenerateMap();
            HideEditorMap();

            if (_playerCoords.Count == 0) {
                _gameEnded = true;
                _hud.SetMainMessage("NO PLAYER ON THE LEVEL");
                return;
            }

            _mgr.GetGrid().GetTile(_playerCoords[_selectedPlayerIdx]).SetSelected(true);
        }

        internal void Quit()
        {
            Clear3dMap();
            ShowEditorMap();
        }

        private void HideEditorMap()
        {
            foreach (var t in _mgr.GetGrid())
            {
                t.gameObject.SetActive(false);
            }
        }

        private void ShowEditorMap()
        {
            foreach (var t in _mgr.GetGrid())
            {
                t.gameObject.SetActive(true);
            }
        }

        private void Clear3dMap() {
            foreach (Transform child in LevelParent.transform)
            {
                Destroy(child.gameObject);
            }
        }


        private void GenerateMap()
        {
            Clear3dMap();

            var grid = _mgr.GetGrid();
            for (int y = 0; y < _mgr.mapHeight; y++)
            {
                for (int x = 0; x < _mgr.mapWidth; x++)
                {
                    var tile = grid.GetTile(new Vector2Int(x, y));
                    var prefab = tile.FloorType == TileFloorType.WATER ? FloorWaterTilePrefab : FloorTilePrefab;
                    GameObject floor = Instantiate(prefab, new Vector3(x, 0, y), Quaternion.identity);
                    floor.transform.SetParent(LevelParent.transform);
                    tile.SetFloor3D(floor);
                }
            }

            foreach (var tile in grid)
            {
               ProcessWalls(tile);

                switch (tile.GetOccupierTileType())
                {
                    case TileOccupierType.EMPTY:
                        break;
                    case TileOccupierType.ENEMY:
                        GameObject enemy = Instantiate(EnemyPrefab, tile.Pos, Quaternion.identity);
                        enemy.transform.SetParent(LevelParent.transform);
                        tile.currentObject = enemy;
                        break;
                    case TileOccupierType.HERO:
                        GameObject player = Instantiate(PlayerPrefab, tile.Pos, Quaternion.identity);
                        player.transform.SetParent(LevelParent.transform);
                        tile.currentObject = player;
                        break;
                }
            }
        }

        private void ProcessWalls(Tile tile)
        {
            if (tile.CheckWall(Direction.Up))
            {
                GameObject wall = Instantiate(WallTilePrefab, tile.Pos, Quaternion.identity);
                if (tile.GetWall(Direction.Up).isExit)
                    wall.GetComponentInChildren<SpriteRenderer>().sprite = ExitSprite;
                wall.transform.SetParent(LevelParent.transform);
            }
            if (tile.CheckWall(Direction.Down))
            {
                GameObject wall = Instantiate(WallTilePrefab, tile.Pos, Quaternion.Euler(new(0, 180, 0)));
                if (tile.GetWall(Direction.Down).isExit) 
                    wall.GetComponentInChildren<SpriteRenderer>().sprite = ExitSprite;
                wall.transform.SetParent(LevelParent.transform);
            }
            if (tile.CheckWall(Direction.Left))
            {
                GameObject wall = Instantiate(WallTilePrefab, tile.Pos, Quaternion.Euler(new(0, -90, 0)));
                if (tile.GetWall(Direction.Left).isExit) 
                    wall.GetComponentInChildren<SpriteRenderer>().sprite = ExitSprite;
                wall.transform.SetParent(LevelParent.transform);
            }
            if (tile.CheckWall(Direction.Right))
            {
                GameObject wall = Instantiate(WallTilePrefab, tile.Pos, Quaternion.Euler(new(0, 90, 0)));
                if (tile.GetWall(Direction.Right).isExit) 
                    wall.GetComponentInChildren<SpriteRenderer>().sprite = ExitSprite;
                wall.transform.SetParent(LevelParent.transform);
            }
        }

        public void ChangeSelection(bool deselect = true) {
            if (deselect) {
                _mgr.GetGrid().GetTile(_playerCoords[_selectedPlayerIdx]).SetSelected(false);
            }
            _selectedPlayerIdx += 1;
            _selectedPlayerIdx %= _playerCoords.Count;
            _mgr.GetGrid().GetTile(_playerCoords[_selectedPlayerIdx]).SetSelected(true);
        }

        public void MakeMove(Direction? dir)
        {
            if (_gameEnded) return;
            if (dir == null) return;

            var g = _mgr.GetGrid();
            var playerCoords = _playerCoords[_selectedPlayerIdx];
            var playerTile = g.GetTile(playerCoords);
            var targetTile = g.GetAdjacentTile(playerCoords, dir.Value);

            if (targetTile?.GetOccupierTileType() == TileOccupierType.EMPTY
                    && playerTile.AllowsMove(dir.Value))
            {
                if (targetTile.OnTileInteractable != null)
                {
                    targetTile.OnTileInteractable.Interact(this, playerCoords);
                }
                playerTile.SetTileOccupierType(TileOccupierType.EMPTY);
                playerTile.MoveObjectToTile(targetTile);
                playerTile.SetSelected(false);
                targetTile.SetTileOccupierType(TileOccupierType.HERO);
                targetTile.SetSelected(true);
                _playerCoords[_selectedPlayerIdx] = targetTile.GetCoords();
                moveEnemies();
            }
            else
            {
                if (dir != null && playerTile != null)
                {
                    playerTile.TryInteract(dir.Value, this);
                }
            }
        }

        private void moveEnemies()
        {
            foreach (var e in _enemies) {
                e.Move();
            }
        }

        public void HandleClick(Vector2Int coord, PointerEventData.InputButton button)
        {
            //do nothing
        }

        public void PlayerLost()
        {
            _gameEnded = true;
            _hud.SetMainMessage("YOU LOST");
        }

        public void Escape(Vector2Int coord)
        {
            if (_playerCoords.Contains(coord))
            {
                _playerCoords.Remove(coord);
                var t = _mgr.GetGrid().GetTile(coord);
                t.SetSelected(false);
                t.SetTileOccupierType(TileOccupierType.EMPTY);
                t.RemoveCurrentObject();
            }
            if (_playerCoords.Count == 0)
            {
                _gameEnded = true;
                _hud.SetMainMessage("YOU WON");
            }
            else
            {
                ChangeSelection(false);
            }
        }
    }

    public class EnemyState
    {
        private GameManager _mgr;
        private SingleGameRun _currentRun;
        private Vector2Int _coord;

        private bool _lastSeenInCurrentTurn;
        private bool _lastSeenPursueActive;
        private Vector2Int _lastSeenCoord;
        private Direction _lastSeenDirection;
        private int _lastSeenDistance;

        public EnemyState(GameManager mgr, SingleGameRun currentRun, Vector2Int coord)
        {
            _mgr = mgr;
            _currentRun = currentRun;
            _coord = coord;
        }

        private Tile _myTile => _mgr.GetGrid().GetTile(_coord);

        public void Move()
        {
            _lastSeenInCurrentTurn = false;

            var tile = _myTile;
            for (int dir = 0; dir < 4; ++dir)
            {
                var d = (Direction)dir;
                if (!tile.AllowsMove(d)) { continue; }
                var adjacent = _mgr.GetGrid().GetAdjacentTile(_coord, d);
                if (adjacent?.GetOccupierTileType() == TileOccupierType.HERO)
                {
                    if (adjacent.FloorType != TileFloorType.WATER)
                    {
                        _currentRun.PlayerLost();
                        moveTo(adjacent);
                    }
                    return;
                }
                if (adjacent?.GetOccupierTileType() == TileOccupierType.EMPTY)
                {
                    seekPlayer(adjacent, d);
                }
            }
            if (_lastSeenPursueActive)
            {
                var adjacent = _mgr.GetGrid().GetAdjacentTile(_coord, _lastSeenDirection);

                if (adjacent.GetOccupierTileType() != TileOccupierType.EMPTY) { return; }
                if (adjacent.FloorType == TileFloorType.WATER) { return; }


                moveTo(adjacent);

                _lastSeenDistance -= 1;
                if (_lastSeenDistance == 0)
                {
                    _lastSeenPursueActive = false;
                }
            }
        }

        private void seekPlayer(Tile tile, Direction d)
        {
            var distance = 1;
            while (tile && tile.AllowsMove(d))
            {
                distance += 1;
                tile = _mgr.GetGrid().GetAdjacentTile(tile.GetCoords(), d);
                if (tile?.GetOccupierTileType() == TileOccupierType.HERO)
                {
                    if (_lastSeenInCurrentTurn && _lastSeenDistance < distance)
                    {
                        // keep previous player seenm this turn as they ware closer
                        continue;
                    }
                    _lastSeenPursueActive = true;
                    _lastSeenInCurrentTurn = true;
                    _lastSeenDirection = d;
                    _lastSeenDistance = distance;
                    _lastSeenCoord = tile.GetCoords();
                }
            }
        }

        private void moveTo(Tile adjacent)
        {
            _myTile.SetTileOccupierType(TileOccupierType.EMPTY);
            _myTile.MoveObjectToTile(adjacent);
            adjacent.SetTileOccupierType(TileOccupierType.ENEMY);
            _coord = adjacent.GetCoords();
        }
    }
}