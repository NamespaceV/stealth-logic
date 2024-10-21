using Assets.Gameplay.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour, IInteractable
{
    public bool isExit { get; private set; }
    private GameManager _gameManager;

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
        if (exit != isExit){
            ToggleExit();
        }
    }

    public void ToggleExit()
    {
        isExit = !isExit;

        if (isExit) _spriteRenderer.color = _exitColor;
        else _spriteRenderer.color = _defaultColor;
    }

    public void Interact()
    {
        if(isExit)
        {
            Debug.Log("You escaped.");
        }
    }
}
