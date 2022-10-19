using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
    private SpriteRenderer _playerSpriteRdr;
    private float _defaultHorizontalScale;
    private float _defaultVerticalScale;

    [Header("Jump Animation")]
    [SerializeField] private float _jumpingAnimTime;
    [SerializeField] private float _horizontalShorten;
    [SerializeField] private float _verticalStretch;

    [Header("Landing Animation")]
    [SerializeField] private float _landingAnimTime;
    [SerializeField] private float _horizontalStrech;
    [SerializeField] private float _verticalShorten;

    private void Awake()
    {
        _playerSpriteRdr = GetComponent<SpriteRenderer>();
        _defaultHorizontalScale = transform.localScale.x;
        _defaultVerticalScale = transform.localScale.y;

        Movement.JumpingSignal += OnJumpEvent;
        Movement.LandingSignal += OnLandingEvent;
        
    }



    private void OnJumpEvent()
    {
        StartCoroutine(StrechAnimation(_horizontalShorten, _verticalStretch, _jumpingAnimTime));
    }

    private void OnLandingEvent()
    {
        Debug.Log("Licorne patate");
        StartCoroutine(StrechAnimation(_horizontalStrech, _verticalShorten, _landingAnimTime));
    }


    private IEnumerator StrechAnimation(float hozirontalTargetScale, float verticalTargetScale, float animDuration)
    {
        var animTimer = 0f;
        float interpolFactor = 1f;
        Vector3 defaultScale = new Vector3(_defaultHorizontalScale, _defaultVerticalScale, 1);
        Vector3 strechScale = new Vector3(hozirontalTargetScale, verticalTargetScale, 1);

        while(animTimer < animDuration)
        {
            interpolFactor = Mathf.Abs(animTimer / (animDuration * 0.5f) - 1f);
            transform.localScale = Vector3.Lerp(strechScale, defaultScale, interpolFactor);
            animTimer += Time.deltaTime;
            yield return null;
        }

        transform.localScale = defaultScale;
        
        yield return null;
    }

    private void OnDestroy()
    {
        Movement.JumpingSignal -= OnJumpEvent;
        Movement.LandingSignal -= OnLandingEvent;
    }
}
