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

    [SerializeField] private List<GameObject> Walls;
    private List<bool> _walls = new List<bool>(4);
    private GameManager _mgr;
    private Vector2Int _coord;
    private TileType _type;

    public void Register(GameManager manager, Vector2Int tile_coord) {
        _mgr = manager;
        _coord = tile_coord;
        name = $"Tile_{_coord.x}_{_coord.y}";
        for (int i = 0;i < Walls.Count; i++) {
            _walls.Add(true);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _mgr.TileClicked(_coord, eventData.button);
    }

    public void ToggleWall(Direction dir)
    {
        var wallIdx = (int)dir;
        _walls[wallIdx] = !_walls[wallIdx];
        Walls[wallIdx].SetActive(_walls[wallIdx]);
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
            Walls[i].SetActive(_walls[i]);
        }
    }
}
