using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Train Your Brain/Card Library")]
public class CardLibrary : ScriptableObject
{
    [field: SerializeField] public List<CardDefinition> Cards { get; private set; } = new();

    public IReadOnlyList<CardDefinition> ValidCards =>
        Cards.Where(c => c != null && !string.IsNullOrWhiteSpace(c.Id) && c.Sprite != null).ToList();
}