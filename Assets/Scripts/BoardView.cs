using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardView : MonoBehaviour
{
    [field: SerializeField] public RectTransform BoardArea { get; private set; }
    [field: SerializeField] public GridLayoutGroup Grid { get; private set; }
    [field: SerializeField] public CardView CardPrefab { get; private set; }
    [field: SerializeField] public CardLibrary CardLibrary { get; private set; }
    [field: SerializeField] public BoardPreset TestPreset { get; private set; }
    [field: SerializeField] public bool AutoBuildInPlayMode { get; private set; } = true;
    [field: SerializeField] public float CardWidthToHeight { get; private set; } = 0.75f;

    public event Action<int> CardClicked;


    public IReadOnlyList<CardView> SpawnedCards => spawnedCards;
    public IReadOnlyList<string> DeckCardIds => deckCardIds;

    List<CardView> spawnedCards = new();

    readonly List<string> deckCardIds = new();


    void Start()
    {
        if (!Application.isPlaying)
            return;

        if (AutoBuildInPlayMode && TestPreset != null && CardLibrary != null)
            Build(new GameSessionConfig(TestPreset));
    }

    public void Build(GameSessionConfig session)
    {
        if (session == null || session.Preset == null)
        {
            Debug.LogError("Session or preset is null.");
            return;
        }

        if (BoardArea == null || Grid == null || CardPrefab == null)
        {
            Debug.LogError("BoardView is missing references.");
            return;
        }

        var preset = session.Preset;
        var totalCards = preset.TotalCards;

        if ((totalCards % 2) != 0)
        {
            Debug.LogError("Preset is invalid.");
            return;
        }

        var validCards = CardLibrary != null ? CardLibrary.ValidCards : null;
        if (validCards == null || validCards.Count == 0)
        {
            Debug.LogError("CardLibrary has no valid cards.");
            return;
        }

        var requiredPairs = totalCards / 2;
        if (validCards.Count < requiredPairs)
        {
            Debug.LogError("Not enough unique CardDefinitions.");
            return;
        }

        Clear();

        Grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        Grid.constraintCount = preset.Columns;

        ApplyDynamicCellSize(preset.Rows, preset.Columns);

        var rng = new System.Random(session.Seed);
        var deck = CreateDeck(validCards, requiredPairs, rng);

        deckCardIds.Clear();
        for (var i = 0; i < deck.Count; i++)
            deckCardIds.Add(deck[i].Id);

        for (var i = 0; i < deck.Count; i++)
        {
            var def = deck[i];
            var card = Instantiate(CardPrefab, Grid.transform);
            card.Initialize(i, def.Id, def.Sprite);

            if (card.Button != null)
            {
                var slot = i;
                card.Button.onClick.AddListener(() => CardClicked?.Invoke(slot));
            }

            spawnedCards.Add(card);
        }
    }

    public void Clear()
    {
        spawnedCards.Clear();
        deckCardIds.Clear();

        if (Grid == null)
            return;

        for (var i = Grid.transform.childCount - 1; i >= 0; i--)
            Destroy(Grid.transform.GetChild(i).gameObject);
    }

    void ApplyDynamicCellSize(int rows, int columns)
    {
        Canvas.ForceUpdateCanvases();

        if (BoardArea != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(BoardArea);

        var area = BoardArea.rect.size;

        var padding = Grid.padding;
        var spacing = Grid.spacing;

        var usableWidth = area.x - padding.left - padding.right - (spacing.x * (columns - 1));
        var usableHeight = area.y - padding.top - padding.bottom - (spacing.y * (rows - 1));

        var maxCellWidth = usableWidth / columns;
        var maxCellHeight = usableHeight / rows;

        var aspect = Mathf.Clamp(CardWidthToHeight, 0.1f, 10f);

        var widthFromHeight = maxCellHeight * aspect;
        var heightFromWidth = maxCellWidth / aspect;

        var cellWidth = Mathf.Min(maxCellWidth, widthFromHeight);
        var cellHeight = Mathf.Min(maxCellHeight, heightFromWidth);

        cellWidth = Mathf.Max(1f, Mathf.Floor(cellWidth));
        cellHeight = Mathf.Max(1f, Mathf.Floor(cellHeight));

        Grid.cellSize = new Vector2(cellWidth, cellHeight);
    }

    static List<CardDefinition> CreateDeck(IReadOnlyList<CardDefinition> validCards, int requiredPairs, System.Random rng)
    {
        var selected = new List<CardDefinition>(requiredPairs);
        var pool = new List<CardDefinition>(validCards);
        Shuffle(pool, rng);

        for (var i = 0; i < requiredPairs; i++)
            selected.Add(pool[i]);

        var deck = new List<CardDefinition>(requiredPairs * 2);
        for (var i = 0; i < selected.Count; i++)
        {
            deck.Add(selected[i]);
            deck.Add(selected[i]);
        }

        Shuffle(deck, rng);
        return deck;
    }

    public void BuildFromSavedDeck(GameSessionConfig session, IReadOnlyList<string> savedDeckIds, bool[] matchedSlots)
    {
        if (session == null || session.Preset == null)
            return;

        if (BoardArea == null || Grid == null || CardPrefab == null || CardLibrary == null)
            return;

        var preset = session.Preset;
        var totalCards = preset.TotalCards;

        if (savedDeckIds == null || savedDeckIds.Count != totalCards)
        {
            Debug.LogError("Saved deck is invalid.");
            return;
        }

        Clear();

        Grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        Grid.constraintCount = preset.Columns;

        ApplyDynamicCellSize(preset.Rows, preset.Columns);

        deckCardIds.Clear();
        deckCardIds.AddRange(savedDeckIds);

        for (var i = 0; i < savedDeckIds.Count; i++)
        {
            var id = savedDeckIds[i];

            if (!CardLibrary.TryGetById(id, out var def))
            {
                Debug.LogError($"Missing CardDefinition for id: {id}");
                continue;
            }

            var card = Instantiate(CardPrefab, Grid.transform);
            card.Initialize(i, def.Id, def.Sprite);

            if (matchedSlots != null && i < matchedSlots.Length && matchedSlots[i])
                card.SetMatchedCanvasGroup();

            if (card.Button != null)
            {
                var slot = i;
                card.Button.onClick.AddListener(() => CardClicked?.Invoke(slot));
            }

            spawnedCards.Add(card);
        }
    }

    static void Shuffle<T>(IList<T> list, System.Random rng)
    {
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
