using System;

[Serializable]
public class GameSessionConfig
{
    public BoardPreset Preset { get; }
    public int Seed { get; }

    public GameSessionConfig(BoardPreset preset)
    {
        Preset = preset;
        Seed = new Random().Next();
    }

    public GameSessionConfig(BoardPreset preset, int seed)
    {
        Preset = preset;
        Seed = seed;
    }
}
