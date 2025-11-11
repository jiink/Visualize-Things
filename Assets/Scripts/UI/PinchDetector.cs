using System;
using UnityEngine;

public delegate void HandPinchEventHandler(object sender, OVRPlugin.Hand hand, OVRHand.HandFinger finger, bool state);

public class PinchDetector : MonoBehaviour
{

    public event HandPinchEventHandler PinchEvent;

    void Start()
    {
    }

    void Update()
    {
    }
}
