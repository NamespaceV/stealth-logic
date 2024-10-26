using System;
using System.Collections.Generic;
using Unity.VisualScripting;
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
                    tile.Migrate();
                }
            }
        }
    }

    [Serializable]
    public class TileData
    {
        public TileOccupierType Type;
        public TileFloorType Floor;
        public int HeroCount;
        public List<bool> Walls; // TODO: LEGACY, DELETE
        public List<bool> Exits; // TODO: LEGACY, DELETE
        public List<WallData> WallsData = new List<WallData>(4);

        public void Migrate()
        {
            if (Exits == null || Exits.Count != 4) {
                Exits = new List<bool>{ false, false, false, false };
            }
            if (WallsData == null || WallsData.Count != 4)
            {
                WallsData = new List<WallData>();
                for (int dir = 0; dir < 4; ++dir)
                {
                    WallsData.Add(WallData.NoWall());
                    if (Walls[dir])
                    {
                        WallsData[dir].Exists = true;
                    }
                    if (Exits[dir])
                    {
                        WallsData[dir].DoorType = DoorType.EXIT;
                    }
                }
            }
        }
    }
    
    [Serializable]
    public class WallData
    {
        public bool Exists;
        public DoorType DoorType;
        public DoorColor DoorColor;
        public static WallData NoWall() { return new WallData { Exists = false }; }
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
