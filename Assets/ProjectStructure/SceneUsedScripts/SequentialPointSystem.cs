using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;


namespace paperScaleOfDifferentLeastCounts
{
    public class SequentialPointSystem : MonoBehaviour
    {
        [SerializeField] GameObject Universal_Drag;
    [Header("Text Spawning")]
    public TextMeshProUGUI textPrefab;
    public Canvas targetCanvas;
    public string startingText = "D";
    public float textSpacing = 20f;   // UI spacing (adjust in inspector)
    [Header("Optional Enable Object Instead Of Text")]
    public bool useEnableDisableMode = false;
    public GameObject objectToEnableOnEachCorrect;
    private List<TextMeshProUGUI> spawnedTexts = new List<TextMeshProUGUI>();
        [Header("Assign Pencil")]
        public Transform pencil;
    
        [Header("Auto Mark Settings")]
        public GameObject autoMarkButton;
        public int autoMarkUnlockCount = 3;
        private bool autoMarkEnabled = false;
        private bool autoMarkListenerAdded = false;
    
        [Header("OPTIONAL: Initial Override")]
        public Transform pencilStartTransform;
        public Transform scale;
        public Transform scaleStartTransform;
    [Header("Dot Parent")]
    public Transform dotParent;   // Assign empty GameObject in Inspector
        [Header("NEW: Rotation Control")]
        public bool useRotationForPencil = false;
    
        [Header("Assign Tip (Child of Pencil)")]
        public Transform tip;
    
        [Header("Optional Start Snap Transform")]
        public Transform startSnapPoint;
    
        [Header("Scale Points (Ruler Direction)")]
        public Transform startPoint;
        public Transform endPoint;
    
        [Header("Single Moving Glow Marker")]
        public Transform glow;
    
        [Header("Glow Spacing")]
        public float glowSpacing = 0.0147f;
    
        [Header("How Many Dots To Spawn")]
        public int totalDots = 20;
    
        [Header("Dot Prefab")]
        public GameObject dotPrefab;
    
        [Header("Drag Settings")]
        public float dragSmoothness = 15f;
    
        [Header("Hit Distance")]
        public float hitDistance = 0.01f;
    
        [Header("Next Glow Delay")]
        public float nextGlowDelay = 0.1f;
    
        [Header("Completion Event")]
        public UnityEvent onAllPointsCompleted;
    
        [Header("Camera Follow")]
        public Transform cameraTransform;
        public Transform followTarget;
        public bool followCameraWhileDragging = false;
        public float cameraFollowSmooth = 10f;
    
        public GameObject zeroMarker;
    
        private Vector3 scaleDir;
        private float scaleLength;
    
        private int currentIndex = 0;
        private bool dragging = false;
        private bool isProcessing = false;
    
        private Vector3 targetPosition;
        private float pencilOriginalY;
    
        private Vector3 initialPencilPosition;
        private Quaternion initialPencilRotation;
        private Vector3 initialGlowPosition;   // 🔥 FIX
    
        void Start()
        {
            // Add AutoMark listener safely
            if (autoMarkButton != null && !autoMarkListenerAdded)
            {
                autoMarkButton.GetComponent<Button>()
                    .onClick.AddListener(AutoMarkAllRemainingDots);
    
                autoMarkListenerAdded = true;
            }
    
            if (glow != null)
            {
                glow.gameObject.SetActive(true);
                initialGlowPosition = glow.position;   // 🔥 STORE ORIGINAL
            }
    
            if (autoMarkButton != null)
                autoMarkButton.SetActive(false);
    
            if (zeroMarker != null)
                zeroMarker.SetActive(true);
    
            if (Universal_Drag != null)
                Universal_Drag.SetActive(false);
    
            if (scale != null && scaleStartTransform != null)
            {
                scale.position = scaleStartTransform.position;
                scale.rotation = scaleStartTransform.rotation;
            }
    
            if (pencil != null && pencilStartTransform != null)
            {
                pencil.position = pencilStartTransform.position;
    
                if (useRotationForPencil)
                    pencil.rotation = pencilStartTransform.rotation;
            }
            else if (startSnapPoint != null && pencil != null)
            {
                pencil.position = startSnapPoint.position;
            }
    
            if (pencil != null)
            {
                initialPencilPosition = pencil.position;
                initialPencilRotation = pencil.rotation;
            }
    
            scaleDir = (endPoint.position - startPoint.position).normalized;
            scaleLength = Vector3.Distance(startPoint.position, endPoint.position);
    
            pencilOriginalY = pencil.position.y;
            targetPosition = pencil.position;
        }
    
        void LateUpdate()
        {
            if (!followCameraWhileDragging) return;
            if (!dragging) return;
            if (cameraTransform == null || followTarget == null) return;
    
            Vector3 desiredPosition = followTarget.position;
            desiredPosition.z = cameraTransform.position.z;
    
            cameraTransform.position = Vector3.Lerp(
                cameraTransform.position,
                desiredPosition,
                Time.deltaTime * cameraFollowSmooth
            );
        }
    
        void Update()
        {
            if (pencil == null || tip == null || glow == null) return;
    
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (hit.transform == pencil || hit.transform == tip)
                        dragging = true;
                }
            }
    
            if (Input.GetMouseButtonUp(0))
                dragging = false;
    
            if (dragging)
                SlideOnScaleLocked();
    
            Vector3 smoothPos = Vector3.Lerp(
                pencil.position,
                targetPosition,
                Time.deltaTime * dragSmoothness
            );
    
            smoothPos.y = pencilOriginalY;
            pencil.position = smoothPos;
    
            CheckGlowHit();
        }
    
        void SlideOnScaleLocked()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane plane = new Plane(-Camera.main.transform.forward, startPoint.position);
    
            if (plane.Raycast(ray, out float distance))
            {
                Vector3 hitPoint = ray.GetPoint(distance);
    
                float projected = Vector3.Dot(hitPoint - startPoint.position, scaleDir);
                float glowProjected = Vector3.Dot(glow.position - startPoint.position, scaleDir);
    
                projected = Mathf.Clamp(projected, 0f, glowProjected);
    
                Vector3 newPos = startPoint.position + scaleDir * projected;
                newPos.y = pencilOriginalY;
    
                targetPosition = newPos;
            }
        }
    
        void CheckGlowHit()
        {
            if (isProcessing) return;
            if (currentIndex >= totalDots) return;
    
            float distance = Vector3.Distance(tip.position, glow.position);
    
            if (distance <= hitDistance)
            {
                isProcessing = true;
    
                Vector3 snapPos = glow.position;
                snapPos.y = pencilOriginalY;
                targetPosition = snapPos;
    
            GameObject newDot = Instantiate(dotPrefab, glow.position, dotPrefab.transform.rotation);
    
    if (dotParent != null)
        newDot.transform.SetParent(dotParent);
    
    if (useEnableDisableMode)
    {
        if (objectToEnableOnEachCorrect != null)
            objectToEnableOnEachCorrect.SetActive(true);
    }
    else
    {
        SpawnSingleText(currentIndex);
    }
    
    StartCoroutine(MoveGlowForward());
        }
    
    IEnumerator MoveGlowForward()
    {
        yield return new WaitForSeconds(nextGlowDelay);
    
        currentIndex++;
    
        // 🔥 FIRST: Check if player completed all dots manually
        if (currentIndex >= totalDots)
        {
            glow.gameObject.SetActive(false);
    
            // 🔥 Hide AutoMark safely
            if (autoMarkButton != null)
                autoMarkButton.SetActive(false);
    
            autoMarkEnabled = false;
    
            isProcessing = false;
    
            onAllPointsCompleted?.Invoke();
            yield break;
        }
    
        // 🔥 THEN: Enable AutoMark if unlock condition reached
        if (!autoMarkEnabled && currentIndex >= autoMarkUnlockCount)
        {
            if (autoMarkButton != null)
                autoMarkButton.SetActive(true);
    
            autoMarkEnabled = true;
        }
    
        // 🔥 Move glow forward normally
        glow.position += scaleDir * glowSpacing;
    
        isProcessing = false;
    }
        }
     public void AutoMarkAllRemainingDots()
    {
        if (currentIndex >= totalDots)
            return;
    
        StopAllCoroutines();   // Prevent coroutine conflicts
        isProcessing = false;
    
        Vector3 spawnPos = glow.position;
        int remaining = totalDots - currentIndex;
    
        for (int i = 0; i < remaining; i++)
        {
            GameObject newDot = Instantiate(dotPrefab, spawnPos, dotPrefab.transform.rotation);
    
            // 🔥 Parent assignment
            if (dotParent != null)
                newDot.transform.SetParent(dotParent);
                SpawnSingleText(currentIndex + i);
    
            // 🔥 Move to next spacing position
            spawnPos += scaleDir * glowSpacing;
        }
    
        // 🔥 Move glow to final correct position
        glow.position = initialGlowPosition + scaleDir * (glowSpacing * totalDots);
    
        currentIndex = totalDots;
    
        glow.gameObject.SetActive(false);
    
        // 🔥 Move pencil to final position
        Vector3 finalPos = glow.position;
        finalPos.y = pencilOriginalY;
    
        pencil.position = finalPos;
        targetPosition = finalPos;
    
        // 🔥 Hide AutoMark button
        if (autoMarkButton != null)
            autoMarkButton.SetActive(false);
    
        onAllPointsCompleted?.Invoke();
    
        MoveCameraToFinalTargetInstant();
    }
        void MoveCameraToFinalTargetInstant()
        {
            if (cameraTransform == null || followTarget == null)
                return;
    
            Vector3 desiredPosition = followTarget.position;
            desiredPosition.z = cameraTransform.position.z;
    
            cameraTransform.position = desiredPosition;
        }
    void SpawnSingleText(int index)
    {
        if (textPrefab == null || targetCanvas == null)
            return;
    
        TextMeshProUGUI newText =
            Instantiate(textPrefab, targetCanvas.transform);
    
        // 🔥 Text starts from A1
        newText.text = startingText + (index + 1).ToString();
    
        // 🔥 Get prefab local position
        Vector3 basePos = textPrefab.transform.localPosition;
    
        // 🔥 Only modify Z
        basePos.z += textSpacing * index;
    
        newText.transform.localPosition = basePos;
    
        // 🔥 Keep prefab rotation
        newText.transform.localRotation = textPrefab.transform.localRotation;
    
        spawnedTexts.Add(newText);
    }
        
        public void ResetPencil()
        {
            dragging = false;
            isProcessing = false;
            StopAllCoroutines();   // 🔥 IMPORTANT
    
            pencil.position = initialPencilPosition;
            pencil.rotation = initialPencilRotation;
    
            targetPosition = initialPencilPosition;
            pencilOriginalY = initialPencilPosition.y;
    
            currentIndex = 0;
            autoMarkEnabled = false;
    
            if (autoMarkButton != null)
                autoMarkButton.SetActive(false);
    
            // 🔥 Proper glow reset
            glow.position = initialGlowPosition;
            glow.gameObject.SetActive(true);
        }
        }
    
}