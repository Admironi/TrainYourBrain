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
    [field: SerializeField] public ContinuePopupView ContinuePopupView { get; private set; }

    [field: SerializeField] public GameObject PanelMenu { get; private set; }
    [field: SerializeField] public GameObject PanelGame { get; private set; }
    [field: SerializeField] public Button HomeButton { get; private set; }

    [field: SerializeField] public float MatchRevealSeconds { get; private set; } = 0.7f;
    [field: SerializeField] public float MismatchRevealSeconds { get; private set; } = 0.7f;

    [field: SerializeField] public int BaseMatchScore { get; private set; } = 10;
    [field: SerializeField] public int ComboBonusStep { get; private set; } = 5;

    [field: SerializeField] public BoardPreset[] Presets { get; private set; }

    readonly Queue<int> pendingSingles = new();
    readonly Queue<PendingPair> pairQueue = new();

    Coroutine resolveRoutine;

    GameSessionState session;
    GameSessionConfig currentConfig;
    BoardPreset currentPreset;

    SessionSaveData pendingSaved;
    int pendingRows;
    int pendingCols;

    void Awake()
    {
        if (BoardView == null) Debug.LogError("BoardView is not assigned.", this);
        if (HudView == null) Debug.LogError("HudView is not assigned.", this);
        if (WinView == null) Debug.LogError("WinView is not assigned.", this);
        if (ContinuePopupView == null) Debug.LogError("ContinuePopupView is not assigned.", this);
        if (PanelMenu == null) Debug.LogError("PanelMenu is not assigned.", this);
        if (PanelGame == null) Debug.LogError("PanelGame is not assigned.", this);
        if (HomeButton == null) Debug.LogError("HomeButton is not assigned.", this);
    }

    void OnEnable()
    {
        if (MenuView != null)
            MenuView.PresetSelected += StartGame;

        if (BoardView != null)
            BoardView.CardClicked += OnCardClicked;

        HomeButton.onClick.AddListener(ShowMenu);

        WinView.PlayAgainClicked += OnPlayAgain;
        WinView.MainMenuClicked += OnMainMenu;

        ContinuePopupView.ContinueClicked += OnContinueFromPopup;
        ContinuePopupView.CloseClicked += OnClosePopup;
    }

    void OnDisable()
    {
        if (MenuView != null)
            MenuView.PresetSelected -= StartGame;

        if (BoardView != null)
            BoardView.CardClicked -= OnCardClicked;

        HomeButton.onClick.RemoveListener(ShowMenu);

        WinView.PlayAgainClicked -= OnPlayAgain;
        WinView.MainMenuClicked -= OnMainMenu;

        ContinuePopupView.ContinueClicked -= OnContinueFromPopup;
        ContinuePopupView.CloseClicked -= OnClosePopup;
    }

    void Start()
    {
        ShowMenu();
    }

    void StartGame(BoardPreset preset)
    {
        if (preset == null)
            return;

        StartFreshGame(preset);
    }

    void StartFreshGame(BoardPreset preset)
    {
        ResetRuntimeState();

        WinView.Hide();
        ContinuePopupView.Hide();

        currentPreset = preset;
        currentConfig = new GameSessionConfig(preset);

        session = new GameSessionState(preset.TotalPairs, BaseMatchScore, ComboBonusStep);

        BoardView.Build(currentConfig);
        RefreshHud();

        SaveNow();

        PanelMenu.SetActive(false);
        PanelGame.SetActive(true);
    }

    void LoadSavedGame(BoardPreset preset, SessionSaveData data)
    {
        ResetRuntimeState();

        WinView.Hide();
        ContinuePopupView.Hide();

        currentPreset = preset;
        currentConfig = new GameSessionConfig(preset, data.Seed);

        session = new GameSessionState(preset.TotalPairs, BaseMatchScore, ComboBonusStep);
        session.LoadProgress(data.MatchedPairs, data.Turns, data.Score, data.ComboStreak);

        BoardView.BuildFromSavedDeck(currentConfig, data.DeckCardIds, data.MatchedSlots);
        RefreshHud();

        PanelMenu.SetActive(false);
        PanelGame.SetActive(true);
    }

    void ShowMenu()
    {
        ResetRuntimeState();

        WinView.Hide();

        BoardView.Clear();

        PanelMenu.SetActive(true);
        PanelGame.SetActive(false);

        ContinuePopupView.Hide();
        TryShowContinuePopup();
    }

    void TryShowContinuePopup()
    {
        if (!SaveService.TryLoad(out pendingRows, out pendingCols, out pendingSaved))
            return;

        if (pendingSaved == null)
            return;

        ContinuePopupView.Show();
        PanelMenu.SetActive(false);
    }

    void OnClosePopup()
    {
        ContinuePopupView.Hide();
        pendingSaved = null;
        pendingRows = 0;
        pendingCols = 0;
        PanelMenu.SetActive(true);
    }

    void OnContinueFromPopup()
    {
        ContinuePopupView.Hide();

        if (pendingSaved == null)
            return;

        var preset = FindPreset(pendingRows, pendingCols);
        if (preset == null)
        {
            SaveService.Clear();
            OnClosePopup();
            return;
        }

        LoadSavedGame(preset, pendingSaved);

        pendingSaved = null;
        pendingRows = 0;
        pendingCols = 0;
    }

    BoardPreset FindPreset(int rows, int cols)
    {
        if (Presets == null)
            return null;

        for (var i = 0; i < Presets.Length; i++)
        {
            var p = Presets[i];
            if (p == null)
                continue;

            if (p.Rows == rows && p.Columns == cols)
                return p;
        }

        return null;
    }

    void SaveNow()
    {
        if (session == null || currentConfig == null || currentPreset == null)
            return;

        var cards = BoardView.SpawnedCards;
        var matchedSlots = new bool[cards.Count];

        for (var i = 0; i < cards.Count; i++)
            matchedSlots[i] = cards[i] != null && cards[i].IsMatched;

        var deckIds = BoardView.DeckCardIds;
        var deckArray = new string[deckIds.Count];
        for (var i = 0; i < deckIds.Count; i++)
            deckArray[i] = deckIds[i];

        var data = new SessionSaveData
        {
            PresetName = currentPreset.name,
            Rows = currentPreset.Rows,
            Columns = currentPreset.Columns,
            Seed = currentConfig.Seed,
            Turns = session.Turns,
            Score = session.Score,
            ComboStreak = session.ComboStreak,
            MatchedPairs = session.MatchedPairs,
            DeckCardIds = deckArray,
            MatchedSlots = matchedSlots
        };

        SaveService.Save(currentPreset.Rows, currentPreset.Columns, data);
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
        currentConfig = null;
        currentPreset = null;

        RefreshHud();
    }

    void RefreshHud()
    {
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
        if (currentPreset == null)
            return;

        SaveService.Clear();
        StartFreshGame(currentPreset);
    }

    void OnMainMenu()
    {
        ShowMenu();
    }

    void OnCardClicked(int slotIndex)
    {
        if (session == null)
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
            HudView.SetTurns(session.Turns);
            SaveNow();

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

                SaveNow();

                if (session.IsComplete())
                {
                    SaveService.Clear();

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

                SaveNow();
            }
        }

        resolveRoutine = null;
    }
}
