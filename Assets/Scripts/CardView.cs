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

    public int SlotIndex { get; private set; }
    public string CardId { get; private set; }

    public void Initialize(int slotIndex, string cardId, Sprite iconSprite)
    {
        SlotIndex = slotIndex;
        CardId = cardId;

        if (FrontIconImage != null)
            FrontIconImage.sprite = iconSprite;

        SetFaceDown();
        SetInteractable(true);
    }

    public void SetFaceDown()
    {
        if (BackImage != null) BackImage.gameObject.SetActive(true);
        if (FrontRoot != null) FrontRoot.SetActive(false);
    }

    public void SetFaceUp()
    {
        if (BackImage != null) BackImage.gameObject.SetActive(false);
        if (FrontRoot != null) FrontRoot.SetActive(true);
    }

    public void SetMatched()
    {
        SetInteractable(false);

        if (BackImage != null) BackImage.gameObject.SetActive(false);
        if (FrontRoot != null) FrontRoot.SetActive(false);

        if (CanvasGroup != null)
        {
            CanvasGroup.alpha = 0f;
            CanvasGroup.blocksRaycasts = false;
            CanvasGroup.interactable = false;
        }
    }

    public void SetInteractable(bool isInteractable)
    {
        if (Button != null) Button.interactable = isInteractable;
    }
}

