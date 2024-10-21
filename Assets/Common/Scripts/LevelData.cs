using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Common.Scripts
{
    [Serializable]
    public class LevelData
    {
        public Vector2Int Size;
        public List<ListWrapper<TileData>> Tiles = new List<ListWrapper<TileData>>();

        public void Migrate()
        {
            for (int x = 0; x < Size.x; ++x)
            {
                for (int y = 0; y < Size.y; ++y)
                {
                    var tile = Tiles[x][y];
                    if (tile.Exits == null || tile.Exits.Count != 4) {
                        tile.Exits = new List<bool>{ false, false, false, false };
                    }
                }
            }
        }
    }

    [Serializable]
    public class TileData
    {
        public TileType Type;
        public int HeroCount;
        public List<bool> Walls;
        public List<bool> Exits;
    }

    // Unity cant serialize List<List<>> :/
    // https://discussions.unity.com/t/serialize-nested-lists/47472/2
    [Serializable]
    public class ListWrapper<T>
    {
        [SerializeField] private List<T> _innerList = new List<T>();
 
        public T this[int key]
        {
            get
            {
                return _innerList[key];
            }
            set
            {
                _innerList[key] = value;
            }
        }

        public void Add(T element)
        {
            _innerList.Add(element);
        }
    }
}
