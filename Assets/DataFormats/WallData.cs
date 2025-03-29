using System;

namespace DataFormats
{
    [Serializable]
    public class WallData
    {
        public bool Exists;
        public DoorType DoorType;
        public DoorColor DoorColor;
        public static WallData NoWall() { return new WallData { Exists = false }; }
    }
}