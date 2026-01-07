using System;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class WinView : MonoBehaviour
{
    [field: SerializeField] public Button PlayAgainButton { get; private set; }
    [field: SerializeField] public Button MainMenuButton { get; private set; }

    [field: SerializeField] public TextMeshProUGUI ScoreText { get; private set; }
    [field: SerializeField] public TextMeshProUGUI TurnsText { get; private set; }

    public event Action PlayAgainClicked;
    public event Action MainMenuClicked;


    void OnEnable()
    {
        if (PlayAgainButton != null) PlayAgainButton.onClick.AddListener(OnPlayAgain);
        if (MainMenuButton != null) MainMenuButton.onClick.AddListener(OnMainMenu);
    }

    void OnDisable()
    {
        if (PlayAgainButton != null) PlayAgainButton.onClick.RemoveListener(OnPlayAgain);
        if (MainMenuButton != null) MainMenuButton.onClick.RemoveListener(OnMainMenu);
    }

    public void Show(int score, int turns)
    {
        ScoreText.text = $"Score: {score}";
        TurnsText.text = $"Turns: {turns}";

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    void OnPlayAgain()
    {
        PlayAgainClicked?.Invoke();
    }

    void OnMainMenu()
    {
        MainMenuClicked?.Invoke();
    }
}
