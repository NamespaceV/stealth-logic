using Assets.Gameplay.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DoorType
{
    NONE = 0,
    EXIT,
    DOOR,
    GATE_SINGLE,
    GATE_RAINBOW,
}

public enum DoorColor
{
    COLOR_1,
    COLOR_2,
    COLOR_3,
    COLOR_4,
}


public class Wall : MonoBehaviour, IInteractable
{
    private static readonly Color[] DoorColors =
    {
        Color.red,
        Color.green,
        new Color(0, 0.7f, 0.7f),
        new Color(0.7f, 0.0f, 0.7f),
    };
    public static Color FromColor(DoorColor doorColor) => DoorColors[(int)doorColor];
    public static DoorColor Next(DoorColor a) => (DoorColor)(((int)a + 1) % DoorColors.Length);
    public static DoorColor Prev(DoorColor a) => (DoorColor)(((int)a - 1 + DoorColors.Length) % DoorColors.Length);
    public bool IsExit => _doorType == DoorType.EXIT;
    private GameManager _gameManager;

    private DoorType _doorType;
    private DoorColor _doorColor;

    private Color _defaultColor;
    private Color _exitColor = Color.cyan;

    private Tile _tile;
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _tile = GetComponentInParent<Tile>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _defaultColor = _spriteRenderer.color;
    }

    private void Start()
    {
        _gameManager = GameManager.Instance;
    }

    public void SetExit(bool exit){
        if (exit != IsExit){
            ToggleExit();
        }
    }

    public void ToggleExit()
    {
        _doorType = IsExit ? DoorType.NONE : DoorType.EXIT;
        _spriteRenderer.color = GetWallColor();
    }

    private Color GetWallColor()
    {
        if (IsExit) return _exitColor;
        if (_doorType == DoorType.DOOR) return FromColor(_doorColor);
        if (_doorType == DoorType.GATE_SINGLE) return FromColor(_doorColor);
        return _defaultColor;
    }

    public void ToggleDoor(DoorColor doorColor)
    {
        if (_doorType == DoorType.DOOR && _doorColor == doorColor)
        {
            _doorType = DoorType.NONE;
        }
        else
        {
            _doorType = DoorType.DOOR;
            _doorColor = doorColor;
        }
        _spriteRenderer.color = GetWallColor();
    }
    
    public void ToggleGate(DoorColor gateColor)
    {
        if (_doorType == DoorType.GATE_SINGLE && _doorColor == gateColor)
        {
            _doorType = DoorType.NONE;
        }
        else
        {
            _doorType = DoorType.GATE_SINGLE;
            _doorColor = gateColor;
        }
        _spriteRenderer.color = GetWallColor();
    }
    
    public void ToggleRainbowGate()
    {
        _doorType = _doorType == DoorType.GATE_RAINBOW ? DoorType.NONE : DoorType.GATE_RAINBOW;
        _spriteRenderer.color = GetWallColor();
    }


    public void Interact(SingleGameRun gameRun, Vector2Int coord)
    {
        if(IsExit)
        {
            gameRun.Escape(coord);
        }
    }
}
