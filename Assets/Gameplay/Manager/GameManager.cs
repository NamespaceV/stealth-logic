using Assets.Common.Scripts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.U2D.Aseprite;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Assets.Gameplay.Manager
{
    public class GameManager :  MonoBehaviour
    {
        public GameObject TilePrefab;
        private Grid<Tile> _grid = new Grid<Tile>();

        private Vector2Int _selectedTileCoord;

        public void Start()
        {
            for (int x = 0; x < 10; ++x)
            {
                for (int y = 0; y < 10; ++y) {
                    var coord = new Vector2Int(x, y);
                    var go = Instantiate(TilePrefab, new Vector2(coord.x, coord.y), Quaternion.identity, transform);
                    var tile = go.GetComponent<Tile>();
                    tile.Register(this, coord);
                    _grid.SetTile(coord, tile);
                }
            }
        }

        public void TileClicked(Vector2Int coord, PointerEventData.InputButton button)
        {
            if (button == PointerEventData.InputButton.Left)
            {
                if (_selectedTileCoord != null)
                {
                    _grid.GetTile(_selectedTileCoord).SetSelected(false);
                }
                _selectedTileCoord = coord;
                _grid.GetTile(coord).SetSelected(true);

            }
            else if (button == PointerEventData.InputButton.Right)
            {
                var tile = _grid.GetTile(coord);
                var type = tile.GetTileType();
                tile.SetTileType((TileType)(((int)type + 1) % 3));
            }
        }

        public void OnMove(InputValue moveValue) {
            if (_selectedTileCoord == null){
                return;
            }
            var input = moveValue.Get<Vector2>();
            var dir = InputToDirection(input);
            if (dir == null){ return; }
            Debug.Log($"Changing wall {_selectedTileCoord} {dir}");
            _grid.GetTile(_selectedTileCoord).ToggleWall(dir.Value);
            _grid.GetAdjacentTile(_selectedTileCoord, dir.Value)?.ToggleWall(dir.Value.Opposite());
        }

        static Direction? InputToDirection(Vector2 input)
        {
            if (input.x > 0.25) { return Direction.Right; }
            if (input.x < -0.25) { return Direction.Left; }
            if (input.y > 0.25) { return Direction.Up; }
            if (input.y < -0.25) { return Direction.Down; }
            return null;
        }

        public void SaveLevel() {
            var path = EditorUtility.SaveFilePanel("SaveLevel", "C:/levels", "level", "json");
            if (string.IsNullOrEmpty(path)) return;
            var data = new LevelData();
            data.Size = _grid.GetSize();
            for (var x = 0; x < data.Size.x; ++x){
                data.Tiles.Add(new ListWrapper<TileData>());
                for (var y = 0; y < data.Size.y; ++y)
                {
                    data.Tiles[x].Add(_grid.GetTile(new Vector2Int(x, y)).ToData());
                }
            }
            File.WriteAllText(path, JsonUtility.ToJson(data));
        }
    }
}