using UnityEngine;

[CreateAssetMenu(menuName = "Train Your Brain/Card Definition")]
public class CardDefinition : ScriptableObject
{
    [field: SerializeField] public string Id { get; private set; }
    [field: SerializeField] public Sprite Sprite { get; private set; }
}
