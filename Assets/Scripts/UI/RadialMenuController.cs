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

    private void OnPinch(object sender, OVRPlugin.Hand hand, OVRHand.HandFinger finger, bool state, Transform pointerPose)
    {
        if (state)
        {
            Debug.Log($"PINCH BEGIN!! {hand} {finger}");
            RadialMenuKind kind = finger switch
            {
                OVRHand.HandFinger.Index => RadialMenuKind.Primary,
                OVRHand.HandFinger.Middle => RadialMenuKind.Primary,
                OVRHand.HandFinger.Ring => RadialMenuKind.Secondary,
                _ => throw new Exception($"Invalid finger {finger}")
            };
            _activeHand = hand;
            ShowRadialMenu(kind, pointerPose);
        }
    }

    private void ShowRadialMenu(RadialMenuKind kind, Transform hand)
    {
        if (_radialMenu != null)
        {
            HideRadialMenu();
        }
        _radialMenu = Instantiate(_radialMenuPrefab);
        _radialMenu.transform.position = hand.position;
        _radialMenu.transform.LookAt(_headPos);
        // back off a bit so the hand is clearly in front of the menu and not inside/behind it
        _radialMenu.transform.Translate(0, 0, -0.1f, _radialMenu.transform);
        RadialMenu rm = _radialMenu.GetComponent<RadialMenu>();
        rm.Populate(_primaryRadialMenuDef);
        rm.SelectionEvent += OnMenuSelection;
    }
   
    private void HideRadialMenu()
    {
        if (_radialMenu != null)
        {
            Destroy(_radialMenu);
        }
    }

    private void OnMenuSelection(object sender, RadialButtonData.RmSelection id)
    {
        switch (id)
        {
            case RadialButtonData.RmSelection.LoadModel:
                Services.Get<UiManagerService>().ShowFileBrowser();
                break;
            default:
                Debug.Log($"Unimplemented selection {id}");
                break;
        }
    }


    public void PrintSomething(string text)
    {
        Debug.Log(text);
    }
}
