using System.Collections.Generic;
using CoreLogic.Grid;
using CoreLogic.States;
using DataFormats;
using UnityEngine;

namespace CoreLogic
{
    public enum GameState
    {
        PLAYING,
        ERROR,
        WON,
        LOST,
    }

    public class SingleGameRun
    {
        public GameState GameState { get; private set; }

        private List<Vector2Int> _playerCoords = new();
        private List<EnemyState> _enemies = new();
        private ButtonsState _buttonsState = new();
        private PortalsState _portalsState = new();
        
        private Grid<TileLogic> _grid = new ();
        
        public List<string> Errors = new List<string>();

        public static SingleGameRun Create(LevelData levelData)
        {
            return new SingleGameRun(levelData);
        }

        public SingleGameRun(LevelData levelData)
        {
            for (int x = 0; x < levelData.Size.x; ++x)
            {
                for (int y = 0; y < levelData.Size.y; ++y)
                {
                    var tile = levelData.Tiles[x][y];
                    var coordinates = new Vector2Int(x, y);
                    _grid.SetTile(coordinates, TileLogic.Create(tile, coordinates, this));
                }
            }
      
            GameState = GameState.PLAYING;
            _enemies.Clear();
            _playerCoords.Clear();

            foreach (var tile in _grid)
            {
                var coords = tile.GetCoords();
                var occupier = tile.GetOccupierTileType();
                if ( occupier == TileOccupierType.HERO)
                {
                    _playerCoords.Add(coords);
                }
                else if (occupier == TileOccupierType.ENEMY)
                {
                    _enemies.Add(new EnemyState(this, coords));
                }

                var buttonColor = tile.GetButtonColor();
                if (buttonColor.HasValue)
                {
                    _buttonsState.RegisterButton(buttonColor.Value, coords);
                    if (occupier != TileOccupierType.EMPTY)
                    {
                        _buttonsState.ButtonPressed(buttonColor.Value, coords);
                    }
                }
                var portalColor = tile.GetPortalColor();
                if (portalColor.HasValue)
                {
                    _portalsState.RegisterPortal(portalColor.Value, coords);
                }
            }
            
            if (_playerCoords.Count == 0) {
                GameState = GameState.ERROR;
                Errors.Add("NO PLAYER ON THE LEVEL");
                return;
            }

            if (!_portalsState.IsValid(out var errorMessage))
            {
                GameState = GameState.ERROR;
                Errors.Add(errorMessage);
                return;
            }
        }
        
        public int GetPlayerCount() => _playerCoords.Count;
        
        public Vector2Int GetPlayerCoords(int idx) => _playerCoords[idx];

        public void MakeMove(int playerIdx, Direction? dir)
        {
            if (GameState != GameState.PLAYING) return;
            if (dir == null) return;

            var playerCoords = GetPlayerCoords(playerIdx);
            var playerTile = _grid.GetTile(playerCoords);
            var targetTile = _grid.GetAdjacentTile(playerCoords, dir.Value);
            
            if (playerTile.HasExit(dir.Value))
            {
                playerTile.FreePlayer();
                _playerCoords.RemoveAt(playerIdx);
                if (_playerCoords.Count == 0)
                {
                    GameState = GameState.WON;
                    return;
                }
                moveEnemies();
                return;
            }
            
            if (targetTile?.GetOccupierTileType() == TileOccupierType.STONE)
            {
                var afterStoneTile = _grid.GetAdjacentTile(targetTile.GetCoords(), dir.Value);
                if (afterStoneTile?.GetOccupierTileType() == TileOccupierType.EMPTY
                    && playerTile.AllowsMove(dir.Value, _buttonsState)
                    && targetTile.AllowsMove(dir.Value, _buttonsState))
                {
                    targetTile.MoveOccupierTo(afterStoneTile);
                }
            }
            
            if (targetTile?.GetFloorType() == TileFloorType.PORTAL)
            {
                var otherPortalCoords = targetTile.GetOtherPortalCoordinates(_portalsState);
                var otherPortalTile = _grid.GetTile(otherPortalCoords);
                if (otherPortalTile?.GetOccupierTileType() == TileOccupierType.EMPTY)
                {
                    _playerCoords[playerIdx] = otherPortalCoords;
                    playerTile.MoveOccupierTo(otherPortalTile);
                    moveEnemies();
                    return;
                }
            }

            if (targetTile?.GetOccupierTileType() == TileOccupierType.EMPTY
                && playerTile.AllowsMove(dir.Value, _buttonsState))
            {
                playerTile.MoveOccupierTo(targetTile);
                _playerCoords[playerIdx] = targetTile.GetCoords();
                moveEnemies();
            }
        }

        private void moveEnemies()
        {
            foreach (var e in _enemies) {
                e.Move();
            }
        }
        
        public void PlayerLost()
        {                    
            GameState = GameState.LOST;
            //_hud.SetMainMessage("YOU LOST");
        }


        public ButtonsState GetButtonsState()
        {
            return _buttonsState;
        }

        // TODO: Fix encapsulation for Enemy state
        public Grid<TileLogic> GetGrid() => _grid;

        public delegate void OccupierMovedEvent(Vector2Int from, Vector2Int to);
        public event OccupierMovedEvent OnOccupierMoved;

        public void OccupierMoved(Vector2Int from, Vector2Int to)
        {
            OnOccupierMoved?.Invoke(from, to);
        }
    }
}