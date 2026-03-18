using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;


namespace paperScaleOfDifferentLeastCounts
{
    public class AutoSpawnDialer : MonoBehaviour
    {
        [Header("Container Prefab (Image with TMP_InputField child)")]
        public GameObject inputContainerPrefab;
    
        private Vector3 firstTextBasePosition;
        private bool firstTextCaptured = false;
    
        [Header("Spawn Canvas")]
        public Transform spawnCanvas;
    
        [Header("Correct Text Prefab (TMP)")]
        public TextMeshProUGUI correctTextPrefab;
    
        [Header("Spawn Settings")]
        public int totalFields = 31;
        public float gap = 150f;
    
        [Header("Camera Step Movement")]
        public Transform cameraTransform;
        public float cameraSmooth = 8f;
        public float cameraStepX = -0.0203f;
    
        [Header("Auto Fill")]
        public Button autoFillButton;
        public int maxAttempts = 3;
    
        [Header("Delays")]
        public float replaceDelay = 0.5f;     // before fade
        public float textSpawnDelay = 0.5f;   // after container disabled
    
        [Header("Result Image Fade")]
        public float resultFadeSpeed = 5f;
    
        [Header("Events")]
        public UnityEvent OnEachCorrect;
        public UnityEvent OnAllCorrect;
        public UnityEvent OnWrongAnswer;
    
        List<GameObject> containers = new List<GameObject>();
        List<TMP_InputField> fields = new List<TMP_InputField>();
    
        int activeIndex = 0;
        int attemptCounter = 0;
        bool finished = false;
        bool waitingForReplace = false;
    
        void Start()
        {
            SpawnSingleField(activeIndex);
    
            if (autoFillButton != null)
            {
                autoFillButton.gameObject.SetActive(false);
                autoFillButton.onClick.AddListener(AutoFillAll);
            }
        }
    
        // ================= SPAWN ONE FIELD =================
    
        void SpawnSingleField(int index)
        {
            if (index >= totalFields) return;
    
            Vector3 basePos = inputContainerPrefab.transform.localPosition;
            Quaternion baseRot = inputContainerPrefab.transform.localRotation;
    
            GameObject container = Instantiate(inputContainerPrefab, spawnCanvas);
    
            Vector3 pos = basePos;
            pos.x += gap * index;
    
            container.transform.localPosition = pos;
            container.transform.localRotation = baseRot;
    
            TMP_InputField field = container.GetComponentInChildren<TMP_InputField>();
    
            field.text = "";
            field.interactable = true;
    
            containers.Add(container);
            fields.Add(field);
        }
    
        // ================= DIGIT BUTTON =================
    
        public void OnDigitPressed(string digit)
        {
            if (finished || waitingForReplace || activeIndex >= fields.Count) return;
    
            fields[activeIndex].text += digit;
        }
    
        // ================= SUBMIT BUTTON =================
    
        public void OnSubmit()
        {
            if (finished || waitingForReplace || activeIndex >= fields.Count) return;
    
            string enteredText = fields[activeIndex].text;
    
            if (!int.TryParse(enteredText, out int val))
            {
                fields[activeIndex].text = "";
                return;
            }
    
            attemptCounter++;
    
            if (val != activeIndex)
            {
                fields[activeIndex].text = "";
                OnWrongAnswer?.Invoke();
                CheckAutoFillCondition();
                return;
            }
    
            StartCoroutine(ReplaceAfterDelay(enteredText));
        }
    
        // ================= DELAY REPLACE =================
    
        IEnumerator ReplaceAfterDelay(string value)
        {
            waitingForReplace = true;
    
            // 1️⃣ Initial delay
            yield return new WaitForSeconds(replaceDelay);
    
            // 2️⃣ Fade result image
            yield return StartCoroutine(FadeInResultImage(containers[activeIndex]));
    
            // 3️⃣ Disable container
            containers[activeIndex].SetActive(false);
    
            // 4️⃣ Wait before showing text
            yield return new WaitForSeconds(textSpawnDelay);
    
            // 5️⃣ Spawn correct text
            SpawnCorrectText(activeIndex, value);
    
            OnEachCorrect?.Invoke();
    
            activeIndex++;
    
            CheckAutoFillCondition();
    
            if (activeIndex < totalFields)
            {
                SpawnSingleField(activeIndex);
                MoveCameraStep();
            }
            else
            {
                FinishPuzzle();
            }
    
            waitingForReplace = false;
        }
    
        // ================= FADE RESULT IMAGE =================
    
        IEnumerator FadeInResultImage(GameObject container)
        {
            Transform result = container.transform.Find("Resultimage"); // match your exact name
    
            if (result == null) yield break;
    
            Image img = result.GetComponent<Image>();
            if (img == null) yield break;
    
            Color c = img.color;
            c.a = 0f;
            img.color = c;
    
            while (c.a < 1f)
            {
                c.a += Time.deltaTime * resultFadeSpeed;
                img.color = c;
                yield return null;
            }
    
            c.a = 1f;
            img.color = c;
        }
    
        // ================= SPAWN CORRECT TEXT =================
    
        void SpawnCorrectText(int index, string value)
        {
            TextMeshProUGUI newText = Instantiate(correctTextPrefab, spawnCanvas);
            newText.text = value;
    
            RectTransform textRect = newText.GetComponent<RectTransform>();
    
            if (!firstTextCaptured)
            {
                firstTextBasePosition = textRect.position;
                firstTextCaptured = true;
            }
            else
            {
                Vector3 newPos = firstTextBasePosition;
                newPos.x += cameraStepX * index;
                textRect.position = newPos;
            }
        }
    
        // ================= CAMERA MOVE =================
    
        void MoveCameraStep()
        {
            if (cameraTransform == null) return;
    
            StopCoroutine(nameof(SmoothStepMove));
            StartCoroutine(SmoothStepMove());
        }
    
        IEnumerator SmoothStepMove()
        {
            Vector3 start = cameraTransform.position;
            Vector3 end = start;
            end.x += cameraStepX;
    
            while (Mathf.Abs(cameraTransform.position.x - end.x) > 0.0001f)
            {
                Vector3 pos = cameraTransform.position;
                pos.x = Mathf.Lerp(pos.x, end.x, Time.deltaTime * cameraSmooth);
                cameraTransform.position = pos;
                yield return null;
            }
    
            cameraTransform.position = end;
        }
    
        // ================= AUTO FILL =================
    
        void CheckAutoFillCondition()
        {
            if (attemptCounter >= maxAttempts && autoFillButton != null)
                autoFillButton.gameObject.SetActive(true);
        }
    
       void AutoFillAll()
    {
        if (finished) return;
    
        // 🔥 Make sure ALL containers exist first
        for (int i = containers.Count; i < totalFields; i++)
        {
            SpawnSingleField(i);
        }
    
        // 🔥 Disable every container safely
        for (int i = 0; i < containers.Count; i++)
        {
            if (containers[i] != null)
                containers[i].SetActive(false);
        }
    
        // 🔥 Spawn all remaining correct texts
        for (int i = activeIndex; i < totalFields; i++)
        {
            SpawnCorrectText(i, i.ToString());
        }
    
        activeIndex = totalFields;
    
        FinishPuzzle();
    }
        // ================= FINISH =================
    
        void FinishPuzzle()
        {
            if (finished) return;
    
            finished = true;
    
            if (autoFillButton != null)
                autoFillButton.gameObject.SetActive(false);
    
            OnAllCorrect?.Invoke();
        }
    }
    
}