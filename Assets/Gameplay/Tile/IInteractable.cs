using Assets.Gameplay.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    public void Interact(SingleGameRun gameRun, Vector2Int coord);
}
