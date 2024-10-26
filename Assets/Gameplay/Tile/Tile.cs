using Assets.Common.Scripts;
using Assets.Gameplay.Manager;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum TileOccupierType
{
    EMPTY,
    ENEMY,
    HERO,
}

public enum TileFloorType
{
    EMPTY,
    WATER,
}

public class Tile : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private SpriteRenderer Background;
    [SerializeField] private GameObject     PlayerVisualisation;
    [SerializeField] private GameObject     EnemyVisualisation;
    [SerializeField] private GameObject     WaterVisualisation;
    [SerializeField] private SpriteRenderer ButtonVisualisation;


    public IInteractable OnTileInteractable;
    public Dictionary<Direction, IInteractable> Interactables = new();

    [SerializeField] private List<Wall> Walls;
    private GameManager _mgr;
    private Vector2Int _coord;
    public Vector3 Pos;
    private TileOccupierType _occupierType;
    private TileFloorType _floorType;
    private DoorColor? _buttonColor;
    private int _heroCount;
    public GameObject currentObject;
    [SerializeField] private bool _selected;
    [SerializeField] private GameObject _floor3d;

    public TileFloorType FloorType { get => _floorType; set => SetFloorType(value); }

    public void Register(GameManager manager, Vector2Int tile_coord) {
        _mgr = manager;
        _coord = tile_coord;
        name = $"Tile_{_coord.x}_{_coord.y}";
        for (int i = 0;i < Walls.Count; i++) {
            Interactables[(Direction)i] = Walls[i];
        }

        Debug.Assert(Walls.Count == 4, $"{this} invalid Walls count = {Walls.Count}");

        Pos = new(_coord.x, 0, _coord.y);
    }

    public void ReadFromData(TileData tileData)
    {
        SetTileOccupierType(tileData.Type);
        SetFloorType(tileData.Floor);
        _buttonColor = tileData.HasButton ? tileData.ButtonColor : null;
        UpdateButtonSprite();
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
        Interactables[dir] = Walls[wallIdx].Exists ? Walls[wallIdx] : null;
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
        if (_floor3d != null)
        {
            var selection = _floor3d.GetComponentInChildren<MeshRenderer>(includeInactive:true)?.gameObject;
            selection?.SetActive(_selected);
        }
    }

    public void SetTileOccupierType(TileOccupierType type)
    {
        _occupierType = type;
        PlayerVisualisation.SetActive(type == TileOccupierType.HERO);
        EnemyVisualisation.SetActive(type == TileOccupierType.ENEMY);
    }


    private void SetFloorType(TileFloorType value)
    {
        _floorType = value;
        WaterVisualisation.SetActive(_floorType == TileFloorType.WATER);
    }

    public TileOccupierType GetOccupierTileType(){
        return _occupierType;
    }

    public void TryInteract(Direction direction, SingleGameRun currentRun)
    {
        if (Interactables.ContainsKey(direction))
        {
            Interactables[direction].Interact(currentRun, _coord);
        }
    }

    public Vector2Int GetCoords()
    {
        return _coord;
    }

    public bool HasWall(Direction dir) => Walls[(int)dir].Exists;

    public bool AllowsMove(Direction dir)
    {
        return !HasWall(dir);
    }

    public Wall GetWall(Direction dir)
    {
        return Walls[(int)dir];
    }

    public void RemoveCurrentObject()
    {
        Destroy(currentObject.gameObject);
        currentObject = null;
    }

    public void MoveObjectToTile(Tile targetTile)
    {
        currentObject.transform.position = targetTile.Pos;
        targetTile.currentObject = currentObject;
        currentObject = null;
    }

    internal void ToggleExit(Direction dir)
    {
        if (!HasWall(dir)) return;
        GetWall(dir).ToggleExit();
    }

    public void SetFloor3D(GameObject floor3d)
    {
        _floor3d = floor3d;
        _floor3d.GetComponentInChildren<MeshRenderer>(includeInactive: true)?.gameObject.SetActive(_selected);
    }

    public void ToggleButton(DoorColor buttonColor)
    {
        _buttonColor = _buttonColor != buttonColor ? buttonColor : null;
        UpdateButtonSprite();
    }

    private void UpdateButtonSprite()
    {
        ButtonVisualisation.gameObject.SetActive(_buttonColor.HasValue);
        if (_buttonColor.HasValue)
        {
            ButtonVisualisation.color = Wall.FromColor(_buttonColor.Value);
        }
    }
}
