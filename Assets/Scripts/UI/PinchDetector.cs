using System;
using UnityEngine;

public delegate void HandPinchEventHandler(object sender, OVRPlugin.Hand hand, OVRHand.HandFinger finger, bool state);

public class PinchDetector : MonoBehaviour
{
    [SerializeField]
    private OVRHand _ovrHandLeft;
    [SerializeField]
    private OVRHand _ovrHandRight;

    private bool _wasLeftRingPinching = false;
    private bool _wasLeftMiddlePinching = false;
    private bool _wasRightRingPinching = false;
    private bool _wasRightMiddlePinching = false;

    public event HandPinchEventHandler PinchEvent;

    void Start()
    {
    }

    private void CheckPinchState(OVRHand hand, OVRHand.HandFinger finger, ref bool wasPinching)
    {
        if (!hand.IsDataValid || !hand.IsDataHighConfidence)
        {
            return;
        }
        bool isPinching = hand.GetFingerPinchStrength(finger) > 0.77f;
        if (isPinching && !wasPinching)
        {
            PinchEvent?.Invoke(this, hand.GetHand(), finger, true);
        }
        else if (!isPinching && wasPinching)
        {
            PinchEvent?.Invoke(this, hand.GetHand(), finger, false);
        }
        wasPinching = isPinching;
    }

    void Update()
    {
        CheckPinchState(_ovrHandLeft, OVRHand.HandFinger.Ring, ref _wasLeftRingPinching);
        CheckPinchState(_ovrHandLeft, OVRHand.HandFinger.Middle, ref _wasLeftMiddlePinching);
        CheckPinchState(_ovrHandRight, OVRHand.HandFinger.Ring, ref _wasRightRingPinching);
        CheckPinchState(_ovrHandRight, OVRHand.HandFinger.Middle, ref _wasRightMiddlePinching);
    }
}
