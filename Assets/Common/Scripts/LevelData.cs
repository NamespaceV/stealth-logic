using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Common.Scripts
{
    [Serializable]
    public class LevelData
    {
        public Vector2Int Size;
        public List<List<TileData>> Tiles = new List<List<TileData>>();
    }

    [Serializable]
    public class TileData
    {
        public TileType Type;
        public List<bool> Walls;
    }
}
