using UnityEngine;

[CreateAssetMenu(menuName = "Evaluation/MCQ Question")]
public class MCQQuestionData : ScriptableObject
{
    [Header("Question Info")]
    [TextArea(2, 5)]
    public string questionText;

    [Header("Common")]
    public Sprite referenceImage;

    [TextArea(3, 8)]
    public string explanationText;

    // ---------------- MCQ / True-False ----------------
    public string[] options;
    public int correctOptionIndex;

    // ---------------- Drag & Drop ----------------
    public string correctDropId;  
    // Example: "EnginePart_A"

    // ---------------- Value / Animation Based ----------------
    public float correctValue;
    public float tolerance = 0.1f;
}
