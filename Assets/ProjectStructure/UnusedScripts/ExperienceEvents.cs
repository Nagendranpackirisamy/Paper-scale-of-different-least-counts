using System;
using UnityEngine;

public static class ExperienceEvents
{
    // Slide Events
    public static Action<int> OnSlideEntered;
    public static Action<int> OnSlideExited;

    // SubState Events
    public static Action<string> OnSubStateChanged;

    // Camera Requests
    public static Action<Transform, AnimationCurve, float> OnCameraMoveRequested;
}
