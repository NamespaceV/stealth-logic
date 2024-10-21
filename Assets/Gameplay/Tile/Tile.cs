using Assets.Common.Scripts;
using Assets.Gameplay.Manager;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum TileType
{
    EMPTY,
    ENEMY,
    HERO,
}

public class Tile : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private SpriteRenderer Background;
    [SerializeField] private GameObject PlayerVisualisation;
    [SerializeField] private GameObject EnemyVisualisation;

    public IInteractable OnTileInteractable;
    public Dictionary<Direction, IInteractable> Interactables = new();

    [SerializeField] private List<Wall> Walls;
    private List<bool> _walls = new List<bool>(4);
    private GameManager _mgr;
    private Vector2Int _coord;
    public Vector3 Pos;
    private TileType _type;
    public GameObject currentObject;

    public void Register(GameManager manager, Vector2Int tile_coord) {
        _mgr = manager;
        _coord = tile_coord;
        name = $"Tile_{_coord.x}_{_coord.y}";
        for (int i = 0;i < Walls.Count; i++) {
            _walls.Add(true);
        }

        Pos = new(_coord.x, 0, _coord.y);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _mgr.TileClicked(_coord, eventData.button);
    }

    public void ToggleWall(Direction dir)
    {
        var wallIdx = (int)dir;
        _walls[wallIdx] = !_walls[wallIdx];
        Walls[wallIdx].gameObject.SetActive(_walls[wallIdx]);

        if (_walls[wallIdx])
        {
            Interactables[dir] = Walls[wallIdx];
        }
        else
        {
            Interactables[dir] = null;
        }
    }

    public void SetSelected(bool val)
    {
        Background.color = val ? Color.red : Color.white;
    }

    public void SetTileType(TileType type)
    {
        _type = type;
        PlayerVisualisation.SetActive(type == TileType.HERO);
        EnemyVisualisation.SetActive(type == TileType.ENEMY);
    }

    public TileType GetTileType(){
        return _type;
    }

    public TileData ToData()
    {
        var result = new TileData();
        result.Type = _type;
        result.Walls = new List<bool>(_walls);
        return result;
    }

    public void ReadFromData(TileData tileData)
    {
        SetTileType(tileData.Type);
        _walls = new List<bool>(tileData.Walls);
        for (int i = 0; i < 4; ++i){
            Walls[i].gameObject.SetActive(_walls[i]);
        }
    }

    public void TryInteract(Direction direction)
    {
        if (Interactables.ContainsKey(direction)) Interactables[direction].Interact();
    }

    public Vector2Int GetCoords()
    {
        return _coord;
    }

    public bool CheckWall(Direction dir) => _walls[(int)dir];

    public bool AllowsMove(Direction dir)
    {
        return !CheckWall(dir);
    }

    public Wall GetWall(Direction dir)
    {
        return Walls[(int)dir];
    }

    public void MoveObjectToTile(Tile targetTile)
    {
        currentObject.transform.position = targetTile.Pos;
        targetTile.currentObject = currentObject;
        currentObject = null;
    }
}
