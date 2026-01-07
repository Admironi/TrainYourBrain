using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    [field: SerializeField] public Button Button { get; private set; }
    [field: SerializeField] public Image BackImage { get; private set; }
    [field: SerializeField] public GameObject FrontRoot { get; private set; }
    [field: SerializeField] public Image FrontFrameImage { get; private set; }
    [field: SerializeField] public Image FrontIconImage { get; private set; }
    [field: SerializeField] public CanvasGroup CanvasGroup { get; private set; }

    [field: SerializeField] public RectTransform FlipRoot { get; private set; }
    [field: SerializeField] public float FlipHalfDuration { get; private set; } = 0.12f;

    public int SlotIndex { get; private set; }
    public string CardId { get; private set; }
    public bool IsFaceUp { get; private set; }

    Coroutine flipRoutine;

    public void Initialize(int slotIndex, string cardId, Sprite iconSprite)
    {
        SlotIndex = slotIndex;
        CardId = cardId;

        if (FrontIconImage != null)
            FrontIconImage.sprite = iconSprite;

        if (CanvasGroup != null)
        {
            CanvasGroup.alpha = 1f;
            CanvasGroup.blocksRaycasts = true;
            CanvasGroup.interactable = true;
        }

        if (FlipRoot == null)
            FlipRoot = (RectTransform)transform;

        SetFaceDownInstant();
        SetInteractable(true);
    }

    public void SetInteractable(bool value)
    {
        if (Button != null)
            Button.interactable = value;
    }

    public void SetFaceUpInstant()
    {
        IsFaceUp = true;

        if (BackImage != null)
            BackImage.gameObject.SetActive(false);

        if (FrontRoot != null)
            FrontRoot.SetActive(true);

        ResetScale();
    }

    public void SetFaceDownInstant()
    {
        IsFaceUp = false;

        if (BackImage != null)
            BackImage.gameObject.SetActive(true);

        if (FrontRoot != null)
            FrontRoot.SetActive(false);

        ResetScale();
    }

    public void FlipToFaceUp()
    {
        StartFlip(true);
    }

    public void FlipToFaceDown()
    {
        StartFlip(false);
    }

    void StartFlip(bool toFaceUp)
    {
        if (flipRoutine != null)
            StopCoroutine(flipRoutine);

        flipRoutine = StartCoroutine(FlipRoutine(toFaceUp));
    }

    IEnumerator FlipRoutine(bool toFaceUp)
    {
        var root = FlipRoot != null ? FlipRoot : (RectTransform)transform;
        var t = 0f;

        while (t < FlipHalfDuration)
        {
            t += Time.unscaledDeltaTime;
            var a = Mathf.Clamp01(t / FlipHalfDuration);
            root.localScale = new Vector3(Mathf.Lerp(1f, 0f, a), 1f, 1f);
            yield return null;
        }

        if (toFaceUp)
            SetFaceUpInstant();
        else
            SetFaceDownInstant();

        t = 0f;

        while (t < FlipHalfDuration)
        {
            t += Time.unscaledDeltaTime;
            var a = Mathf.Clamp01(t / FlipHalfDuration);
            root.localScale = new Vector3(Mathf.Lerp(0f, 1f, a), 1f, 1f);
            yield return null;
        }

        root.localScale = Vector3.one;
        flipRoutine = null;
    }

    void ResetScale()
    {
        var root = FlipRoot != null ? FlipRoot : (RectTransform)transform;
        root.localScale = Vector3.one;
    }

    public void SetMatchedCanvasGroup()
    {
        SetInteractable(false);

        if (BackImage != null)
            BackImage.gameObject.SetActive(false);

        if (FrontRoot != null)
            FrontRoot.SetActive(false);

        if (CanvasGroup != null)
        {
            CanvasGroup.alpha = 0f;
            CanvasGroup.blocksRaycasts = false;
            CanvasGroup.interactable = false;
        }
    }
}
