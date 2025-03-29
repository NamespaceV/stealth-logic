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
                    tile.Migrate();
                }
            }
        }
    }

}
