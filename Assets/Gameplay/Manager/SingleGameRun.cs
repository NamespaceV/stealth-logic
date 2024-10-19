using Assets.Common.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                    if (_playerCoords != null) {
                        Debug.LogWarning("Multiple Players not supported yet "+_playerCoords);
                    }
                    _playerCoords = tile.GetCoords();
                }
                else if (tile.GetTileType() == TileType.ENEMY)
                {
                    _enemies.Add(new EnemyState(_mgr, tile.GetCoords()));
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
                    && playerTile.AllowsMove(dir.Value)) {
                playerTile.SetTileType(TileType.EMPTY);
                playerTile.SetSelected(false);
                adjacentTile.SetTileType(TileType.HERO);
                adjacentTile.SetSelected(true);
                _playerCoords = adjacentTile.GetCoords();
            }

        }

        public void HandleClick(Vector2Int coord, PointerEventData.InputButton button)
        {
        }
    }

    public class EnemyState
    {
        private GameManager mgr;
        private Vector2Int coord;

        public EnemyState(GameManager mgr, Vector2Int coord)
        {
            this.mgr = mgr;
            this.coord = coord;
        }
    }
}
