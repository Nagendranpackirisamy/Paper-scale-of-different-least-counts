using UnityEngine;


namespace paperScaleOfDifferentLeastCounts
{
    public class SwitchFromSequenceToPencilDrag : MonoBehaviour
    {
        [Header("Assign 3 Sequence Objects")]
        public GameObject[] sequenceObjects;   // Assign 3 objects here
    
        [Header("Assign Raycast Scale Object")]
        public GameObject raycastScale;
    
        // 🔥 SINGLE FUNCTION — Call from UnityEvent
        public void SwitchFromSequenceToPencilDragFunction(int sequenceIndex, bool raycastState)
        {
            // Turn OFF selected sequence object
            if (sequenceObjects != null &&
                sequenceIndex >= 0 &&
                sequenceIndex < sequenceObjects.Length)
            {
                if (sequenceObjects[sequenceIndex] != null)
                    sequenceObjects[sequenceIndex].SetActive(false);
            }
    
            // Set raycast scale state
            if (raycastScale != null)
            {
                raycastScale.SetActive(raycastState);
            }
    
            Debug.Log("Switch executed for index: " + sequenceIndex);
        }
    }
    
}