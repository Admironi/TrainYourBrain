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
    [field: SerializeField] public HudView HudView { get; private set; }
    [field: SerializeField] public WinView WinView { get; private set; }

    [field: SerializeField] public GameObject PanelMenu { get; private set; }
    [field: SerializeField] public GameObject PanelGame { get; private set; }
    [field: SerializeField] public Button HomeButton { get; private set; }

    [field: SerializeField] public float MatchRevealSeconds { get; private set; } = 1.2f;
    [field: SerializeField] public float MismatchRevealSeconds { get; private set; } = 1.2f;

    [field: SerializeField] public int BaseMatchScore { get; private set; } = 10;
    [field: SerializeField] public int ComboBonusStep { get; private set; } = 5;

    readonly Queue<int> pendingSingles = new();
    readonly Queue<PendingPair> pairQueue = new();

    Coroutine resolveRoutine;
    BoardPreset lastPreset;
    GameSessionState session;

    void OnEnable()
    {
        if (MenuView != null)
            MenuView.PresetSelected += StartGame;

        if (BoardView != null)
            BoardView.CardClicked += OnCardClicked;

        if (HomeButton != null)
            HomeButton.onClick.AddListener(ShowMenu);

        if (WinView != null)
        {
            WinView.PlayAgainClicked += OnPlayAgain;
            WinView.MainMenuClicked += OnMainMenu;
        }
    }

    void OnDisable()
    {
        if (MenuView != null)
            MenuView.PresetSelected -= StartGame;

        if (BoardView != null)
            BoardView.CardClicked -= OnCardClicked;

        if (HomeButton != null)
            HomeButton.onClick.RemoveListener(ShowMenu);

        if (WinView != null)
        {
            WinView.PlayAgainClicked -= OnPlayAgain;
            WinView.MainMenuClicked -= OnMainMenu;
        }
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

        WinView.Hide();
        lastPreset = preset;

        session = new GameSessionState(preset.TotalPairs, BaseMatchScore, ComboBonusStep);

        BoardView.Build(new GameSessionConfig(preset));
        RefreshHud();

        if (PanelMenu != null) PanelMenu.SetActive(false);
        if (PanelGame != null) PanelGame.SetActive(true);
    }

    void ShowMenu()
    {
        ResetRuntimeState();

        WinView.Hide();

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

        session = null;
        RefreshHud();
    }

    void RefreshHud()
    {
        if (HudView == null)
            return;

        var totalPairs = session != null ? session.TotalPairs : 0;
        var matchedPairs = session != null ? session.MatchedPairs : 0;
        var turns = session != null ? session.Turns : 0;
        var score = session != null ? session.Score : 0;

        HudView.SetMatches(matchedPairs, totalPairs);
        HudView.SetTurns(turns);
        HudView.SetScore(score);
    }

    void OnPlayAgain()
    {
        if (lastPreset == null)
            return;

        StartGame(lastPreset);
    }

    void OnMainMenu()
    {
        ShowMenu();
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

            session.RegisterTurn();
            HudView?.SetTurns(session.Turns);

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
                session.RegisterMatch();

                cardA.SetMatchedCanvasGroup();
                cardB.SetMatchedCanvasGroup();

                HudView.SetMatches(session.MatchedPairs, session.TotalPairs);
                HudView.SetScore(session.Score);

                if (session.IsComplete())
                {
                    pairQueue.Clear();
                    pendingSingles.Clear();
                    WinView.Show(session.Score, session.Turns);
                }
            }
            else
            {
                session.RegisterMismatch();

                cardA.FlipToFaceDown();
                cardB.FlipToFaceDown();
            }
        }

        resolveRoutine = null;
    }
}
