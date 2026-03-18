using UnityEngine;
using System.Collections;


namespace paperScaleOfDifferentLeastCounts
{
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(Animator))]
    public class SlideTriggeredAnimator : MonoBehaviour
    {
        [Header("References")]
        public SlideCameraController slideController;
        private Animator anim;
    
        [Header("Settings")]
        public int allowedSlideNumber;   // Slide number where animation is allowed
        public string animationTriggerName = "Play"; // Animator Trigger name
    
        [Header("Debug")]
        public bool disableAfterPlay = true;
    
        private bool hasPlayed = false;
        private bool isPlaying = false;
    
        void Awake()
        {
            anim = GetComponent<Animator>();
        }
    
        void OnMouseDown()
        {
            if (isPlaying) return;
            if (hasPlayed && disableAfterPlay) return;
    
            // 🔥 Check slide number
            if (slideController.CurrentPageNumber != allowedSlideNumber)
                return;
    
            // 🔥 Make sure camera finished moving
            if (!slideController.CameraAtTarget)
                return;
    
            StartCoroutine(PlayAnimation());
        }
    
        IEnumerator PlayAnimation()
        {
            isPlaying = true;
    
            // 🔒 Disable navigation buttons
            slideController.nextButton.interactable = false;
            slideController.previousButton.interactable = false;
    
            // 🎬 Play animation
            anim.SetTrigger(animationTriggerName);
    
            // Wait until animation actually starts
            yield return null;
    
            // Wait until animation finishes
            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            float animLength = stateInfo.length;
    
            yield return new WaitForSeconds(animLength);
    
            hasPlayed = true;
            isPlaying = false;
    
            // 🔓 Enable next slide
            slideController.EnableNextButton();
        }
    }
    
}