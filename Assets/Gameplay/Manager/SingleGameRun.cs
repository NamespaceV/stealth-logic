using Assets.Common.Scripts;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Gameplay.Manager
{
    public class SingleGameRun
    {
        private GameManager _mgr;

        private bool _gameEnded;

        private Vector2Int? _playerCoords;
        private List<EnemyState> _enemies = new List<EnemyState>();

        public SingleGameRun(GameManager manager)
        {
            _mgr = manager;

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
        }

        public void MakeMove(Direction? dir)
        {
            if (_gameEnded) return;
            if (dir == null) return;

            var g = _mgr.GetGrid();
            var playerTile = g.GetTile(_playerCoords.Value);
            var adjacentTile = g.GetAdjacentTile(_playerCoords.Value, dir.Value);

            if (adjacentTile?.GetTileType() == TileType.EMPTY
                    && playerTile.AllowsMove(dir.Value))
            {
                playerTile.SetTileType(TileType.EMPTY);
                playerTile.SetSelected(false);
                adjacentTile.SetTileType(TileType.HERO);
                adjacentTile.SetSelected(true);
                _playerCoords = adjacentTile.GetCoords();

                moveEnemies();
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
            adjacent.SetTileType(TileType.ENEMY);
            _coord = adjacent.GetCoords();
        }
    }
}
