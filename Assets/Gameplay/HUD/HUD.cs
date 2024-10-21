using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [SerializeField] private Button _saveButton;
    [SerializeField] private Button _loadButton;
    [SerializeField] private TMP_Text _runButtonText;
    
    [SerializeField] private TMP_Text _mainMessage;
    [SerializeField] private TMP_Text _smallMessage;

    public void Start()
    {
        SetSmallMessage("");
        SetMainMessage("");
    }

    public void StartPlay() {
        _runButtonText.text = "Quit play";
        _saveButton.gameObject.SetActive(false);
        _loadButton.gameObject.SetActive(false);
        SetSmallMessage("");
        SetMainMessage("");
    }

    public void EndPlay() {
        _runButtonText.text = "Play";
        _saveButton.gameObject.SetActive(true);
        _loadButton.gameObject.SetActive(true);
        SetSmallMessage("");
        SetMainMessage("");
    }

    public void SetSmallMessage(string text) {
        _smallMessage.text = text;
        _smallMessage.enabled = !string.IsNullOrWhiteSpace(text);
    }

    public void SetMainMessage(string text)
    {
        _mainMessage.text = text;
        _mainMessage.enabled = !string.IsNullOrWhiteSpace(text);
    }

}
