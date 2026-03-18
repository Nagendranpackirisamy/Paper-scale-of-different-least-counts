using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class NumpadController : MonoBehaviour
{
    // -------- PERSISTENT MEMORY --------

    private static Dictionary<int, bool> solvedSlides
        = new Dictionary<int, bool>();

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
    public paperScaleOfDifferentLeastCounts. SlideCameraController slideManager;

    private string currentInput = "";
    private bool hasDecimal = false;
    private int maxLength = 4;

    private bool isLocked = false;

    void OnEnable()
    {
        // ?? Check if already solved before
        if (solvedSlides.ContainsKey(slidePageNumber) &&
            solvedSlides[slidePageNumber])
        {
            isLocked = true;
            inputField.readOnly = true;
            slideManager.EnableNextButton();
        }
        else
        {
            Clear();
            isLocked = false;
            inputField.readOnly = false;
        }

        if (feedbackImage != null)
            feedbackImage.gameObject.SetActive(false);
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
            // ?? Permanently lock
            isLocked = true;
            inputField.readOnly = true;

            solvedSlides[slidePageNumber] = true;

            slideManager.EnableNextButton();
        }
    }

    IEnumerator ShowFeedback(bool correct)
    {
        feedbackImage.gameObject.SetActive(true);
        feedbackImage.sprite = correct ? correctSprite : wrongSprite;

        yield return new WaitForSeconds(feedbackDuration);

        feedbackImage.gameObject.SetActive(false);
    }
}