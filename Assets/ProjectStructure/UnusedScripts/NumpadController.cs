using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class NumpadController : MonoBehaviour
{
    // -------- PERSISTENT MEMORY --------

    private static Dictionary<int, bool> solvedSlides = new Dictionary<int, bool>();

    [Header("Slide Binding")]
    public int slidePageNumber;

    [Header("Input Field")]
    public TMP_InputField inputField;

    [Header("Answer Settings")]
    public float correctValue = 10f;
    public float tolerance = 0.1f;

    [Header("Feedback UI")]
    public Image feedbackImage;
    public Sprite correctSprite;
    public Sprite wrongSprite;
    public float feedbackDuration = 1.5f;

    [Header("Slide Manager")]
    public paperScaleOfDifferentLeastCounts.SlideCameraController slideManager;

    [Header("Auto Fill")]
    public Button autoFillButton;
    public float autoFillVisibleTime = 5f;

    [Header("Events")]
    public UnityEvent OnCorrectAnswer;
    public UnityEvent OnWrongAnswer;
    public UnityEvent OnAutoFillShown;
    public UnityEvent OnAutoFillUsed;

    private string currentInput = "";
    private bool hasDecimal = false;
    private int maxLength = 4;

    private bool isLocked = false;
    private int wrongAttempts = 0;

    private Coroutine autoFillRoutine;

    void OnEnable()
    {
        // Check if already solved before
        if (solvedSlides.ContainsKey(slidePageNumber) &&
            solvedSlides[slidePageNumber])
        {
            isLocked = true;
            inputField.readOnly = true;
            slideManager.EnableNextButton();
        }
        else
        {
            currentInput = "";
            hasDecimal = false;
            wrongAttempts = 0;

            isLocked = false;
            inputField.readOnly = false;
            UpdateField();
        }

        if (feedbackImage != null)
            feedbackImage.gameObject.SetActive(false);

        if (autoFillButton != null)
            autoFillButton.gameObject.SetActive(false);
    }

    // -------- DIGITS --------

    public void AddDigit(string digit)
    {
        if (isLocked) return;

        if (currentInput.Length >= maxLength)
            return;

        currentInput += digit;
        UpdateField();
    }

    public void AddDecimal()
    {
        if (isLocked) return;

        if (hasDecimal || currentInput.Length == 0)
            return;

        currentInput += ".";
        hasDecimal = true;
        UpdateField();
    }

    public void Clear()
    {
        if (isLocked) return;

        currentInput = "";
        hasDecimal = false;
        UpdateField();
    }

    void UpdateField()
    {
        inputField.text = currentInput;
    }

    // -------- CHECK --------

    public void Check()
    {
        if (isLocked) return;

        if (!float.TryParse(currentInput, out float value))
            return;

        bool correct = Mathf.Abs(value - correctValue) <= tolerance;

        StopAllCoroutines();
        StartCoroutine(ShowFeedback(correct));

        if (correct)
        {
            CompleteCorrectAnswer();
        }
        else
        {
            OnWrongAnswer?.Invoke();

            wrongAttempts++;

            if (wrongAttempts >= 3 && autoFillButton != null)
            {
                if (autoFillRoutine != null)
                    StopCoroutine(autoFillRoutine);

                autoFillRoutine = StartCoroutine(ShowAutoFillButton());
            }
        }
    }

    // -------- AUTO FILL --------

    IEnumerator ShowAutoFillButton()
    {
        autoFillButton.gameObject.SetActive(true);

        OnAutoFillShown?.Invoke();

        yield return new WaitForSeconds(autoFillVisibleTime);

        autoFillButton.gameObject.SetActive(false);
    }

    public void AutoFillAnswer()
    {
        if (isLocked) return;

        if (autoFillRoutine != null)
            StopCoroutine(autoFillRoutine);

        autoFillButton.gameObject.SetActive(false);

        currentInput = correctValue.ToString();
        hasDecimal = currentInput.Contains(".");
        UpdateField();

        OnAutoFillUsed?.Invoke();

        StopAllCoroutines();
        StartCoroutine(ShowFeedback(true));

        CompleteCorrectAnswer();
    }

    // -------- CORRECT ANSWER --------

    void CompleteCorrectAnswer()
    {
        isLocked = true;
        inputField.readOnly = true;

        solvedSlides[slidePageNumber] = true;

        slideManager.EnableNextButton();

        OnCorrectAnswer?.Invoke();
    }

    // -------- FEEDBACK --------

    IEnumerator ShowFeedback(bool correct)
    {
        feedbackImage.gameObject.SetActive(true);
        feedbackImage.sprite = correct ? correctSprite : wrongSprite;

        yield return new WaitForSeconds(feedbackDuration);

        feedbackImage.gameObject.SetActive(false);
    }
}