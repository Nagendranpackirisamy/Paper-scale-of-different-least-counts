using UnityEngine;

public class DragMeasureController : MonoBehaviour
{
    [Header("References")]
    public UIDragPen dragPen;
    public GameObject numpadPanel;
    public GameObject textComponenet;
    public NumpadController numpadController;

    void Start()
    {
        numpadPanel.SetActive(false);
        textComponenet.SetActive(false);

        dragPen.onSnapped += OnPenSnapped;
    }

    void OnPenSnapped()
    {
        numpadPanel.SetActive(true);
        textComponenet.SetActive(true);
    }
}
