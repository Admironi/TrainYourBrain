using UnityEngine;

public class AudioService : MonoBehaviour
{
    [field: SerializeField] public AudioSource Source { get; private set; }

    [field: SerializeField] public AudioClip ButtonClick { get; private set; }
    [field: SerializeField] public AudioClip CardFlip { get; private set; }
    [field: SerializeField] public AudioClip Match { get; private set; }
    [field: SerializeField] public AudioClip Mismatch { get; private set; }
    [field: SerializeField] public AudioClip GameWon { get; private set; }

    [field: SerializeField, Range(0f, 1f)] public float UiVolume { get; private set; } = 0.9f;
    [field: SerializeField, Range(0f, 1f)] public float GameVolume { get; private set; } = 0.9f;

    void Awake()
    {
        if (Source == null)
            Source = GetComponent<AudioSource>();
    }

    public void PlayButtonClick() => Play(ButtonClick, UiVolume);
    public void PlayCardFlip() => Play(CardFlip, GameVolume);
    public void PlayMatch() => Play(Match, GameVolume);
    public void PlayMismatch() => Play(Mismatch, GameVolume);
    public void PlayGameWon() => Play(GameWon, GameVolume);

    void Play(AudioClip clip, float volume)
    {
        if (Source == null || clip == null)
            return;

        Source.PlayOneShot(clip, volume);
    }
}
