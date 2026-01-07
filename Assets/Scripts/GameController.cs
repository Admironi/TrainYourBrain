using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [field: SerializeField] public MenuView MenuView { get; private set; }
    [field: SerializeField] public BoardView BoardView { get; private set; }

    [field: SerializeField] public GameObject PanelMenu { get; private set; }
    [field: SerializeField] public GameObject PanelGame { get; private set; }
    [field: SerializeField] public Button HomeButton { get; private set; }

    [field: SerializeField] public float MatchRevealSeconds { get; private set; } = 1.2f;
    [field: SerializeField] public float MismatchRevealSeconds { get; private set; } = 1.2f;

    readonly Queue<int> pendingSelections = new();
    readonly List<int> faceUpUnresolved = new(2);

    bool isResolving;

    void OnEnable()
    {
        if (MenuView != null)
            MenuView.PresetSelected += StartGame;

        if (BoardView != null)
            BoardView.CardClicked += OnCardClicked;

        if (HomeButton != null)
            HomeButton.onClick.AddListener(ShowMenu);
    }

    void OnDisable()
    {
        if (MenuView != null)
            MenuView.PresetSelected -= StartGame;

        if (BoardView != null)
            BoardView.CardClicked -= OnCardClicked;

        if (HomeButton != null)
            HomeButton.onClick.RemoveListener(ShowMenu);
    }

    void Start()
    {
        ShowMenu();
    }

    void Update()
    {
        if (isResolving)
            return;

        while (pendingSelections.Count > 0)
        {
            var slot = pendingSelections.Dequeue();
            if (TrySelect(slot))
                break;
        }
    }

    void StartGame(BoardPreset preset)
    {
        if (preset == null || BoardView == null)
            return;

        pendingSelections.Clear();
        faceUpUnresolved.Clear();
        isResolving = false;

        BoardView.Build(new GameSessionConfig(preset));

        if (PanelMenu != null) PanelMenu.SetActive(false);
        if (PanelGame != null) PanelGame.SetActive(true);
    }

    void ShowMenu()
    {
        pendingSelections.Clear();
        faceUpUnresolved.Clear();
        isResolving = false;

        if (BoardView != null)
            BoardView.Clear();

        if (PanelMenu != null) PanelMenu.SetActive(true);
        if (PanelGame != null) PanelGame.SetActive(false);
    }

    void OnCardClicked(int slotIndex)
    {
        pendingSelections.Enqueue(slotIndex);
    }

    bool TrySelect(int slotIndex)
    {
        if (BoardView == null)
            return false;

        var cards = BoardView.SpawnedCards;
        if (slotIndex < 0 || slotIndex >= cards.Count)
            return false;

        var card = cards[slotIndex];
        if (card == null || !card.gameObject.activeSelf)
            return false;

        if (faceUpUnresolved.Contains(slotIndex))
            return false;

        card.FlipToFaceUp();
        faceUpUnresolved.Add(slotIndex);

        if (faceUpUnresolved.Count < 2)
            return false;

        var a = faceUpUnresolved[0];
        var b = faceUpUnresolved[1];

        faceUpUnresolved.Clear();
        StartCoroutine(ResolvePairRoutine(a, b));
        return true;
    }

    IEnumerator ResolvePairRoutine(int a, int b)
    {
        isResolving = true;

        var cards = BoardView.SpawnedCards;
        if (a < 0 || b < 0 || a >= cards.Count || b >= cards.Count)
        {
            isResolving = false;
            yield break;
        }

        var cardA = cards[a];
        var cardB = cards[b];

        if (cardA == null || cardB == null || !cardA.gameObject.activeSelf || !cardB.gameObject.activeSelf)
        {
            isResolving = false;
            yield break;
        }

        var isMatch = cardA.CardId == cardB.CardId;

        if (isMatch)
        {
            if (MatchRevealSeconds > 0f)
                yield return new WaitForSeconds(MatchRevealSeconds);
            else
                yield return null;

            if (cardA != null && cardA.gameObject.activeSelf)
                cardA.SetMatchedCanvasGroup();

            if (cardB != null && cardB.gameObject.activeSelf)
                cardB.SetMatchedCanvasGroup();

            isResolving = false;
            yield break;
        }

        if (MismatchRevealSeconds > 0f)
            yield return new WaitForSeconds(MismatchRevealSeconds);
        else
            yield return null;

        if (cardA != null && cardA.gameObject.activeSelf)
            cardA.FlipToFaceDown();

        if (cardB != null && cardB.gameObject.activeSelf)
            cardB.FlipToFaceDown();

        isResolving = false;
    }
}
