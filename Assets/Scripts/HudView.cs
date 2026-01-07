using TMPro;
using UnityEngine;

public class HudView : MonoBehaviour
{
    [field: SerializeField] public TMP_Text MatchesText { get; private set; }
    [field: SerializeField] public TMP_Text TurnsText { get; private set; }
    [field: SerializeField] public TMP_Text ScoreText { get; private set; }

    public void SetMatches(int matchedPairs, int totalPairs)
    {
        if (MatchesText != null)
            MatchesText.text = $"Matches: {matchedPairs}/{totalPairs}";
    }

    public void SetTurns(int turns)
    {
        if (TurnsText != null)
            TurnsText.text = $"Turns: {turns}";
    }

    public void SetScore(int score)
    {
        if (ScoreText != null)
            ScoreText.text = $"Score: {score}";
    }
}

