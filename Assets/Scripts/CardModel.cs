public enum CardState
{
    FaceDown,
    FaceUp,
    Matched
}

public class CardModel
{
    public int SlotIndex { get; }
    public string CardId { get; }
    public CardState State { get; set; }

    public CardModel(int slotIndex, string cardId)
    {
        SlotIndex = slotIndex;
        CardId = cardId;
        State = CardState.FaceDown;
    }
}
