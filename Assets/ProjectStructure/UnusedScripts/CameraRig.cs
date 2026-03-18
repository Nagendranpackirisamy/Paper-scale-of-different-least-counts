using UnityEngine;
using System.Collections;

public class CameraRig : MonoBehaviour
{
    private Coroutine currentRoutine;

    private void OnEnable()
    {
        ExperienceEvents.OnCameraMoveRequested += MoveTo;
    }

    private void OnDisable()
    {
        ExperienceEvents.OnCameraMoveRequested -= MoveTo;
    }

    public void MoveTo(Transform target, AnimationCurve curve, float duration)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(MoveRoutine(target, curve, duration));
    }

    private IEnumerator MoveRoutine(Transform target, AnimationCurve curve, float duration)
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        Vector3 endPos = target.position;
        Quaternion endRot = target.rotation;

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = curve.Evaluate(time / duration);

            transform.position = Vector3.LerpUnclamped(startPos, endPos, t);
            transform.rotation = Quaternion.SlerpUnclamped(startRot, endRot, t);

            yield return null;
        }

        transform.SetPositionAndRotation(endPos, endRot);
    }
}
