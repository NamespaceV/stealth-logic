using System.Collections.Generic;
using DataFormats;
using UnityEngine;

namespace CoreLogic.States
{
    public class ButtonsState
    {
        private readonly Dictionary<DoorColor, int> _totalButtons = new();
        private readonly Dictionary<DoorColor, int> _pressedButtons = new();

        public void RegisterButton(DoorColor color, Vector2Int coord)
        {
            if (!_totalButtons.TryAdd(color, 1))
            {
                _totalButtons[color] += 1;
            }
        }

        public void ButtonPressed(DoorColor color, Vector2Int coord)
        {
            Debug.Assert(_totalButtons.ContainsKey(color));

            if (!_pressedButtons.TryAdd(color, 1))
            {
                _pressedButtons[color] += 1;
            }
            
            Debug.Log($"BUTTON pressed. {coord} - {color}\n"
                    + $"{_pressedButtons[color]} / {_totalButtons[color]}\n"
                    + $"Any {IsAnyButtonPressed(color)}, All {AreAllButtonsPressed(color)}, Rainbow {AreAllColorsPressed()}");
        }

        public void ButtonReleased(DoorColor color, Vector2Int coord)
        {
            Debug.Assert(_pressedButtons.ContainsKey(color));
            _pressedButtons[color] -= 1;

            Debug.Log($"BUTTON released. {coord} - {color}\n"
                      + $"{_pressedButtons[color]} / {_totalButtons[color]}\n"
                      + $"Any {IsAnyButtonPressed(color)}, All {AreAllButtonsPressed(color)}, Rainbow {AreAllColorsPressed()}");
        }
        
        public bool IsAnyButtonPressed(DoorColor color)
        {
            return _pressedButtons.ContainsKey(color) && _pressedButtons[color] > 0;
        }
        
        public bool AreAllButtonsPressed(DoorColor color)
        {
            return IsAnyButtonPressed(color) && _pressedButtons[color] == _totalButtons[color];
        }
        
        /// <summary>
        /// At least one button of each registered color
        /// </summary>
        public bool AreAllColorsPressed()
        {
            foreach (var c in _totalButtons.Keys)
            {
                if (!IsAnyButtonPressed(c)) return false;
            }
            return true;
        }
    }
}