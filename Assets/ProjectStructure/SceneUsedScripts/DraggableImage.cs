using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

namespace paperScaleOfDifferentLeastCounts
{
    [RequireComponent(typeof(CanvasGroup))]
    public class DraggableImage : MonoBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("References")]
        public DropSlot targetSlot;
        public Canvas canvas;
        public Camera mainCamera;

        [Header("Visual")]
        public Image imageToDeactivate;

        [Header("Raycast")]
        public LayerMask slotLayer;

        [Header("Drag Movement")]
        public float returnSpeed = 15f;

        [Header("Drag Scale")]
        public float dragScaleMultiplier = 1.3f;
        public float scaleSpeed = 15f;

        [Header("Camera Focus")]
        public Transform cameraFocusPoint;
        public Transform cameraReturnPoint;
        public float cameraMoveDuration = 0.4f;

        RectTransform rect;
        CanvasGroup canvasGroup;

        Vector2 startPos;
        Vector2 pointerOffset;

        Vector3 initialScale;

        Coroutine returnCoroutine;
        Coroutine cameraRoutine;

        void Awake()
        {
            rect = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();

            if (!canvas) canvas = GetComponentInParent<Canvas>();
            if (!mainCamera) mainCamera = Camera.main;

            startPos = rect.anchoredPosition;
            initialScale = rect.localScale;

            if (imageToDeactivate == null)
                imageToDeactivate = GetComponent<Image>();
        }

        // ─────────────────────────
        // BEGIN DRAG
        // ─────────────────────────
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (returnCoroutine != null)
                StopCoroutine(returnCoroutine);

            canvasGroup.blocksRaycasts = false;
            rect.SetAsLastSibling();

            // Instantly enlarge
            rect.localScale = initialScale * dragScaleMultiplier;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                eventData.position,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                out Vector2 localMousePos
            );

            pointerOffset = rect.anchoredPosition - localMousePos;

            targetSlot?.ShowHighlight(true);

            if (cameraFocusPoint != null)
                MoveCamera(cameraFocusPoint.position, cameraFocusPoint.rotation);
        }

        public void OnDrag(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                eventData.position,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                out Vector2 localMousePos
            );

            rect.anchoredPosition = localMousePos + pointerOffset;
        }

        // ─────────────────────────
        // END DRAG
        // ─────────────────────────
        public void OnEndDrag(PointerEventData eventData)
        {
            canvasGroup.blocksRaycasts = true;

            Ray ray = mainCamera.ScreenPointToRay(eventData.position);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, slotLayer))
            {
                DropSlot slot = hit.collider.GetComponentInParent<DropSlot>();

                if (slot != null && slot == targetSlot)
                {
                    // Successful drop
                    slot.Snap();

                    // Restore scale
                    rect.localScale = initialScale;

                    // Hide image
                    if (imageToDeactivate != null)
                        imageToDeactivate.enabled = false;

                    if (cameraReturnPoint != null)
                        MoveCamera(cameraReturnPoint.position, cameraReturnPoint.rotation);

                    return;
                }
            }

            // Wrong drop
            targetSlot?.ShowHighlight(false);

            returnCoroutine = StartCoroutine(ReturnBack());

            if (cameraReturnPoint != null)
                MoveCamera(cameraReturnPoint.position, cameraReturnPoint.rotation);
        }

        // ─────────────────────────
        // RETURN BACK
        // ─────────────────────────
        IEnumerator ReturnBack()
        {
            while (Vector2.Distance(rect.anchoredPosition, startPos) > 0.5f)
            {
                rect.anchoredPosition = Vector2.Lerp(
                    rect.anchoredPosition,
                    startPos,
                    Time.deltaTime * returnSpeed
                );

                rect.localScale = Vector3.Lerp(
                    rect.localScale,
                    initialScale,
                    Time.deltaTime * scaleSpeed
                );

                yield return null;
            }

            rect.anchoredPosition = startPos;
            rect.localScale = initialScale;

            returnCoroutine = null;
        }

        // ─────────────────────────
        // CAMERA MOVE
        // ─────────────────────────
        void MoveCamera(Vector3 targetPos, Quaternion targetRot)
        {
            if (cameraRoutine != null)
                StopCoroutine(cameraRoutine);

            cameraRoutine = StartCoroutine(CameraLerp(targetPos, targetRot));
        }

        IEnumerator CameraLerp(Vector3 targetPos, Quaternion targetRot)
        {
            float t = 0f;

            Vector3 startPos = mainCamera.transform.position;
            Quaternion startRot = mainCamera.transform.rotation;

            while (t < 1f)
            {
                t += Time.deltaTime / cameraMoveDuration;

                mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, t);
                mainCamera.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

                yield return null;
            }

            mainCamera.transform.position = targetPos;
            mainCamera.transform.rotation = targetRot;
        }

        // ───────── For slide change ─────────
        public void SetTargetSlot(DropSlot newSlot)
        {
            targetSlot = newSlot;

            if (imageToDeactivate != null)
                imageToDeactivate.enabled = true;

            rect.anchoredPosition = startPos;
            rect.localScale = initialScale;
        }
    }
}