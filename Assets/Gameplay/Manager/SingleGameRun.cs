using Assets.Common.Scripts;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Gameplay.Manager
{
    public class SingleGameRun : MonoBehaviour
    {
        private GameManager _mgr;

        private bool _gameEnded;

        private Vector2Int? _playerCoords;
        private List<EnemyState> _enemies = new List<EnemyState>();

        [Header("Map")]
        [SerializeField] private GameObject LevelParent;
        [SerializeField] private GameObject FloorTilePrefab;
        [SerializeField] private GameObject WallTilePrefab;
        [SerializeField] private GameObject PlayerPrefab;
        [SerializeField] private GameObject EnemyPrefab;
        [SerializeField] private Sprite ExitSprite;

        private GameObject player;

        private void Start()
        {
            _mgr = GameManager.Instance;
        }

        public void Init()
        {
            foreach (var tile in _mgr.GetGrid())
            {
                if (tile.GetTileType() == TileType.HERO)
                {
                    if (_playerCoords != null)
                    {
                        Debug.LogWarning("Multiple Players not supported yet " + _playerCoords);
                    }
                    _playerCoords = tile.GetCoords();
                }
                else if (tile.GetTileType() == TileType.ENEMY)
                {
                    _enemies.Add(new EnemyState(_mgr, this, tile.GetCoords()));
                }
            }
            _mgr.GetGrid().GetTile(_playerCoords.Value).SetSelected(true);
            GenerateMap();
        }

        private void GenerateMap()
        {
            foreach(Transform child in LevelParent.transform) Destroy(child.gameObject);

            var grid = _mgr.GetGrid();

            for (int y = 0; y < _mgr.mapHeight; y++)
            {
                for (int x = 0; x < _mgr.mapWidth; x++)
                {
                    GameObject floor = Instantiate(FloorTilePrefab, new Vector3(x, 0, y), Quaternion.identity);
                    floor.transform.SetParent(LevelParent.transform);
                }
            }

            foreach (var tile in grid)
            {
               ProcessWalls(tile);

                switch (tile.GetTileType())
                {
                    case TileType.EMPTY:
                        break;
                    case TileType.ENEMY:
                        GameObject enemy = Instantiate(EnemyPrefab, tile.Pos, Quaternion.identity);
                        enemy.transform.SetParent(LevelParent.transform);
                        tile.currentObject = enemy;
                        break;
                    case TileType.HERO:
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

        public void MakeMove(Direction? dir)
        {
            if (_gameEnded) return;
            if (dir == null) return;

            var g = _mgr.GetGrid();
            var playerTile = g.GetTile(_playerCoords.Value);
            var targetTile = g.GetAdjacentTile(_playerCoords.Value, dir.Value);

            if (targetTile?.GetTileType() == TileType.EMPTY
                    && playerTile.AllowsMove(dir.Value))
            {
                if (targetTile.OnTileInteractable != null) targetTile.OnTileInteractable.Interact();
                playerTile.SetTileType(TileType.EMPTY);
                playerTile.MoveObjectToTile(targetTile);
                playerTile.SetSelected(false);
                targetTile.SetTileType(TileType.HERO);
                targetTile.SetSelected(true);
                _playerCoords = targetTile.GetCoords();             
                moveEnemies();
            }
            else
            {
                if(dir != null && playerTile != null) playerTile.TryInteract(dir.Value);
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
                if (adjacent?.GetTileType() == TileType.HERO)
                {
                    _currentRun.PlayerLost();
                    moveTo(adjacent);
                    return;
                }
                if (adjacent?.GetTileType() == TileType.EMPTY)
                {
                    seekPlayer(adjacent, d);
                }
            }
            if (_lastSeenPursueActive)
            {
                var adjacent = _mgr.GetGrid().GetAdjacentTile(_coord, _lastSeenDirection);

                if (adjacent.GetTileType() != TileType.EMPTY) { return; }

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
                if (tile?.GetTileType() == TileType.HERO)
                {
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
            _myTile.SetTileType(TileType.EMPTY);
            _myTile.MoveObjectToTile(adjacent);
            adjacent.SetTileType(TileType.ENEMY);
            _coord = adjacent.GetCoords();
        }
    }
}