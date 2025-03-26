using Assets.Common.Scripts;
using System;
using System.Collections.Generic;
using Gameplay.Manager.SingleRun;
using Settings;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Gameplay.Manager
{
    public class SingleGameRun
    {
        private GameManager _mgr;
        private Map3dManager _map3d;

        private bool _gameEnded;

        private List<Vector2Int> _playerCoords = new List<Vector2Int>();
        private int _selectedPlayerIdx = 0;
        private List<EnemyState> _enemies = new List<EnemyState>();
        private ButtonsState _buttonsState = new ButtonsState();
        private PortalsState _portalsState = new PortalsState(); 

        private HUD _hud;

        public SingleGameRun(GameManager mgr, GameObject levelParent, GameConfigSO config, HUD hud)
        {
            _mgr = mgr;
            _hud = hud;
            _map3d = new Map3dManager(
                _mgr,
                levelParent,
                config);
        }

        public void Init()
        {
            _gameEnded = false;
            _enemies.Clear();
            _playerCoords.Clear();
            _selectedPlayerIdx = 0;

            foreach (var tile in _mgr.GetGrid())
            {
                var occupier = tile.GetOccupierTileType();
                var coords = tile.GetCoords();
                if ( occupier == TileOccupierType.HERO)
                {
                    _playerCoords.Add(coords);
                }
                else if (occupier == TileOccupierType.ENEMY)
                {
                    _enemies.Add(new EnemyState(_mgr, this, coords));
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

            _map3d.Generate();
            
            Toggle3dMap(_mgr.Is3dMapOn);

            if (_playerCoords.Count == 0) {
                _gameEnded = true;
                _hud.SetMainMessage("NO PLAYER ON THE LEVEL");
                return;
            }

            if (!_portalsState.IsValid(out var errorMessage))
            {
                _gameEnded = true;
                _hud.SetMainMessage(errorMessage);
                return;
            }

            _mgr.GetGrid().GetTile(_playerCoords[_selectedPlayerIdx]).SetSelected(true);
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

            if (targetTile?.GetOccupierTileType() == TileOccupierType.STONE)
            {
                var afterStoneTile = g.GetAdjacentTile(targetTile.GetCoords(), dir.Value);
                if (afterStoneTile?.GetOccupierTileType() == TileOccupierType.EMPTY
                    && playerTile.AllowsMove(dir.Value)
                    && targetTile.AllowsMove(dir.Value))
                {
                    targetTile.SetTileOccupierType(TileOccupierType.EMPTY, _buttonsState);
                    afterStoneTile.SetTileOccupierType(TileOccupierType.STONE, _buttonsState);
                    targetTile.MoveObjectToTile(afterStoneTile);
                }
            }

            if (targetTile?.GetOccupierTileType() != TileOccupierType.EMPTY
                || !playerTile.AllowsMove(dir.Value))
            {
                if (playerTile.TryInteract(dir.Value, this))
                {
                    moveEnemies();
                    updateDoors();
                }
                return;
            }
            
            if (targetTile.OnTileInteractable != null)
            {
                targetTile.OnTileInteractable.TryInteract(this, playerCoords);
            }

            playerTile.SetTileOccupierType(TileOccupierType.EMPTY, _buttonsState);
            playerTile.MoveObjectToTile(targetTile);
            playerTile.SetSelected(false);
            if (targetTile.FloorType == TileFloorType.PORTAL)
            {
                var otherPortalCoords = _portalsState.GetOtherPortalCoords(targetTile.GetPortalColor().Value, targetTile.GetCoords());
                var otherPortalTile = g.GetTile(otherPortalCoords);
                if (otherPortalTile?.GetOccupierTileType() == TileOccupierType.EMPTY)
                {
                    otherPortalTile.SetSelected(true);
                    otherPortalTile.SetTileOccupierType(TileOccupierType.HERO, _buttonsState);
                    _playerCoords[_selectedPlayerIdx] = otherPortalTile.GetCoords();
                    moveEnemies();
                    updateDoors();
                    return;
                }
            }
            
            targetTile.SetTileOccupierType(TileOccupierType.HERO, _buttonsState);
            targetTile.SetSelected(true);
            _playerCoords[_selectedPlayerIdx] = targetTile.GetCoords();
            moveEnemies();
            updateDoors();
        }

        private void updateDoors()
        {
            foreach (var tile in _mgr.GetGrid())
            {
                tile.UpdateDoors(_buttonsState);
            }
        }

        private void moveEnemies()
        {
            foreach (var e in _enemies) {
                e.Move(_buttonsState);
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
                t.SetTileOccupierType(TileOccupierType.EMPTY, _buttonsState);
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
}