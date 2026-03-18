using UnityEngine;
using TMPro;

public class SlideNumberUI : MonoBehaviour
{
    [SerializeField] private TMP_Text text;

    private void OnEnable()
    {
        ExperienceEvents.OnSlideEntered += UpdateNumber;
    }

    private void OnDisable()
    {
        ExperienceEvents.OnSlideEntered -= UpdateNumber;
    }

    private void UpdateNumber(int number)
    {
        text.text = $"Slide {number}";
    }
}
