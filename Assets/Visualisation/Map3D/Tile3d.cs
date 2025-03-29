using UnityEngine;

namespace Gameplay.Manager.SingleRun
{
    public class Tile3d
    {
        public GameObject _floor3d;
        public GameObject _selection3d;
        public GameObject _occupier3d;
        
        public Vector3 Pos;

        public void InitPosition(Vector2Int coord)
        {
            Pos = new(coord.x, 0, coord.y);
        }
        
        public void SetSelected(bool val)
        {
            if (_selection3d != null)
            {
                _selection3d?.SetActive(val);
            }
        }

        public void MoveObjectToTile(Tile3d targetTile3d)
        {
            Debug.Assert(_occupier3d != null);
            
            _occupier3d.transform.position = targetTile3d.Pos;
            targetTile3d._occupier3d = _occupier3d;
            _occupier3d = null;
        }
        
        public void SetFloor(GameObject floor3d)
        {
            _floor3d = floor3d;
            _selection3d = _floor3d.GetComponentInChildren<MeshRenderer>(includeInactive: true)?.gameObject;
        }

    }
}