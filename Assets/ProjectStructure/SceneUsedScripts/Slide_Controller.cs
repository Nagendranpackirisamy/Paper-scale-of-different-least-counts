using UnityEngine;

using UnityEngine.UI;

using TMPro;

using System.Collections;

using System.Collections.Generic;

using UnityEngine.Events;



namespace paperScaleOfDifferentLeastCounts

{

    public class SlideCameraController : MonoBehaviour

    {

        [System.Serializable]

        public class Step

        {

            [Header("Identification")]

            public int pageNumber;



            [Header("Camera")]

            public Transform cameraPoint;



            [Header("Events")]

            public UnityEvent onNextClicked;

            public UnityEvent onBackClicked;

            public UnityEvent onRepeatedNextClicked;



            [Header("Special UI Controls")]

            public bool enableNextOnStart = false;



            public bool showNumpad = false;

            public GameObject numpadObject;



            public bool showCalculator = false;

            public GameObject calculatorObject;

        }



        [Header("Camera")]

        public Transform cameraTransform;



        [Header("Camera Movement")]

        [Min(0.1f)]

        public float moveDuration = 1.5f;



        public PencilDraw pencilDraw;



        [SerializeField]

        private AnimationCurve positionCurve =

            AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);



        [SerializeField]

        private AnimationCurve rotationCurve =

            AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);



        [Header("Steps")]

        public List<Step> steps;



        // ✅ SAFE ACCESSORS FOR OTHER SCRIPTS

        public int CurrentStepIndex => currentIndex;



        public int CurrentPageNumber =>

            (steps != null && steps.Count > 0)

                ? steps[currentIndex].pageNumber

                : -1;



        [Header("Navigation Buttons")]

        public Button nextButton;

        public Button previousButton;



        [Header("Slide Counter UI")]

        public TextMeshProUGUI slideCounterText;



        [Header("Debug")]

        public bool enableCheatKey = true;



        [Header("Camera Event")]

        public UnityEvent OnCameraMoveStarted;



        int currentIndex;

        bool isMoving;



        public bool CameraAtTarget { get; private set; }



        bool[] slideCompleted;

        bool[] slideLocked;

        bool[] eventUsed;



        void Start()

        {

            slideCompleted = new bool[steps.Count];

            slideLocked = new bool[steps.Count];

            eventUsed = new bool[steps.Count];



            currentIndex = 0;



            cameraTransform.position = steps[0].cameraPoint.position;

            cameraTransform.rotation = steps[0].cameraPoint.rotation;



            ApplyStepSettings(0);



            nextButton.interactable = slideCompleted[0];

            UpdateBackButton();

            UpdateSlideCounterUI();



            CameraAtTarget = true;

        }



        void Update()

        {

            if (!enableCheatKey)

                return;



            if (Input.GetKeyDown(KeyCode.N))

                ForceNextSlide_Cheat();

        }



        void StopAllPencilDraws()

        {

            PencilDraw[] allPencils = FindObjectsOfType<PencilDraw>();



            foreach (PencilDraw pd in allPencils)

            {

                pd.StopMovement();

            }

        }



        void ForceNextSlide_Cheat()

        {

            if (currentIndex >= steps.Count - 1)

                return;



            StopAllCoroutines();



            isMoving = false;

            CameraAtTarget = true;



            slideCompleted[currentIndex] = true;

            slideLocked[currentIndex] = true;



            nextButton.interactable = true;



            StartCoroutine(MoveTo(currentIndex + 1));

        }



        public void EnableNextButton()

        {

            slideCompleted[currentIndex] = true;

            slideLocked[currentIndex] = true;



            nextButton.interactable = true;



            UpdateBackButton();

        }



        public void Next()

        {

            if (isMoving)

                return;



            if (!slideCompleted[currentIndex])

                return;



            if (currentIndex >= steps.Count - 1)

                return;



            StopAllPencilDraws();



            if (!eventUsed[currentIndex])

            {

                steps[currentIndex].onNextClicked?.Invoke();

                eventUsed[currentIndex] = true;

            }



            steps[currentIndex].onRepeatedNextClicked?.Invoke();



            StartCoroutine(MoveTo(currentIndex + 1));

        }



        public void Previous()

        {

            if (isMoving)

                return;



            if (currentIndex <= 0)

                return;



            steps[currentIndex].onBackClicked?.Invoke();



            StopAllPencilDraws();



            StartCoroutine(MoveTo(currentIndex - 1));

        }



        bool ShouldAnimate(int fromIndex, int toIndex)

        {

            if (toIndex > fromIndex)

            {

                if (!slideLocked[toIndex])

                    return true;

            }



            return false;

        }



        IEnumerator MoveTo(int targetIndex)

        {

            if (isMoving)

                yield break;



            isMoving = true;

            CameraAtTarget = false;



            OnCameraMoveStarted?.Invoke();



            Step to = steps[targetIndex];



            bool animate = ShouldAnimate(currentIndex, targetIndex);



            Vector3 startPos = cameraTransform.position;

            Quaternion startRot = cameraTransform.rotation;



            Vector3 endPos = to.cameraPoint.position;

            Quaternion endRot = to.cameraPoint.rotation;



            currentIndex = targetIndex;



            ApplyStepSettings(currentIndex);



            nextButton.interactable = slideCompleted[currentIndex];

            UpdateBackButton();

            UpdateSlideCounterUI();



            if (!animate)

            {

                cameraTransform.position = endPos;

                cameraTransform.rotation = endRot;



                CameraAtTarget = true;

                isMoving = false;

                yield break;

            }



            float elapsed = 0f;



            while (elapsed < moveDuration)

            {

                elapsed += Time.deltaTime;



                float t = Mathf.Clamp01(elapsed / moveDuration);



                float posT = positionCurve.Evaluate(t);

                float rotT = rotationCurve.Evaluate(t);



                cameraTransform.position = Vector3.Lerp(startPos, endPos, posT);

                cameraTransform.rotation = Quaternion.Slerp(startRot, endRot, rotT);



                yield return null;

            }



            cameraTransform.position = endPos;

            cameraTransform.rotation = endRot;



            CameraAtTarget = true;

            isMoving = false;

        }



        void ApplyStepSettings(int index)

        {

            Step s = steps[index];



            if (slideLocked[index])

            {

                slideCompleted[index] = true;

                DisableSpecialUI(s);

                return;

            }



            if (s.enableNextOnStart)

                slideCompleted[index] = true;



            if (s.numpadObject != null)

                s.numpadObject.SetActive(s.showNumpad);



            if (s.calculatorObject != null)

                s.calculatorObject.SetActive(s.showCalculator);

        }



        void DisableSpecialUI(Step s)

        {

            if (s.numpadObject != null)

                s.numpadObject.SetActive(false);



            if (s.calculatorObject != null)

                s.calculatorObject.SetActive(false);

        }



        void UpdateBackButton()

        {

            if (currentIndex <= 0)

            {

                previousButton.interactable = false;

                return;

            }



            previousButton.interactable = slideCompleted[currentIndex];

        }



        void UpdateSlideCounterUI()

        {

            if (slideCounterText == null)

                return;



            slideCounterText.text =

                $"{steps[currentIndex].pageNumber} / {steps.Count}";

        }

    }

}