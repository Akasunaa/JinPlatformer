using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    [SerializeField, Range(0.05f, 1f)] private float _shakeDuration;
    [SerializeField, Range(0.05f, 2f)] private float _shakeMagnitude;
    [SerializeField, Range(0,1)] private float _fallSpeedThreshold;
    [SerializeField, Range(10, 120)] private float _shakeFrequency;

    private Vector3 _initialCameraPose;
    private bool _isShaking;

    private void Awake()
    {
        _initialCameraPose = transform.localPosition;

        Movement.LandingSignalWithValue += OnBigFallingSignal;
    }

    private void OnBigFallingSignal(float fallSpeedRate)
    {
        if(fallSpeedRate >= _fallSpeedThreshold && !_isShaking)
        {
            StartCoroutine(Shake());
        }

        
    }

    private IEnumerator Shake()
    {
        _isShaking = true;
        var timer = 0f;
        var lastImpulseDate = Time.time;
        var shakeAmount = 1f;
        Vector2 randomDirection;

        while(timer < _shakeDuration)
        {
            if(Time.time > lastImpulseDate + 1 / _shakeFrequency)
            {
                lastImpulseDate = Time.time;
                randomDirection = Random.insideUnitCircle;
                transform.localPosition = new Vector3(_initialCameraPose.x + randomDirection.x,_initialCameraPose.y + randomDirection.y, transform.localPosition.z);
            }
            
            timer += Time.deltaTime;
            shakeAmount = timer / _shakeDuration;
            yield return null;
        }
        transform.localPosition = _initialCameraPose;

        _isShaking = false;
    }

    private void OnDestroy()
    {
        Movement.LandingSignalWithValue -= OnBigFallingSignal;
    }


}
