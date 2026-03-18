using UnityEngine;


namespace paperScaleOfDifferentLeastCounts
{
    public class DotPositionRecorder : MonoBehaviour
    {
        [Header("Assign ALL your dots here (in order)")]
        public Transform[] dots;
    
        [Header("Press this key to record positions")]
        public KeyCode recordKey = KeyCode.R;
    
        void Update()
        {
            if (Input.GetKeyDown(recordKey))
            {
                Debug.Log("===== DOT POSITIONS (COPY THIS) =====");
    
                for (int i = 0; i < dots.Length; i++)
                {
                    if (dots[i] == null) continue;
    
                    Vector3 p = dots[i].position;
    
                    Debug.Log(
                        $"Dot {i}: new Vector3({p.x}f, {p.y}f, {p.z}f),"
                    );
                }
    
                Debug.Log("===== END =====");
            }
        }
    }
    
    
}