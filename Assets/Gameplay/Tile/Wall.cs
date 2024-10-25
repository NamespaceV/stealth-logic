using Assets.Gameplay.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

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
    [SerializeField] private SpriteRenderer _wallSpriteRenderer;
    [SerializeField] private SpriteRenderer _doorSprite;
    [SerializeField] private SpriteRenderer _doorOpeningSprite;
    [SerializeField] private SpriteRenderer _gateSprite;
    [SerializeField] private SpriteRenderer _rainbowGateSprite;


    

    private void Awake()
    {
        _tile = GetComponentInParent<Tile>();
        _defaultColor = _wallSpriteRenderer.color;
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
        UpdateWallSprites();
    }

    private void UpdateWallSprites()
    {
        _wallSpriteRenderer.color = IsExit ? _exitColor : _defaultColor;
        
        _doorSprite.gameObject.SetActive(_doorType == DoorType.DOOR || _doorType == DoorType.GATE_SINGLE);
        _doorSprite.color = FromColor(_doorColor);
        _gateSprite.gameObject.SetActive(_doorType == DoorType.GATE_SINGLE);
        _gateSprite.color = FromColor(_doorColor);
        _rainbowGateSprite.gameObject.SetActive(_doorType == DoorType.GATE_RAINBOW);
        
        _doorOpeningSprite.gameObject.SetActive(false);
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
        UpdateWallSprites();
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
        UpdateWallSprites();
    }
    
    public void ToggleRainbowGate()
    {
        _doorType = _doorType == DoorType.GATE_RAINBOW ? DoorType.NONE : DoorType.GATE_RAINBOW;
        UpdateWallSprites();
    }


    public void Interact(SingleGameRun gameRun, Vector2Int coord)
    {
        if(IsExit)
        {
            gameRun.Escape(coord);
        }
    }
}
