using UnityEngine;

public delegate void HandPinchEventHandler(object sender, OVRPlugin.Hand hand, OVRHand.HandFinger finger, bool state, Vector3 pointerPose);

public class PinchDetector : MonoBehaviour
{
    [SerializeField]
    private OVRHand _ovrHandLeft;
    [SerializeField]
    private OVRHand _ovrHandRight;
    [SerializeField] private OVRCameraRig _ovrCamRig;

    public event HandPinchEventHandler PinchEvent;

    void OnValidate()
    {
        if (_ovrCamRig == null)
        {
            _ovrCamRig = FindFirstObjectByType<OVRCameraRig>();
        }
    }

    void Start()
    {
    }

    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.Start, OVRInput.Controller.LHand))
        {
            PinchEvent.Invoke(this,
                OVRPlugin.Hand.HandLeft,
                OVRHand.HandFinger.Index,
                true,
                _ovrCamRig.trackingSpace.TransformPoint(_ovrHandLeft.PointerPose.position));
        } else if (OVRInput.GetDown(OVRInput.Button.Start, OVRInput.Controller.RHand))
        {
            PinchEvent.Invoke(this, OVRPlugin.Hand.HandRight, OVRHand.HandFinger.Index, true, _ovrHandRight.PointerPose.position);
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
