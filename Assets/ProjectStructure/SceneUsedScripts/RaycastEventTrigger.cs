using UnityEngine;
using UnityEngine.Events;


namespace paperScaleOfDifferentLeastCounts
{
    public class RaycastEventTrigger : MonoBehaviour
    {
        [Header("Raycast Settings")]
        public Camera rayCamera;            // Camera used for raycast (assign Main Camera)
        public float maxDistance = 100f;    // Ray distance
    
        [Header("Target (Optional)")]
        public Transform targetObject;      // If assigned, event fires ONLY when this object is hit
        public LayerMask layerMask = ~0;    // Optional layer filter
    
        [Header("Event")]
        public UnityEvent onRaycastHit;     // 🔥 Event that fires on hit
    
        void Update()
        {
            if (Input.GetMouseButtonDown(0)) // Left click
            {
                FireRaycast();
            }
        }
    
        void FireRaycast()
        {
            if (rayCamera == null)
                rayCamera = Camera.main;
    
            Ray ray = rayCamera.ScreenPointToRay(Input.mousePosition);
    
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, layerMask))
            {
                // If no target assigned → fire on ANY hit
                if (targetObject == null)
                {
                    onRaycastHit?.Invoke();
                    return;
                }
    
                // If target assigned → fire ONLY when that object is hit
                if (hit.transform == targetObject || hit.transform.IsChildOf(targetObject))
                { Debug.Log("Hitting");
                    onRaycastHit?.Invoke();
                }
            }
        }
    }
    
    
}