﻿using System;
using Assets.Common.Scripts;
using Gameplay.Manager.SingleRun;
using NUnit.Framework;
using UnityEngine;

namespace Assets.Gameplay.Manager
{
    public class TileLogic
    {
        public static TileLogic Create(TileData tile, Vector2Int coordinates, SingleGameRun currentRun)
        {
            return new TileLogic()
            {
                _currentRun = currentRun,
                _tile = tile,
                _coordinates = coordinates,
                _occupierType = tile.Type,
            };
        }

        private TileData _tile { get; set; }
        private Vector2Int _coordinates { get; set; }
        public TileOccupierType _occupierType;
        public SingleGameRun _currentRun;

        public Vector2Int GetCoords() => _coordinates;

        public TileOccupierType GetOccupierTileType() => _occupierType;
        public TileFloorType GetFloorType() => _tile.Floor;

        public DoorColor? GetButtonColor() => _tile.HasButton ? _tile.ButtonColor : null;

        public DoorColor? GetPortalColor() => _tile.HasPortal ? _tile.PortalColor : null;

        public void MoveOccupierTo(TileLogic nextTile)
        {
            Debug.Assert(nextTile._occupierType == TileOccupierType.EMPTY);
            nextTile._occupierType = _occupierType;
            _occupierType = TileOccupierType.EMPTY;

            _currentRun.OccupierMoved(GetCoords(), nextTile.GetCoords());

            var buttons = _currentRun.GetButtonsState();
            if (_tile.HasButton)
            {
                buttons.ButtonReleased(_tile.ButtonColor, _coordinates);
            }

            if (nextTile._tile.HasButton)
            {
                buttons.ButtonPressed(nextTile._tile.ButtonColor, nextTile._coordinates);
            }
        }

        public bool HasExit(Direction dir)
        {
            return _tile.WallsData[(int)dir].DoorType == DoorType.EXIT;
        }

        public bool AllowsMove(Direction dir, ButtonsState buttonsState)
        {
            var wallData = _tile.WallsData[(int)dir];
            if (!wallData.Exists) return true;
            return DoorIsOpen(wallData.DoorType, wallData.DoorColor, buttonsState);
        }

        public static bool DoorIsOpen(DoorType type, DoorColor color, ButtonsState buttonsState)
        {
            switch (type)
            {
                case DoorType.NONE: return false;
                case DoorType.EXIT: return true;
                case DoorType.DOOR: return buttonsState.IsAnyButtonPressed(color);
                case DoorType.GATE_SINGLE: return buttonsState.AreAllButtonsPressed(color);
                case DoorType.GATE_RAINBOW: return buttonsState.AreAllColorsPressed();
            }

            throw new Exception($"Unknown door type: {type}");
        }

        public Vector2Int GetOtherPortalCoordinates(PortalsState portalsState)
        {
            return portalsState.GetOtherPortalCoords(_tile.PortalColor, GetCoords());
        }

        public void KillPlayer()
        {
            Debug.Assert(_occupierType == TileOccupierType.HERO);
            _occupierType = TileOccupierType.EMPTY;
        }

        public void FreePlayer()
        {
            Debug.Assert(_occupierType == TileOccupierType.HERO);
            _occupierType = TileOccupierType.EMPTY;
            _currentRun.OccupierMoved(GetCoords(), GetCoords());
        }
    }
}