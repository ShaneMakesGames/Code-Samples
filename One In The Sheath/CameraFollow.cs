using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // The hierarchy goes Movement Parent/Screenshake Parent/Camera
    // This makes it easy to handle smooth camera tracking & screen shake simultaneously
    public Transform movementParent;
    public Transform screenShakeParent;

    public bool screenShakeActive;

    public Transform target;

    public float smoothSpeed = 0.125f;
    public Vector3 offset;
    public Vector3 currentVelocity;

    void FixedUpdate()
    {
        if (!GameManager.singleton.battleActive) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.SmoothDamp(movementParent.localPosition, desiredPosition, ref currentVelocity, smoothSpeed);

        movementParent.localPosition = smoothedPosition;
    }

    public void TryScreenShake()
    {
        if (screenShakeActive) return;

        StartCoroutine(ScreenShakeCoroutine(new Vector2(0.5f, 0.5f), 0.05f, 0.1f, startDelayTime: 0.1f));
    }

    private IEnumerator ScreenShakeCoroutine(Vector2 shakeVector, float shakeDuration, float totalTimeToShakeFor, float startDelayTime = 0f)
    {
        screenShakeActive = true;

        yield return new WaitForSeconds(startDelayTime);

        float totalTimePassed = 0;
        float currentMoveTimePassed = 0;

        Vector3 startPos = screenShakeParent.position;
        Vector3 posAtMoveStart = startPos;
        int sign = 1;
        Vector3 targetPos = new Vector3(startPos.x + (shakeVector.x * sign), startPos.y + (shakeVector.y * sign), startPos.z);

        while (totalTimePassed < totalTimeToShakeFor)
        {
            if (currentMoveTimePassed < shakeDuration)
            {
                screenShakeParent.position = Vector3.Lerp(posAtMoveStart, targetPos, currentMoveTimePassed / shakeDuration);
                currentMoveTimePassed += Time.deltaTime;
            }
            else
            {
                posAtMoveStart = screenShakeParent.position;
                sign = -sign;
                targetPos = new Vector3(startPos.x + (shakeVector.x * sign), startPos.y + (shakeVector.y * sign), startPos.z);
                currentMoveTimePassed = 0;
            }

            totalTimePassed += Time.deltaTime;
            yield return null;
        }

        posAtMoveStart = screenShakeParent.position;
        totalTimePassed = 0;

        while (totalTimePassed < shakeDuration)
        {
            screenShakeParent.position = Vector3.Lerp(posAtMoveStart, startPos, totalTimePassed / shakeDuration);
            totalTimePassed += Time.deltaTime;
            yield return null;
        }

        screenShakeParent.position = startPos;
        screenShakeActive = false;
    }
}