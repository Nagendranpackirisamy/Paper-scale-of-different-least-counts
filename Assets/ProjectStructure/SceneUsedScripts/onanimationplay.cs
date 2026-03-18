using UnityEngine;
using UnityEngine.Events;   // 👈 Required for UnityEvent


namespace paperScaleOfDifferentLeastCounts
{
    public class onanimationplay : MonoBehaviour
    {
        public GameObject obejcttodisable;
        public GameObject obejcttoenable;
        public GameObject whitpaper;
    
        // 🔥 NEW PUBLIC EVENT
        public UnityEvent onAnimationInvoked;
    
        private Vector3 initialPosition;
        private Quaternion initialRotation;
    
        void Start()
        {
            if (whitpaper != null)
            {
                initialPosition = whitpaper.transform.position;
                initialRotation = whitpaper.transform.rotation;
            }
        }
    
        public void onanimmation()
        {
            onAnimationInvoked?.Invoke();
            if (obejcttoenable != null)
                obejcttoenable.SetActive(true);
    
            if (obejcttodisable != null)
                obejcttodisable.SetActive(false);
    
        }
    
        public void onanimmationend()
        {
            if (whitpaper != null)
            {
                whitpaper.transform.position = initialPosition;
                whitpaper.transform.rotation = initialRotation;
            }
        }
    }
    
}