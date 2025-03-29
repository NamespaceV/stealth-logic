using CoreLogic;
using CoreLogic.States;
using DataFormats;
using UnityEngine;

namespace Visualisation.TileVisualisation
{
    public class Wall2d : MonoBehaviour
    {
        private static readonly Color[] DoorColors =
        {
            Color.red,
            Color.green,
            new Color(0, 0.7f, 0.7f),
            new Color(0.7f, 0.7f, 0.0f),
        };
        public static Color FromColor(DoorColor doorColor) => DoorColors[(int)doorColor];
        public static DoorColor Next(DoorColor a) => (DoorColor)(((int)a + 1) % DoorColors.Length);
        public static DoorColor Prev(DoorColor a) => (DoorColor)(((int)a - 1 + DoorColors.Length) % DoorColors.Length);
        public bool IsExit => _doorType == DoorType.EXIT;
        public bool Exists => gameObject.activeSelf;

        private GameManager _gameManager;

        private DoorType _doorType;
        private DoorColor _doorColor;
        private bool _isOpen;

        private Color _defaultColor;
        private Color _exitColor = Color.cyan;

        private Tile2d _tile2d;
        [SerializeField] private SpriteRenderer _wallSpriteRenderer;
        [SerializeField] private SpriteRenderer _doorSprite;
        [SerializeField] private SpriteRenderer _doorOpeningSprite;
        [SerializeField] private SpriteRenderer _gateSprite;
        [SerializeField] private SpriteRenderer _rainbowGateSprite;

        private void Awake()
        {
            _tile2d = GetComponentInParent<Tile2d>();
            _defaultColor = _wallSpriteRenderer.color;
        }

        private void Start()
        {
            _gameManager = GameManager.Instance;
        }
    
        public void UpdateDoor(ButtonsState buttonsState)
        {
            switch (_doorType)
            {
                case DoorType.DOOR: 
                case DoorType.GATE_SINGLE:
                case DoorType.GATE_RAINBOW:
                    _isOpen = TileLogic.DoorIsOpen(_doorType, _doorColor, buttonsState); 
                    break;
            }

            if (_doorType != DoorType.NONE && _doorType != DoorType.EXIT)
            {
                _doorOpeningSprite.gameObject.SetActive(_isOpen);
            }
        }
    
        public bool BlocksMovement()
        {
            return Exists && !_isOpen;
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
    
    
        public void ToggleWallExists()
        {
            gameObject.SetActive(!Exists);
            UpdateWallSprites();
        }
    
        public void ToggleExit()
        {
            _doorType = IsExit ? DoorType.NONE : DoorType.EXIT;
            UpdateWallSprites();
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
    
        public void ReadFromData(WallData wallData)
        {
            if (!wallData.Exists)
            {
                gameObject.SetActive(false);
                return;
            }

            _doorType = wallData.DoorType;
            _doorColor = wallData.DoorColor;
            UpdateWallSprites();
        }
    
        public WallData ToData()
        {
            var result = new WallData();
            result.Exists = gameObject.activeSelf;
            result.DoorType = _doorType;
            result.DoorColor = _doorColor;
            return result;
        }

    }
}
