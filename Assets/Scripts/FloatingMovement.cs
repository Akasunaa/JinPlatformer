using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingMovement : MonoBehaviour
{
    [SerializeField] private float _amplitude;
    [SerializeField] private float _loopDuration;
    private bool _startNewLoop;
    private Vector3 _initialPosition;

    private void Awake()
    {
        _startNewLoop = true;
        _initialPosition = transform.position;
    }

    private void Update()
    {
        if (_startNewLoop)
        {
            StartCoroutine(Floating());
        }
    }

    
    private IEnumerator Floating()
    {
        _startNewLoop = false;

        var timer = 0f;
        float interpolFactor = 1f;
        Vector3 lowPosition = new Vector3(_initialPosition.x, _initialPosition.y - _amplitude, 0);
        Vector3 highPosition = new Vector3(_initialPosition.x, _initialPosition.y + _amplitude , 0);

        while (timer < _loopDuration)
        {
            interpolFactor = Mathf.Abs(timer / (_loopDuration * 0.5f) - 1f);
            transform.position = Vector3.Lerp(lowPosition, highPosition, interpolFactor);
            timer += Time.deltaTime;
            yield return null;
        }

        transform.position = highPosition;

        _startNewLoop = true;

        yield return null;
    }


}
