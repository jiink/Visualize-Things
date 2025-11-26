using Oculus.Interaction;
using System;
using UnityEngine;

public class HandRayService : MonoBehaviour
{
    [SerializeField] private RayInteractor _handRayInteractorLeft;
    [SerializeField] private RayInteractor _handRayInteractorRight;
    [SerializeField] private RayInteractor _progressIndicatorPrefab;
    private readonly float _requiredPinchTimeS = 1.0f;
    private GameObject _hoveredObject = null;
    private bool _isHoldingPinch = false;
    private OVRHand.Hand _pinchHoldingHand = OVRHand.Hand.HandLeft;
    private float _holdStartTime = 0.0f;
    private RadialProgressIndicator _radialProgressIndicator;
    private void Start()
    {
        Services.Get<ModelLoadingService>().ModelSpawnedEvent += OnNewModelSpawned;
    }

    private void OnNewModelSpawned(object sender, ModelSpawnedEventArgs e)
    {
        GameObject ob = e.SpawnedModel;
        if (!ob.TryGetComponent<PointableUnityEventWrapper>(out var evWrap))
        {
            Debug.LogError("no event wrapper on new model");
            return;
        }
        evWrap.WhenSelect.AddListener((pe) => OnModelSelected(ob, pe));
        evWrap.WhenUnselect.AddListener((pe) => OnModelUnselected(ob, pe));
        evWrap.WhenHover.AddListener((pe) => OnModelHovered(ob, pe));
        evWrap.WhenUnhover.AddListener((pe) => OnModelUnhovered(ob, pe));
        Debug.Log("new model subscribed stuff");
    }

    // could happen twice in a row
    private void OnModelUnhovered(GameObject ob, PointerEvent pe)
    {
        Debug.Log($"object unhovered.");
    }

    private void OnModelHovered(GameObject ob, PointerEvent pe)
    {
        Debug.Log("object hovered");
    }

    // when your pinch stops
    private void OnModelUnselected(GameObject ob, PointerEvent pe)
    {
        Debug.Log("<color=blue>object unselected</color>");
        if (pe.Identifier == _handRayInteractorLeft.Identifier)
        {
            _pinchHoldingHand = OVRHand.Hand.HandLeft;
        }
        else if (pe.Identifier == _handRayInteractorRight.Identifier)
        {
            _pinchHoldingHand = OVRHand.Hand.HandRight;
        }
        else
        {
            Debug.LogError($"Couldn't match pointerevent identifier {pe.Identifier}");
            _pinchHoldingHand = OVRHand.Hand.None;
        }
        Destroy(_radialProgressIndicator.gameObject);
    }

    // when your pinch starts
    private void OnModelSelected(GameObject ob, PointerEvent pe)
    {
        Debug.Log("<color=green>object selected!</color>");
        _holdStartTime = Time.time;
    }

    private void ShowContextMenu(GameObject currentHoveredObject, OVRHand.Hand pinchHoldingHand)
    {
        Vector3 pointerP = pinchHoldingHand switch
        {
            OVRHand.Hand.HandLeft => Services.Get<UiManagerService>().LeftPointerPos,
            OVRHand.Hand.HandRight => Services.Get<UiManagerService>().RightPointerPos,
            _ => throw new Exception("No hand?")
        };
        Services.Get<UiManagerService>().ShowContextMenu(
            currentHoveredObject,
            pointerP + (Camera.main.transform.forward * 0.1f)
        );
    }

    private void Update()
    {
        if (_isHoldingPinch)
        {
            float elapsedTime = Time.time - _holdStartTime;
            float progress = elapsedTime / _requiredPinchTimeS;
            if (_radialProgressIndicator != null)
            {
                _radialProgressIndicator.Progress = progress;
            }
            if (elapsedTime >= _requiredPinchTimeS)
            {
                ShowContextMenu(_hoveredObject, _pinchHoldingHand);
                _isHoldingPinch = false;
                _hoveredObject = null;
                Destroy(_radialProgressIndicator.gameObject);
            }
        }
    }
}
