using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


namespace paperScaleOfDifferentLeastCounts
{
    [System.Serializable]
    public class SlideData
    {
        [Header("Slide Positions")]
        public Transform pencilPos;
        public Transform scalePos;
    
        [Header("Scale Points")]
        public Transform startPoint;
        public Transform endPoint;
    }
    
    [System.Serializable]
    public class SlideSet
    {[Header("Text Spawning")]
    public TextMeshProUGUI textPrefab;
    public Canvas targetCanvas;
    public string startingText;
    public float textSpacing = 0.0204f;
        public int startIndex;
        public int count;
    
        [Header("Per Set Step Control")]
        public bool useStepGap = true;
        public float stepGap = 0.0203f;
    
        [Header("Per Set Axis Control")]
        public bool moveOnZAxis = false;
    
        [Header("Per Set Camera Follow (NEW)")]
        public bool followCameraWhileDragging = false;
        public float cameraFollowSmooth = 10f;
        public Transform cameraFollowTarget;
    
        [Header("Per Set Events (NEW)")]
        public UnityEvent onEachLineCompleted;
        public UnityEvent onSetCompleted;
    }
    
    public class PencilDraw : MonoBehaviour
    {[Header("Line Parent")]
    public Transform lineParent;   // Assign empty GameObject in Inspector
        private List<TextMeshProUGUI> spawnedTexts = new List<TextMeshProUGUI>();
        [Header("Camera Follow (UPDATED - Per Set, No Offset Needed)")]
        public Transform cameraTransform;
        public Transform followTarget;
    private Transform activeFollowTarget;   // 🔥 NEW
    private bool currentStepCompleted = false;
        private bool allowDragging = true;
        private bool forceStopFollow = false;
    private Vector3 cameraVelocity = Vector3.zero;
        [Header("Single Objects")]
        public Transform pencil;
        public Transform tip;
        public Transform scale;
        public GameObject raycastscale;
        public GameObject highlight_Scale;
        private SlideCameraController slideController;
    
        [Header("Slides (ONLY 1 reference needed per set)")]
        public List<SlideData> slides = new List<SlideData>();
    
        [Header("Auto Arrange")]
        public bool useAutoArrange = true;
        public List<SlideSet> slideSets = new List<SlideSet>();
    
        [Header("Reset Scale Position (NEW)")]
        public Transform resetScaleTransform;
    
        [Header("Drawing Surface")]
        public Transform drawingSurface;
    
        [Header("UI Controls")]
        public GameObject autoMarkButton;
        public int autoMarkUnlockLineCount = 2;
    
        public float surfaceOffset = 0.002f;
    
        [Header("Line Settings")]
        public Material lineMaterial;
        public float lineWidth = 0.02f;
    
        // 🔥 SPECIAL MODE VARIABLES
        private bool nextSlideOnLineDrawMode = false;
        private int linesDrawnInSpecialMode = 0;
        private bool autoMarkListenerAdded = false;
    
        private LineRenderer line;
        private List<Vector3> points = new List<Vector3>();
    
        private int currentSetIndex = 0;
        private int currentSlideIndex = 0;
        private bool dragging = false;
    
        private float scaleLength;
        private Vector3 scaleDir;
    
        private Transform startPoint;
        private Transform endPoint;
    
        private float lastProjected = 0f;
    
        private Vector3 baseScalePos;
        private Vector3 basePencilPos;
        private Vector3 baseStartPos;
        private Vector3 baseEndPos;
    [Header("Paper Area")]
    public Collider paperCollider;   // Assign your paper object collider
        private float basePencilY;
        private Quaternion baseScaleRotation;
    
        private int currentStep = 0;
        private int activeSetCount = 0;
    [Header("Special Mode Events (NEW)")]
    public UnityEvent onAllLinesCompletedInSlide; // Fires when all lines in current slide/set are done
    
    private bool nextSetEnabled = false; // controls manual next set unlock
        private int completedLinesInSet = 0;
    
        private float activeStepGap = 0f;
        private bool activeUseStepGap = true;
        private bool activeMoveOnZAxis = false;
    
        private bool activeCameraFollow = false;
        private float activeCameraSmooth = 10f;
    
        void Start()
        {slideController = FindObjectOfType<SlideCameraController>();
            if (autoMarkButton != null)
                autoMarkButton.SetActive(false);
    
            if (useAutoArrange && slideSets.Count > 0)
            {
                SetupSet(0);
            }
            else
            {
                SetupSlide(currentSlideIndex);
            }
        }
        bool ArePointsOnPaper()
    {
        if (paperCollider == null || startPoint == null || endPoint == null)
            return false;
    
        Bounds bounds = paperCollider.bounds;
    
        bool startInside = bounds.Contains(startPoint.position);
        bool endInside   = bounds.Contains(endPoint.position);
    
        return startInside && endInside;
    }
    
        // 🔥 TURN ON: Next slide only on line drawn (NO events, NO next set)
        public void NextSlideOnLineDrawn()
        {
            nextSlideOnLineDrawMode = true;
            linesDrawnInSpecialMode = 0;
            autoMarkListenerAdded = false;
    
            if (autoMarkButton != null)
                autoMarkButton.SetActive(false);
    
            Debug.Log("NextSlideOnLineDrawn MODE ENABLED");
        }
    
        // 🔥 TURN OFF: Back to normal system
        public void OffNextSlideOnLineDrawn()
        {
            nextSlideOnLineDrawMode = false;
            linesDrawnInSpecialMode = 0;
            Debug.Log("NextSlideOnLineDrawn MODE DISABLED");
        }
    void SpawnSingleText(SlideSet set, int index)
    {
        if (set.textPrefab == null || set.targetCanvas == null)
            return;
    
        TextMeshProUGUI newText =
            Instantiate(set.textPrefab, set.targetCanvas.transform);
    
        newText.text = index == 0
            ? set.startingText
            : set.startingText + index.ToString();
    
        RectTransform rect = newText.GetComponent<RectTransform>();
    
        // 🔥 Change ONLY X using anchoredPosition
        Vector2 anchored = rect.anchoredPosition;
        anchored.x += set.textSpacing * index;
        rect.anchoredPosition = anchored;
    
        spawnedTexts.Add(newText);
    }
    void LateUpdate()
    {
        if (!activeCameraFollow)
            return;
    
        if (forceStopFollow)
            return;
    
        if (cameraTransform == null || activeFollowTarget == null)
            return;
    
        // 🚨 Wait until slide camera finishes movement
        if (slideController != null && !slideController.CameraAtTarget)
        {
            cameraVelocity = Vector3.zero;
            return;
        }
    
        Vector3 targetPos = activeFollowTarget.position;   // 🔥 FOLLOW SET TARGET
        targetPos.z = cameraTransform.position.z;
    
        cameraTransform.position = Vector3.SmoothDamp(
            cameraTransform.position,
            targetPos,
            ref cameraVelocity,
            0.08f
        );
    }
    public void StopMovement()
    {
        forceStopFollow = true;
        activeCameraFollow = false;
        cameraVelocity = Vector3.zero;
    
        Debug.Log("Camera follow stopped manually.");
    }
    void SnapCameraToFollowTarget()
    {
        if (!activeCameraFollow) return;
        if (cameraTransform == null || activeFollowTarget == null) return;
    
        Vector3 desiredPos = activeFollowTarget.position;
        desiredPos.z = cameraTransform.position.z;
    
        cameraTransform.position = desiredPos;
    }public void AutoMarkCurrentSet()
    {
        if (!useAutoArrange || slideSets.Count == 0)
            return;
    
        SlideSet set = slideSets[currentSetIndex];
    
        // 🔥 Start from actual completed count
        for (int i = completedLinesInSet; i < activeSetCount; i++)
        {
            currentStep = i;
            ApplyCalculatedStep();
    
            CreateLine();
            AddPoint(startPoint.position);
            AddPoint(endPoint.position);
    
            // 🔥 Spawn correct text in correct order
            SpawnSingleText(set, i);
    
            completedLinesInSet = i + 1;
    
            if (!nextSlideOnLineDrawMode)
            {
                set.onEachLineCompleted?.Invoke();
            }
            else
            {
                linesDrawnInSpecialMode = i + 1;
            }
        }
    
        currentStep = activeSetCount - 1;
        ApplyCalculatedStep();
    
        if (!nextSlideOnLineDrawMode)
        {
            set.onSetCompleted?.Invoke();
        }
    
        onAllLinesCompletedInSlide?.Invoke();
    
        if (autoMarkButton != null)
            autoMarkButton.SetActive(false);
    
        Debug.Log("AutoMark Completed Set: " + currentSetIndex);
    }
        public void ResetScalePos()
        {
            if (resetScaleTransform == null)
                return;
    
            highlight_Scale.SetActive(true);
            DisablePencilDrag();
    
            if (raycastscale != null)
                raycastscale.SetActive(true);
    
            scale.position = resetScaleTransform.position;
            scale.rotation = resetScaleTransform.rotation;
    
            lastProjected = 0f;
            dragging = false;
    
            if (startPoint != null && endPoint != null)
            {
                scaleDir = (endPoint.position - startPoint.position).normalized;
                scaleLength = Vector3.Distance(startPoint.position, endPoint.position);
            }
        }
        
        public void ResetScalePosnormal()
        {
            if (resetScaleTransform == null)
                return;
    
            highlight_Scale.SetActive(true);
            //DisablePencilDrag();
    
            if (raycastscale != null)
                raycastscale.SetActive(true);
    
            scale.position = resetScaleTransform.position;
            scale.rotation = resetScaleTransform.rotation;
    
            lastProjected = 0f;
            dragging = false;
    
            if (startPoint != null && endPoint != null)
            {
                scaleDir = (endPoint.position - startPoint.position).normalized;
                scaleLength = Vector3.Distance(startPoint.position, endPoint.position);
            }
        }
    
    
        public void EnablePencilDrag()
        {
            if (raycastscale != null)
                raycastscale.SetActive(false);
    
            allowDragging = true;
            lastProjected = 0f;
            dragging = false;
    
            if (startPoint != null && endPoint != null)
            {
                scaleDir = (endPoint.position - startPoint.position).normalized;
                scaleLength = Vector3.Distance(startPoint.position, endPoint.position);
            }
        }  public void EnablespeicalPencilDrag()
        {
         
    
            allowDragging = true;
            lastProjected = 0f;
            dragging = false;
    
            if (startPoint != null && endPoint != null)
            {
                scaleDir = (endPoint.position - startPoint.position).normalized;
                scaleLength = Vector3.Distance(startPoint.position, endPoint.position);
            }
        }
    
        public void DisablePencilDrag()
        {
            allowDragging = false;
            dragging = false;
        }
    
        public void NextSlide()
        {
            if (!useAutoArrange)
                return;
    
            SlideSet set = slideSets[currentSetIndex];
    
            if (currentStep < activeSetCount - 1)
            {
                currentStep++;
                ApplyCalculatedStep();
                //set.onEachLineCompleted?.Invoke();
                return;
            }
    
            set.onSetCompleted?.Invoke();
    
            currentSetIndex++;
    
            if (currentSetIndex >= slideSets.Count)
            {
                Debug.Log("All sets completed.");
                return;
            }
    
            SetupSet(currentSetIndex);
        }
    
        void SetupSlide(int index)
        {
            SlideData slide = slides[index];
    
            pencil.position = slide.pencilPos.position;
            pencil.rotation = slide.pencilPos.rotation;
    
            scale.position = slide.scalePos.position;
            scale.rotation = slide.scalePos.rotation;
    
            startPoint = slide.startPoint;
            endPoint = slide.endPoint;
    
            scaleDir = (endPoint.position - startPoint.position).normalized;
            scaleLength = Vector3.Distance(startPoint.position, endPoint.position);
        }
    void SetupSet(int setIndex)
    {
        SlideSet set = slideSets[setIndex];
    
        // 🔥 RESET SPECIAL MODE VALUES
        linesDrawnInSpecialMode = 0;
        autoMarkListenerAdded = false;
        nextSetEnabled = false;
    
        if (autoMarkButton != null)
            autoMarkButton.SetActive(false);
    
        SlideData refSlide = slides[set.startIndex];
    
        startPoint = refSlide.startPoint;
        endPoint = refSlide.endPoint;
    
        baseScalePos = refSlide.scalePos.position;
        basePencilPos = refSlide.pencilPos.position;
        baseStartPos = refSlide.startPoint.position;
        baseEndPos = refSlide.endPoint.position;
    
        basePencilY = refSlide.pencilPos.position.y;
        baseScaleRotation = refSlide.scalePos.rotation;
    
        activeStepGap = set.stepGap;
        activeUseStepGap = set.useStepGap;
        activeMoveOnZAxis = set.moveOnZAxis;
    
        activeCameraFollow = set.followCameraWhileDragging;
        activeCameraSmooth = set.cameraFollowSmooth;
    
        // 🔥 Assign per-set camera follow target
        activeFollowTarget = set.cameraFollowTarget;
    
        activeSetCount = set.count;
        currentStep = 0;
        completedLinesInSet = 0;
    
        scaleDir = (endPoint.position - startPoint.position).normalized;
        scaleLength = Vector3.Distance(startPoint.position, endPoint.position);
    SnapCameraToFollowTarget();
        ApplyCalculatedStep();
    
    }
        void ApplyCalculatedStep()
        {currentStepCompleted = false;
            float moveValue = activeUseStepGap ? activeStepGap * currentStep : 0f;
    
            if (activeMoveOnZAxis)
            {
                scale.position = new Vector3(baseScalePos.x, baseScalePos.y, baseScalePos.z + moveValue);
                pencil.position = new Vector3(basePencilPos.x, basePencilY, basePencilPos.z + moveValue);
            }
            else
            {
                scale.position = new Vector3(baseScalePos.x + moveValue, baseScalePos.y, baseScalePos.z);
                pencil.position = new Vector3(basePencilPos.x + moveValue, basePencilPos.y, basePencilPos.z);
            }
    
            scale.rotation = baseScaleRotation;
            dragging = false;
            lastProjected = 0f;
    
            scaleDir = (endPoint.position - startPoint.position).normalized;
            scaleLength = Vector3.Distance(startPoint.position, endPoint.position);
        }
        public void offonnextslideonliendrawnn()
    {
        EnablePencilDrag();
        nextSlideOnLineDrawMode = false;
        linesDrawnInSpecialMode = 0;
        autoMarkListenerAdded = false;
    
        if (autoMarkButton != null)
            autoMarkButton.SetActive(false);
    
        Debug.Log("Special Mode OFF - Back to Normal Mode");
    }
    
        void Update()
        {
            if (!allowDragging)
            {
                dragging = false;
                return;
            }
    
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (hit.collider.transform.root == pencil.root)
                    {
                        StartDrag();
                    }
                }
            }
    
            if (dragging && Input.GetMouseButton(0))
            {
                SlideOnScale();
            }
    
            if (Input.GetMouseButtonUp(0))
            {
                dragging = false;
            }
        }
    
        void StartDrag()
    {
        dragging = true;
        lastProjected = 0f;
    
        // 🔥 Enable camera follow when pencil touched
        activeCameraFollow = true;
        forceStopFollow = false;
    
        CreateLine();
    
        Vector3 offset = pencil.position - tip.position;
        pencil.position = startPoint.position + offset;
    
        AddPoint(tip.position);
    }
    
    void SlideOnScale()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(-Camera.main.transform.forward, startPoint.position);
    
        if (!plane.Raycast(ray, out float distance))
            return;
    
        Vector3 hitPoint = ray.GetPoint(distance);
    
        float projected = Vector3.Dot(hitPoint - startPoint.position, scaleDir);
        projected = Mathf.Clamp(projected, 0f, scaleLength);
    
        if (projected < lastProjected - 0.0001f)
            return;
    
        lastProjected = projected;
    
        Vector3 targetTipPos = startPoint.position + scaleDir * projected;
        Vector3 offset = pencil.position - tip.position;
        pencil.position = targetTipPos + offset;
    
        AddPoint(tip.position);
    
        // =====================================================
        // 🔥 LINE NOT COMPLETED YET
        // =====================================================
        if (projected < scaleLength - 0.001f)
            return;
    
        // 🔥 PREVENT DUPLICATE COMPLETION
        if (currentStepCompleted)
            return;
    
        currentStepCompleted = true;
    
        Vector3 endTipPos = endPoint.position;
        pencil.position = endTipPos + offset;
        dragging = false;
    
        SlideSet set = slideSets[currentSetIndex];
    
        // =====================================================
        // 🔥 SPECIAL MODE
        // =====================================================
        if (nextSlideOnLineDrawMode)
        {
            int spawnIndex = completedLinesInSet + linesDrawnInSpecialMode;
    
            SpawnSingleText(set, spawnIndex);
            linesDrawnInSpecialMode++;
    
            if (autoMarkButton != null &&
                linesDrawnInSpecialMode >= autoMarkUnlockLineCount)
            {
                autoMarkButton.SetActive(true);
    
                if (!autoMarkListenerAdded)
                {
                    Button btn = autoMarkButton.GetComponent<Button>();
                    if (btn != null)
                        btn.onClick.AddListener(AutoMarkCurrentSet);
    
                    autoMarkListenerAdded = true;
                }
            }
    
            if (linesDrawnInSpecialMode >= activeSetCount)
            {
                onAllLinesCompletedInSlide?.Invoke();
    
                if (autoMarkButton != null)
                    autoMarkButton.SetActive(false);
    
                if (!nextSetEnabled)
                    return;
            }
    
            if (currentStep < activeSetCount - 1)
            {
                currentStep++;
                ApplyCalculatedStep();
            }
    
            return;
        }
    
        // =====================================================
        // 🔥 NORMAL MODE
        // =====================================================
        if (!useAutoArrange || slideSets.Count == 0)
            return;
    
        if (completedLinesInSet >= activeSetCount)
            return;
    
        SpawnSingleText(set, completedLinesInSet);
        completedLinesInSet++;
    
        set.onEachLineCompleted?.Invoke();
        Debug.Log("calling normal mode");
    
        if (completedLinesInSet >= activeSetCount)
        {
            set.onSetCompleted?.Invoke();
    
            if (autoMarkButton != null)
                autoMarkButton.SetActive(false);
        }
    }
       void CreateLine()
    {
        GameObject obj = new GameObject("Line_Step_" + currentStep);
    
        // 🔥 Make it child of assigned parent
        if (lineParent != null)
            obj.transform.SetParent(lineParent);
    
        line = obj.AddComponent<LineRenderer>();
    
        line.material = lineMaterial;
        line.startWidth = 0.003f;
        line.endWidth = 0.003f;
        line.useWorldSpace = true;
    
        obj.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    
        points.Clear();
        line.positionCount = 0;
    }
    
        void AddPoint(Vector3 worldPoint)
        {
            Vector3 drawPoint = worldPoint;
            drawPoint.y = drawingSurface.position.y + surfaceOffset;
    
            if (points.Count == 0)
                points.Add(drawPoint);
            else if (points.Count == 1)
                points.Add(drawPoint);
            else
                points[1] = drawPoint;
    
            line.positionCount = points.Count;
            line.SetPositions(points.ToArray());
        }
    }
    
    
}