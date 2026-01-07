using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    struct PendingPair
    {
        public int A;
        public int B;
        public float PairedAt;

        public PendingPair(int a, int b, float pairedAt)
        {
            A = a;
            B = b;
            PairedAt = pairedAt;
        }
    }

    [field: SerializeField] public MenuView MenuView { get; private set; }
    [field: SerializeField] public BoardView BoardView { get; private set; }

    [field: SerializeField] public GameObject PanelMenu { get; private set; }
    [field: SerializeField] public GameObject PanelGame { get; private set; }
    [field: SerializeField] public Button HomeButton { get; private set; }

    [field: SerializeField] public float MatchRevealSeconds { get; private set; } = 1.2f;
    [field: SerializeField] public float MismatchRevealSeconds { get; private set; } = 1.2f;

    readonly Queue<int> pendingSingles = new();
    readonly Queue<PendingPair> pairQueue = new();

    Coroutine resolveRoutine;

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

    void StartGame(BoardPreset preset)
    {
        if (preset == null || BoardView == null)
            return;

        ResetRuntimeState();
        BoardView.Build(new GameSessionConfig(preset));

        BoardView.Build(new GameSessionConfig(preset));

        if (PanelMenu != null) PanelMenu.SetActive(false);
        if (PanelGame != null) PanelGame.SetActive(true);
    }

    void ShowMenu()
    {
        ResetRuntimeState();

        if (BoardView != null)
            BoardView.Clear();

        if (PanelMenu != null) PanelMenu.SetActive(true);
        if (PanelGame != null) PanelGame.SetActive(false);
    }

    void ResetRuntimeState()
    {
        pendingSingles.Clear();
        pairQueue.Clear();

        if (resolveRoutine != null)
        {
            StopCoroutine(resolveRoutine);
            resolveRoutine = null;
        }
    }

    void OnCardClicked(int slotIndex)
    {
        if (BoardView == null)
            return;

        var cards = BoardView.SpawnedCards;
        if (slotIndex < 0 || slotIndex >= cards.Count)
            return;

        var card = cards[slotIndex];
        if (card == null || !card.gameObject.activeSelf)
            return;

        if (card.IsMatched || card.IsFaceUp)
            return;

        card.FlipToFaceUp();

        pendingSingles.Enqueue(slotIndex);

        if (pendingSingles.Count >= 2)
        {
            var a = pendingSingles.Dequeue();
            var b = pendingSingles.Dequeue();
            pairQueue.Enqueue(new PendingPair(a, b, Time.unscaledTime));

            if (resolveRoutine == null)
                resolveRoutine = StartCoroutine(ResolvePairsRoutine());
        }
    }

  

    IEnumerator ResolvePairsRoutine()
    {
        while (pairQueue.Count > 0)
        {
            var pair = pairQueue.Dequeue();

            if (BoardView == null)
                continue;

            var cards = BoardView.SpawnedCards;
            if (pair.A < 0 || pair.B < 0 || pair.A >= cards.Count || pair.B >= cards.Count)
                continue;

            var cardA = cards[pair.A];
            var cardB = cards[pair.B];

            if (cardA == null || cardB == null)
                continue;

            if (!cardA.gameObject.activeSelf || !cardB.gameObject.activeSelf)
                continue;

            if (cardA.IsMatched || cardB.IsMatched)
                continue;

            var isMatch = cardA.CardId == cardB.CardId;
            var desiredDelay = isMatch ? MatchRevealSeconds : MismatchRevealSeconds;

            var elapsed = Time.unscaledTime - pair.PairedAt;
            var remaining = desiredDelay - elapsed;

            if (remaining > 0f)
                yield return new WaitForSecondsRealtime(remaining);
            else
                yield return null;

            if (cardA == null || cardB == null)
                continue;

            if (!cardA.gameObject.activeSelf || !cardB.gameObject.activeSelf)
                continue;

            if (cardA.IsMatched || cardB.IsMatched)
                continue;

            if (isMatch)
            {
                cardA.SetMatchedCanvasGroup();
                cardB.SetMatchedCanvasGroup();
            }
            else
            {
                cardA.FlipToFaceDown();
                cardB.FlipToFaceDown();
            }
        }

        resolveRoutine = null;
    }
}
