using UnityEngine;
using System.Collections.Generic;

public class ExperienceManager : MonoBehaviour
{
    [SerializeField] private List<SlideState> slides;

    private int currentIndex = -1;
    private SlideState currentSlide;

    public int CurrentSlideNumber => currentIndex + 1;
    public int TotalSlides => slides.Count;

    private void Start()
    {
        InitializeSlides();
        GoToSlide(0);
    }

    private void InitializeSlides()
    {
        for (int i = 0; i < slides.Count; i++)
        {
            slides[i].Setup(this, i + 1);
            slides[i].gameObject.SetActive(false);
        }
    }

    public void GoToSlide(int index)
    {
        if (index < 0 || index >= slides.Count)
            return;

        if (currentSlide != null)
        {
            ExperienceEvents.OnSlideExited?.Invoke(currentSlide.SlideNumber);
            currentSlide.Exit();
        }

        currentIndex = index;
        currentSlide = slides[currentIndex];

        currentSlide.Enter();
        ExperienceEvents.OnSlideEntered?.Invoke(currentSlide.SlideNumber);
    }

    public void NextSlide() => GoToSlide(currentIndex + 1);
    public void PreviousSlide() => GoToSlide(currentIndex - 1);
}
