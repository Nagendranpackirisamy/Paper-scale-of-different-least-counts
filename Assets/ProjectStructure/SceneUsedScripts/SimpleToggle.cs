using UnityEngine;


namespace paperScaleOfDifferentLeastCounts
{
    public class SimpleToggle : MonoBehaviour
    {
        public GameObject target;
    
        public void ToggleObject()
        {
            if (target != null)
            {
                target.SetActive(!target.activeSelf);
            }
        }
    }
    
    
}