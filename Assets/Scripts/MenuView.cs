using System;
using UnityEngine;
using UnityEngine.UI;

public class MenuView : MonoBehaviour
{
    [field: SerializeField] public Button Btn2x2 { get; private set; }
    [field: SerializeField] public Button Btn3x4 { get; private set; }
    [field: SerializeField] public Button Btn4x4 { get; private set; }
    [field: SerializeField] public Button Btn6x5 { get; private set; }
    [field: SerializeField] public Button Btn4x7 { get; private set; }

    [field: SerializeField] public BoardPreset Preset2x2 { get; private set; }
    [field: SerializeField] public BoardPreset Preset3x4 { get; private set; }
    [field: SerializeField] public BoardPreset Preset4x4 { get; private set; }
    [field: SerializeField] public BoardPreset Preset6x5 { get; private set; }
    [field: SerializeField] public BoardPreset Preset4x7 { get; private set; }

    public event Action<BoardPreset> PresetSelected;

    void OnEnable()
    {
        if (Btn2x2 != null) Btn2x2.onClick.AddListener(On2x2);
        if (Btn3x4 != null) Btn3x4.onClick.AddListener(On3x4);
        if (Btn4x4 != null) Btn4x4.onClick.AddListener(On4x4);
        if (Btn6x5 != null) Btn6x5.onClick.AddListener(On6x5);
        if (Btn4x7 != null) Btn4x7.onClick.AddListener(On4x7);
    }

    void OnDisable()
    {
        if (Btn2x2 != null) Btn2x2.onClick.RemoveListener(On2x2);
        if (Btn3x4 != null) Btn3x4.onClick.RemoveListener(On3x4);
        if (Btn4x4 != null) Btn4x4.onClick.RemoveListener(On4x4);
        if (Btn6x5 != null) Btn6x5.onClick.RemoveListener(On6x5);
        if (Btn4x7 != null) Btn4x7.onClick.RemoveListener(On4x7);

    }

    void On2x2() => PresetSelected?.Invoke(Preset2x2);
    void On3x4() => PresetSelected?.Invoke(Preset3x4);
    void On4x4() => PresetSelected?.Invoke(Preset4x4);
    void On6x5() => PresetSelected?.Invoke(Preset6x5);
    void On4x7() => PresetSelected?.Invoke(Preset4x7);
}
