using Assets.Common.Scripts;
using System;
using System.Collections.Generic;
using Gameplay.Manager.SingleRun;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Gameplay.Manager
{
    public class SingleGameRun : MonoBehaviour
    {
        private GameManager _mgr;
        private Map3dManager _map3d;

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
            _map3d = new Map3dManager(
                _mgr,
                LevelParent,
                FloorTilePrefab,
                FloorWaterTilePrefab,
                WallTilePrefab,
                PlayerPrefab,
                EnemyPrefab,
                ExitSprite);
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

            _map3d.Generate();
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
            _map3d.Clear();
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
}