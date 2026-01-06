using UnityEngine;

[CreateAssetMenu(menuName = "Train Your Brain/Board Preset")]
public class BoardPreset : ScriptableObject
{
    [field: SerializeField] public int Rows { get; private set; }
    [field: SerializeField] public int Columns { get; private set; }

    public int TotalCards => Rows * Columns;
    public int TotalPairs => TotalCards / 2;
}
