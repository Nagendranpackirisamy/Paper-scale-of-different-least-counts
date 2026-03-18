using UnityEngine;


namespace paperScaleOfDifferentLeastCounts
{
    public class DropSlot : MonoBehaviour
    {
        [Header("References")]
        public GameObject highlight;
        public GameObject finalModel;
    
        // ─── CHANGE HERE ────────────────────────────────
        [Header("Slide System")]
        public SlideCameraController slideController;
        // ───────────────────────────────────────────────
    
        // ─── NEW (for multi-step experiment) ────────────
        [Header("Experiment Flow")]
        public DropSlot requiredPreviousSlot;
        public Transform snapPoint;
        public bool parentToPrevious = true;
        // ───────────────────────────────────────────────
    
        private bool isSnapped = false;
    
        private void Awake()
        {
            if (highlight != null)
                highlight.SetActive(false);
    
            if (finalModel != null)
                finalModel.SetActive(false);
        }
    
        public void ShowHighlight(bool show)
        {
            if (highlight != null)
            {
                highlight.SetActive(isSnapped ? false : show);
            }
        }
    
        public void Snap()
        {
            // ─── ORDER CHECK ───
            if (requiredPreviousSlot != null &&
                !requiredPreviousSlot.IsCompleted())
                return;
    
            if (isSnapped) return;
            isSnapped = true;
    
            // ─── POSITION STACKING ───
            if (snapPoint != null && finalModel != null)
            {
                finalModel.transform.position = snapPoint.position;
                finalModel.transform.rotation = snapPoint.rotation;
    
                if (parentToPrevious && requiredPreviousSlot != null)
                {
                    finalModel.transform.SetParent(
                        requiredPreviousSlot.finalModel.transform
                    );
                }
            }
    
            if (finalModel != null)
                finalModel.SetActive(true);
    
            if (highlight != null)
                highlight.SetActive(false);
    
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;
    
            // ─── CONNECT TO YOUR SYSTEM ───
            if (slideController != null)
            {
                slideController.EnableNextButton();
            }
        }
    
        public bool IsCompleted()
        {
            return isSnapped;
        }
    }
    
    
}