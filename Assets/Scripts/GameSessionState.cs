using UnityEditor.Presets;

public class GameSessionState
{
    public int TotalPairs { get; }
    public int MatchedPairs { get; private set; }
    public int Turns { get; private set; }
    public int Score { get; private set; }
    public int ComboStreak { get; private set; }

    readonly int baseMatchScore;
    readonly int comboBonusStep;

    public GameSessionState(int totalPairs, int baseMatchScore, int comboBonusStep)
    {
        TotalPairs = totalPairs;
        this.baseMatchScore = baseMatchScore;
        this.comboBonusStep = comboBonusStep;

        MatchedPairs = 0;
        Turns = 0;
        Score = 0;
        ComboStreak = 0;
    }

    public void LoadProgress(int matchedPairs, int turns, int score, int comboStreak)
    {
        MatchedPairs = matchedPairs;
        Turns = turns;
        Score = score;
        ComboStreak = comboStreak;
    }

    public void RegisterTurn()
    {
        Turns++;
    }

    public int RegisterMatch()
    {
        MatchedPairs++;

        var gained = baseMatchScore + (ComboStreak * comboBonusStep);
        Score += gained;

        ComboStreak++;
        return gained;
    }

    public void RegisterMismatch()
    {
        ComboStreak = 0;
    }

    public bool IsComplete()
    {
        return TotalPairs > 0 && MatchedPairs >= TotalPairs;
    }
}
