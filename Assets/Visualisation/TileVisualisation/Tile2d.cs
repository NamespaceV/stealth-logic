using System.Collections.Generic;
using CoreLogic.Grid;
using CoreLogic.States;
using DataFormats;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using Visualisation.Map3D;

namespace Visualisation.TileVisualisation
{
    public class Tile2d : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private SpriteRenderer Background;
        [SerializeField] private GameObject     PlayerVisualisation;
        [SerializeField] private GameObject     EnemyVisualisation;
        [SerializeField] private GameObject     StoneVisualisation;
        [SerializeField] private GameObject     WaterVisualisation;
        [SerializeField] private SpriteRenderer ButtonVisualisation;
        [SerializeField] private SpriteRenderer PortalVisualisation;
    
        [SerializeField] private List<Wall2d> Walls;
        private GameManager _mgr;
        private Vector2Int _coord;
        private TileOccupierType _occupierType;
        private TileFloorType _floorType;
        private DoorColor? _buttonColor;
        private DoorColor? _portalColor;
        private int _heroCount;
    
        [SerializeField] private bool _selected;
    
        public Tile3d _tile3d = new Tile3d();
    
        public TileFloorType FloorType { get => _floorType; set => SetFloorType(value); }
        public bool IsSelected => _selected;

        public void Register(GameManager manager, Vector2Int tile_coord) {
            _mgr = manager;
            _coord = tile_coord;
            name = $"Tile_{_coord.x}_{_coord.y}";

            Debug.Assert(Walls.Count == 4, $"{this} invalid Walls count = {Walls.Count}");

            _tile3d.InitPosition(_coord);
        }

        public void ReadFromData(TileData tileData)
        {
            SetTileOccupierType(tileData.Type, null);
            SetFloorType(tileData.Floor);
            _buttonColor = tileData.HasButton ? tileData.ButtonColor : null;
            _portalColor = tileData.HasPortal ? tileData.PortalColor : null;
            if (_portalColor != null)
            {
                SetFloorType(TileFloorType.PORTAL);
            }

            UpdateButtonSprite();
            UpdatePortalSprite();
            for (int i = 0; i < 4; ++i)
            {
                Walls[i].ReadFromData(tileData.WallsData[i]);
            }
        }

        public TileData ToData()
        {
            var result = new TileData();
            result.Type = _occupierType;
            result.Floor = _floorType;
            result.HasButton = _buttonColor.HasValue;
            if (_buttonColor.HasValue)
            {
                result.ButtonColor = _buttonColor.Value;
            }
            result.HasPortal = _portalColor.HasValue;
            if (_portalColor.HasValue)
            {
                result.PortalColor = _portalColor.Value;
            }
            result.HeroCount = _heroCount;
            for (int i = 0; i < 4; ++i)
            {
                result.WallsData.Add(Walls[i].ToData());
            }
            return result;
        }


        public void OnPointerClick(PointerEventData eventData)
        {
            _mgr.TileClicked(_coord, eventData.button);
        }

        public void ToggleWall(Direction dir)
        {
            var wallIdx = (int)dir;
            Walls[wallIdx].ToggleWallExists();
        }
    
    
        public void ToggleDoor(Direction dir, DoorColor doorColor)
        {
            if (!HasWall(dir)) return;
            Walls[(int)dir].ToggleDoor(doorColor);
        }
    
        public void ToggleGate(Direction dir, DoorColor gateColor)
        {
            if (!HasWall(dir)) return;
            Walls[(int)dir].ToggleGate(gateColor);
        }
    
        public void ToggleRainbowGate(Direction dir)
        {
            if (!HasWall(dir)) return;
            Walls[(int)dir].ToggleRainbowGate();
        }

        public void SetSelected(bool val)
        {
            _selected = val;
            Background.color = val ? Color.red : Color.white;
            _tile3d.SetSelected(val);
        }

        public void SetTileOccupierType(TileOccupierType type, [CanBeNull] ButtonsState buttonsState)
        {
            _occupierType = type;
            PlayerVisualisation.SetActive(type == TileOccupierType.HERO);
            EnemyVisualisation.SetActive(type == TileOccupierType.ENEMY);
            StoneVisualisation.SetActive(type == TileOccupierType.STONE);
        }


        private void SetFloorType(TileFloorType value)
        {
            _floorType = value;
            WaterVisualisation.SetActive(_floorType == TileFloorType.WATER);
            PortalVisualisation.gameObject.SetActive(_portalColor != null);
        }

        public TileOccupierType GetOccupierTileType(){
            return _occupierType;
        }

        public Vector2Int GetCoords()
        {
            return _coord;
        }

        public bool HasWall(Direction dir) => Walls[(int)dir].BlocksMovement();
    
        public Wall2d GetWall(Direction dir)
        {
            return Walls[(int)dir];
        }

        public void MoveObjectToTile(Tile2d targetTile2d)
        {
            _tile3d.MoveObjectToTile(targetTile2d._tile3d);
        }

        internal void ToggleExit(Direction dir)
        {
            if (!HasWall(dir)) return;
            GetWall(dir).ToggleExit();
        }

        public void ToggleButton(DoorColor buttonColor)
        {
            _buttonColor = _buttonColor != buttonColor ? buttonColor : null;
            UpdateButtonSprite();
        }
    
    
        public void TogglePortal(DoorColor portalColor)
        {
            _portalColor = _portalColor != portalColor ? portalColor : null;
            UpdatePortalSprite();
        }

        private void UpdatePortalSprite()
        {
            PortalVisualisation.gameObject.SetActive(_portalColor != null);
            if (_portalColor !=  null)
            {
                PortalVisualisation.color = Wall2d.FromColor(_portalColor.Value);
            }
        }

        public DoorColor? GetButtonColor() => _buttonColor;

        public DoorColor? GetPortalColor() => _portalColor;

        private void UpdateButtonSprite()
        {
            ButtonVisualisation.gameObject.SetActive(_buttonColor.HasValue);
            if (_buttonColor.HasValue)
            {
                ButtonVisualisation.color = Wall2d.FromColor(_buttonColor.Value);
            }
        }

        public void UpdateDoors(ButtonsState buttonsState)
        {
            for (var dir = 0; dir < 4; ++dir)
            {
                Walls[(int)dir].UpdateDoor(buttonsState);
            }
        }

    }
}
