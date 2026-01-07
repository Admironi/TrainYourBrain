using System;
using UnityEngine;
using UnityEngine.UI;

public class ContinuePopupView : MonoBehaviour
{
    [field: SerializeField] public Button ContinueButton { get; private set; }
    [field: SerializeField] public Button CloseButton { get; private set; }

    public event Action ContinueClicked;
    public event Action CloseClicked;

    void Awake()
    {
        if (ContinueButton == null || CloseButton == null)
            Debug.LogError("buttons are not assigned.", this);
    }

    void OnEnable()
    {
        ContinueButton.onClick.AddListener(OnContinue);
        CloseButton.onClick.AddListener(OnClose);
    }

    void OnDisable()
    {
        ContinueButton.onClick.RemoveListener(OnContinue);
        CloseButton.onClick.RemoveListener(OnClose);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    void OnContinue()
    {
        ContinueClicked?.Invoke();
    }

    void OnClose()
    {
        CloseClicked?.Invoke();
    }
}
