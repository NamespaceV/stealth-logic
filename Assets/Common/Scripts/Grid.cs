using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Common.Scripts
{
    public enum Direction
    {
        Right,
        Down,
        Left,
        Up,
    }

    public static class DirectionHelpers{
        public static Vector2Int ToOffset(this Direction direction)
        {
            switch (direction) {
                case Direction.Left: return Vector2Int.left;
                case Direction.Right: return Vector2Int.right;
                case Direction.Up: return Vector2Int.up;
                case Direction.Down: return Vector2Int.down;
            }
            throw new NotImplementedException("Missing offset for "+direction);
        }

        public static Direction Opposite(this Direction direction){
            return (Direction)(((int)direction + 2) % 4);
        }
    }

    public class Grid<T> : IEnumerable<T>
    {
        private List<List<T>> _tiles = new List<List<T>>();

        public void SetTile(Vector2Int coord, T tile){
            while (_tiles.Count <= coord.x){
                _tiles.Add(new List<T>());
            }
            while (_tiles[coord.x].Count <= coord.y)
            {
                _tiles[coord.x].Add(default);
            }
            _tiles[coord.x][coord.y] = tile;
        }

        public T GetTile(Vector2Int coord){
            if (coord.x < 0 || coord.y < 0
                || coord.x >= _tiles.Count
                || coord.y >= _tiles[coord.x].Count)
            {
                return default;
            }
            return _tiles[coord.x][coord.y];
        }

        public T GetAdjacentTile(Vector2Int coord, Direction dir)
        {
            return GetTile(coord + dir.ToOffset());
        }

        public Vector2Int GetSize()
        {
            var x = _tiles.Count;
            if (x == 0) return new Vector2Int(0, 0);
            return new Vector2Int(x, _tiles[0].Count);
        }

        public void Clear()
        {
            _tiles.Clear();
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (var x = 0; x < _tiles.Count; ++x){
                for (var y = 0; y < _tiles[x].Count; ++y){ 
                    yield return _tiles[x][y];
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
