using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIDragPen : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Drop Target")]
    public RectTransform dropSlot;
    public GameObject dragPanel;

    [Header("Correct Pen (Pen2)")]
    public Image highlightPenImage;   // Pen2 Image component

    public float snapDistance = 50f;

    private RectTransform rect;
    private Vector2 startPosition;
    private Canvas canvas;

    private bool snapped = false;

    public System.Action onSnapped;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (snapped) return;
        startPosition = rect.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (snapped) return;

        rect.anchoredPosition +=
            eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (snapped) return;

        float distance = Vector2.Distance(
            rect.position,
            dropSlot.position
        );

        if (distance < snapDistance)
        {
            rect.position = dropSlot.position;
            snapped = true;

            // ?? Remove highlight material from Pen2
            if (highlightPenImage != null)
                highlightPenImage.material = null;

            // ?? Hide Pen1 (draggable)
            gameObject.SetActive(false);
            dragPanel.SetActive(false);

            onSnapped?.Invoke();
        }
        else
        {
            rect.anchoredPosition = startPosition;
        }
    }
}
