using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Train Your Brain/Card Library")]
public class CardLibrary : ScriptableObject
{
    [field: SerializeField] public List<CardDefinition> Cards { get; private set; } = new();

    Dictionary<string, CardDefinition> byId;

    public IReadOnlyList<CardDefinition> ValidCards => Cards;

    public bool TryGetById(string id, out CardDefinition def)
    {
        if (string.IsNullOrEmpty(id))
        {
            def = null;
            return false;
        }

        byId ??= BuildLookup();
        return byId.TryGetValue(id, out def) && def != null;
    }

    Dictionary<string, CardDefinition> BuildLookup()
    {
        var dict = new Dictionary<string, CardDefinition>();
        if (Cards == null)
            return dict;

        for (var i = 0; i < Cards.Count; i++)
        {
            var c = Cards[i];
            if (c == null || string.IsNullOrEmpty(c.Id))
                continue;

            dict[c.Id] = c;
        }

        return dict;
    }
}
