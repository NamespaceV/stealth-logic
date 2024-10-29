using Assets.Common.Scripts;
using Assets.Gameplay.Manager;
using UnityEngine;

namespace Gameplay.Manager.SingleRun
{
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

            SeekForPlayers();
            
            if (_lastSeenPursueActive)
            {
                var adjacent = _mgr.GetGrid().GetAdjacentTile(_coord, _lastSeenDirection);
                if (adjacent.FloorType == TileFloorType.WATER) { return; }
                if (adjacent.GetOccupierTileType() == TileOccupierType.HERO)
                {
                    _currentRun.PlayerLost();
                    moveTo(adjacent);
                    return;
                }
                if (adjacent.GetOccupierTileType() != TileOccupierType.EMPTY) { return; }
                
                moveTo(adjacent);
                _lastSeenDistance -= 1;
                if (_lastSeenDistance == 0)
                {
                    _lastSeenPursueActive = false;
                }
                
                SeekForPlayers();
            }
        }

        private void SeekForPlayers()
        {
            for (int dir = 0; dir < 4; ++dir)
            {
                var d = (Direction)dir;
                if (!_myTile.AllowsMove(d)) { continue; }
                seekPlayer(_myTile, d);
            }
        }

        private void seekPlayer(Tile tile, Direction d)
        {
            var distance = 0;
            while (tile != null && tile.AllowsMove(d))
            {
                distance += 1;
                tile = _mgr.GetGrid().GetAdjacentTile(tile.GetCoords(), d);
                if (tile?.GetOccupierTileType() == TileOccupierType.HERO)
                {
                    if (_lastSeenInCurrentTurn && _lastSeenDistance < distance)
                    {
                        // keep previous player seen this turn as they were closer
                        continue;
                    }
                    _lastSeenPursueActive = true;
                    _lastSeenInCurrentTurn = true;
                    _lastSeenDirection = d;
                    _lastSeenDistance = distance;
                    _lastSeenCoord = tile.GetCoords();
                    Debug.Log($"enemy on {_myTile}  spotted player on {tile} distance {distance}.");
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