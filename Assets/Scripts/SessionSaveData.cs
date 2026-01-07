using System;

[Serializable]
public class SessionSaveData
{
    public string PresetName;
    public int Rows;
    public int Columns;

    public int Seed;

    public int Turns;
    public int Score;
    public int ComboStreak;
    public int MatchedPairs;

    public string[] DeckCardIds;
    public bool[] MatchedSlots;
}