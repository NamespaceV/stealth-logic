using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem.Controls;

namespace Gameplay.Manager.SingleRun
{
    public class PortalsState
    {
        private readonly Dictionary<DoorColor, List<Vector2Int>> _portals = new();

        public void RegisterPortal(DoorColor color, Vector2Int coord)
        {
            if (!_portals.ContainsKey(color))
            {
                _portals.Add(color, new List<Vector2Int>());
            }
            _portals[color].Add(coord);
        }

        public bool IsValid(out string errorMessage)
        {
            errorMessage = "";
            foreach (var c in _portals)
            {
                if (c.Value.Count != 2)
                {
                    errorMessage = $"PORTAL COUNT INVALID \n{c.Value.Count} IS NOT 2\nfor color {c.Key}";
                    return false;
                }
            }
            return true;
        }

        public Vector2Int GetOtherPortalCoords(DoorColor color, Vector2Int coords)
        {
            if (_portals[color][0] != coords)
            {
                return _portals[color][0];
            }
            return _portals[color][1];
        }
    }
}