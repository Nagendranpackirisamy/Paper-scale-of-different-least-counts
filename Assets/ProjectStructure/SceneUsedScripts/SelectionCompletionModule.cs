using UnityEngine;


namespace paperScaleOfDifferentLeastCounts
{
    public class SelectionCompletionModule : MonoBehaviour
    {
        [Header("Completion Settings")]
        public int totalCorrectRequired = 1;
    
        public int currentCorrect = 0;
    
        private SlideCameraController slideCameraController;
    
        void Awake()
        {
            // Find controller in scene
            slideCameraController = FindFirstObjectByType<SlideCameraController>();
    
            if (slideCameraController == null)
            {
                Debug.LogError("SlideCameraController not found in scene.");
            }
        }
    
        public void RegisterCorrect()
        {
            currentCorrect++;
    
            Debug.Log($"Correct count: {currentCorrect}/{totalCorrectRequired}");
    
            if (currentCorrect >= totalCorrectRequired)
            {
                CompleteSlide();
            }
        }
    
        void CompleteSlide()
        {
            if (slideCameraController != null)
            {
                slideCameraController.EnableNextButton();
    
                Debug.Log("Slide completed. Next button enabled.");
            }
        }
    
        public void ResetModule()
        {
            currentCorrect = 0;
        }
    }
    
}