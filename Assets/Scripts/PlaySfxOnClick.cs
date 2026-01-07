using UnityEngine;
using UnityEngine.UI;

public class PlaySfxOnClick : MonoBehaviour
{
    [field: SerializeField] public AudioService Audio { get; private set; }
    [field: SerializeField] public Button Button { get; private set; }

    void Awake()
    {
        if (Button == null)
            Button = GetComponent<Button>();
    }

    void OnEnable()
    {
        if (Button != null)
            Button.onClick.AddListener(OnClicked);
    }

    void OnDisable()
    {
        if (Button != null)
            Button.onClick.RemoveListener(OnClicked);
    }

    void OnClicked()
    {
        if (Audio != null)
            Audio.PlayButtonClick();
    }
}
