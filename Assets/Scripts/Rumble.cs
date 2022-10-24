using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public enum RumblePattern
{
    Constant,
    Pulse,
    Nothing
}

public class Rumble : MonoBehaviour
{
    private PlayerInput _playerInput;
    private RumblePattern activeRumbePattern;
    private float rumbleDurration;
    private float pulseDurration;
    private float lowA;
    private float lowStep;
    private float highA;
    private float highStep;
    private float rumbleStep;
    private bool isMotorActive = false;
    public void RumbleConstant(float low, float high, float durration)
    {
        activeRumbePattern = RumblePattern.Constant;
        lowA = low;
        highA = high;
        rumbleDurration = Time.time + durration;

    }

    public void RumblePulse(float low, float high, float burstTime, float durration)
    {
        activeRumbePattern = RumblePattern.Pulse;
        lowA = low;
        highA = high;
        rumbleStep = burstTime;
        pulseDurration = Time.time + burstTime;
        rumbleDurration = Time.time + durration;
        isMotorActive = true;
        var g = GetGamepad();
        g?.SetMotorSpeeds(lowA, highA);
    }


    public void StopRumble()
    {
        var gamepad = GetGamepad();
        if (gamepad != null)
        {
            gamepad.SetMotorSpeeds(0, 0);
        }
    }

    private void OnJumpEvent()
    {
        RumbleConstant(0.1f, 0.3f, 0.1f);
    }

    private void OnDashEvent(float time)
    {
        RumbleConstant(0.1f, 0.05f, time);
    }
    private void OnLandingEvent(float value)
    {
        if (value < 0.5) { value = 0; }
        if (value > 0.7) { value = 1; }
        RumblePulse(1f*value, 3f*value, 0.15f, 0.1f);
    }

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        Movement.JumpingSignal += OnJumpEvent;
        Movement.LandingSignalWithValue += OnLandingEvent;
        Movement.DashSignal += OnDashEvent;
    }

    private void Update()
    {
        if (Time.time > rumbleDurration)
        {
            StopRumble();
            activeRumbePattern = RumblePattern.Nothing;
            return;
        }
        var gamepad = GetGamepad();
        switch (activeRumbePattern)
        {
            case RumblePattern.Constant:
                gamepad?.SetMotorSpeeds(lowA, highA);
                break;

            case RumblePattern.Pulse:

                if (Time.time > pulseDurration)
                {
                    isMotorActive = !isMotorActive;
                    pulseDurration = Time.time + rumbleStep;
                    if (!isMotorActive)
                    {
                        gamepad?.SetMotorSpeeds(0, 0);
                    }
                    else
                    {
                        gamepad?.SetMotorSpeeds(lowA, highA);
                    }
                }

                break;
        }


        }

    private void OnDestroy()
    {
        StopRumble();
        Movement.DashSignal -= OnDashEvent;
        Movement.JumpingSignal -= OnJumpEvent;
        Movement.LandingSignalWithValue -= OnLandingEvent;
    }

    private Gamepad GetGamepad()
    {
        return Gamepad.all.FirstOrDefault(g => _playerInput.devices.Any(d => d.deviceId == g.deviceId));
    }
}
