using UnityEngine;
using System.Collections;


namespace paperScaleOfDifferentLeastCounts
{
    public class SelectableObject : MonoBehaviour
    {
        public bool isCorrect;
    
        [Header("Indicators")]
        public GameObject correctIndicator;
        public GameObject wrongIndicator;
    
        [Header("Wrong Explanation Panel")]
        public GameObject wrongExplanationPanel;
        public float autoHideTime = 2f;
    
        private bool hasSelected = false;
    
        private SelectionCompletionModule completionModule;
    
        private void Start()
        {
            completionModule = GetComponentInParent<SelectionCompletionModule>();
    
            if (correctIndicator != null)
                correctIndicator.SetActive(false);
    
            if (wrongIndicator != null)
                wrongIndicator.SetActive(false);
    
            if (wrongExplanationPanel != null)
                wrongExplanationPanel.SetActive(false);
        }
    
        public void OnButtonPressed()
        {
            if (hasSelected) return;
    
            hasSelected = true;
    
            if (isCorrect)
            {
                if (correctIndicator != null)
                    correctIndicator.SetActive(true);
    
                PlaySFX("Correct");
    
                if (completionModule != null)
                    completionModule.RegisterCorrect();
            }
            else
            {
                if (wrongIndicator != null)
                    wrongIndicator.SetActive(true);
    
                PlaySFX("Wrong");
    
                if (wrongExplanationPanel != null)
                    StartCoroutine(ShowWrongPanel());
            }
        }
    
        IEnumerator ShowWrongPanel()
        {
            wrongExplanationPanel.SetActive(true);
            yield return new WaitForSeconds(autoHideTime);
            wrongExplanationPanel.SetActive(false);
        }
    
        void PlaySFX(string clipName)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(clipName);
            }
            else
            {
                Debug.LogWarning("AudioManager not found in scene!");
            }
        }
    }
    
}