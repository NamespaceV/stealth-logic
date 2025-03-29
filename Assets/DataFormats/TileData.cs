using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace Assets.Common.Scripts
{
    [Serializable]
    public class TileData
    {
        public TileOccupierType Type;
        public TileFloorType Floor;
        public int HeroCount;
        public List<bool> Walls; // TODO: LEGACY, DELETE
        public List<bool> Exits; // TODO: LEGACY, DELETE
        public List<WallData> WallsData = new List<WallData>(4);
        public bool HasButton; //Unity doesn't serialize DoorColor? so split into 2 fields
        public DoorColor ButtonColor;
        [FormerlySerializedAs("HasTeleport")] public bool HasPortal; //Unity doesn't serialize DoorColor? so split into 2 fields
        [FormerlySerializedAs("TeleportColor")] public DoorColor PortalColor;

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
}