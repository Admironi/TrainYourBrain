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
}
