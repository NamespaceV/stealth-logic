using CoreLogic.Grid;
using DataFormats;
using UnityEngine;

namespace CoreLogic.States
{
    public class EnemyState
    {
        private SingleGameRun _currentRun;
        private Vector2Int _coord;

        private bool _lastSeenInCurrentTurn;
        private bool _lastSeenPursueActive;
        private Vector2Int _lastSeenCoord;
        private Direction _lastSeenDirection;
        private int _lastSeenDistance;

        public EnemyState( SingleGameRun currentRun, Vector2Int coord)
        {
            _currentRun = currentRun;
            _coord = coord;
        }
        
        public void Move()
        {
            _lastSeenInCurrentTurn = false;

            SeekForPlayers();
            
            if (_lastSeenPursueActive)
            {
                
                if (!_currentRun.GetGrid().GetTile(_coord)
                        .AllowsMove(_lastSeenDirection, _currentRun.GetButtonsState()))
                {
                    return;
                }
                var adjacent = _currentRun.GetGrid().GetAdjacentTile(_coord, _lastSeenDirection);
                if (adjacent.GetFloorType() == TileFloorType.WATER) { return; }
                if (adjacent.GetOccupierTileType() == TileOccupierType.HERO)
                {
                    _currentRun.PlayerLost();
                    adjacent.KillPlayer();
                    moveTo(adjacent);
                    return;
                }

                if (adjacent.GetOccupierTileType() != TileOccupierType.EMPTY)
                {
                    return;
                }

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
            var myTile = _currentRun.GetGrid().GetTile(_coord);

            for (int dir = 0; dir < 4; ++dir)
            {
                var d = (Direction)dir;
                if (!myTile.AllowsMove(d, _currentRun.GetButtonsState())) { continue; }

                seekPlayer(myTile, d);
            }
        }

        private void seekPlayer(TileLogic tile, Direction d)
        {
            var distance = 0;
            while (tile != null && tile.AllowsMove(d, _currentRun.GetButtonsState()))
            {
                distance += 1;
                tile = _currentRun.GetGrid().GetAdjacentTile(tile.GetCoords(), d);
                if (tile.GetOccupierTileType() == TileOccupierType.STONE)
                {
                    break;
                }

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
                    Debug.Log($"enemy on {_coord}  spotted player on {tile.GetCoords()} distance {distance}.");
                }
            }
        }

        private void moveTo(TileLogic adjacent)
        {
            var myTile = _currentRun.GetGrid().GetTile(_coord);
            myTile.MoveOccupierTo(adjacent);
            _coord = adjacent.GetCoords();
        }
    }
}