using Oculus.Interaction;
using Oculus.Interaction.Input;
using System;
using UnityEngine;

public class RadialMenuController : MonoBehaviour
{
    [SerializeField]
    private GameObject _radialMenuPrefab;
    [SerializeField]
    private Transform _headPos;
    [SerializeField]
    private OVRHand _ovrHandLeft;
    [SerializeField]
    private OVRHand _ovrHandRight;
    [SerializeField]
    private PinchDetector _pinchDetector;
    [SerializeField]
    private RadialMenuDefinition _primaryRadialMenuDef;

    private GameObject _radialMenu;
    private OVRPlugin.Hand _activeHand;

    public enum RadialMenuKind
    {
        Primary,
        Secondary
    };
    
    

    void Start()
    {
        _pinchDetector.PinchEvent += OnPinch;
    }

    void Update()
    {

    }

    private void OnPinch(object sender, OVRPlugin.Hand hand, OVRHand.HandFinger finger, bool state)
    {
        if (state)
        {
            Debug.Log($"PINCH BEGIN!! {hand} {finger}");
            RadialMenuKind kind = finger switch
            {
                OVRHand.HandFinger.Middle => RadialMenuKind.Primary,
                OVRHand.HandFinger.Ring => RadialMenuKind.Secondary,
                _ => throw new Exception($"Invalid finger {finger}")
            };
            Transform handT = hand switch
            {
                OVRPlugin.Hand.HandLeft => _ovrHandLeft.PointerPose,
                OVRPlugin.Hand.HandRight => _ovrHandRight.PointerPose,
                _ => throw new Exception($"Invalid hand {hand}")
            };
            _activeHand = hand;
            ShowRadialMenu(kind, handT);
        } else
        {
            if (hand != _activeHand)
            {
                return;
            }
            Debug.Log($"PINCH END!! {hand} {finger}");
            HideRadialMenu();
        }
    }

    private void ShowRadialMenu(RadialMenuKind kind, Transform hand)
    {
        if (_radialMenu != null)
        {
            return;
        }
        Debug.Log("HERE!");
        _radialMenu = Instantiate(_radialMenuPrefab);
        _radialMenu.transform.position = hand.position;
        _radialMenu.transform.LookAt(_headPos);
        _radialMenu.GetComponent<RadialMenu>().Populate(_primaryRadialMenuDef);
    }

    private void HideRadialMenu()
    {
        if (_radialMenu != null)
        {
            GameObject.Destroy(_radialMenu);
        }
    }

    public void PrintSomething(string text)
    {
        Debug.Log(text);
    }
}
