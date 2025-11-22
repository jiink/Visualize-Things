using UnityEngine;

public delegate void HandPinchEventHandler(object sender, OVRPlugin.Hand hand, OVRHand.HandFinger finger, bool state, Transform pointerPose);

public class PinchDetector : MonoBehaviour
{
    [SerializeField]
    private OVRHand _ovrHandLeft;
    [SerializeField]
    private OVRHand _ovrHandRight;

    public event HandPinchEventHandler PinchEvent;

    void Start()
    {
    }

    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.Start, OVRInput.Controller.LHand))
        {
            PinchEvent.Invoke(this, OVRPlugin.Hand.HandLeft, OVRHand.HandFinger.Index, true, _ovrHandLeft.PointerPose);
        } else if (OVRInput.GetDown(OVRInput.Button.Start, OVRInput.Controller.RHand))
        {
            PinchEvent.Invoke(this, OVRPlugin.Hand.HandRight, OVRHand.HandFinger.Index, true, _ovrHandRight.PointerPose);
        }
    }

    public void LeftMiddlePinch(bool state)
    {
        //PinchEvent.Invoke(this, OVRPlugin.Hand.HandLeft, OVRHand.HandFinger.Middle, state, _ovrHandLeft.PointerPose);
    }

    public void LeftRingPinch(bool state)
    {
        //PinchEvent.Invoke(this, OVRPlugin.Hand.HandLeft, OVRHand.HandFinger.Ring, state, _ovrHandLeft.PointerPose);
    }

    public void RightMiddlePinch(bool state)
    {
        //PinchEvent.Invoke(this, OVRPlugin.Hand.HandRight, OVRHand.HandFinger.Middle, state, _ovrHandRight.PointerPose);
    }

    public void RightRingPinch(bool state)
    {
        //PinchEvent.Invoke(this, OVRPlugin.Hand.HandRight, OVRHand.HandFinger.Ring, state, _ovrHandRight.PointerPose);
    }
}
