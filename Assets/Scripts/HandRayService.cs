using Oculus.Interaction;
using System;
using UnityEngine;

public class HandRayService : MonoBehaviour
{
    [SerializeField] private RayInteractor _handRayInteractorLeft;
    [SerializeField] private RayInteractor _handRayInteractorRight;
    [SerializeField] private GameObject _progressIndicatorPrefab;
    [SerializeField] private GameObject _laserLinePrefab;
    private GameObject _laserLineL;
    private GameObject _laserLineR;
    private LayerMask _laserLineLayer;
    private readonly float _requiredPinchTimeS = 0.3f;
    private GameObject _hoveredObject = null;
    private bool _isHoldingPinch = false;
    private OVRHand.Hand _pinchHoldingHand = OVRHand.Hand.HandLeft;
    private float _holdStartTime = 0.0f;
    private RadialProgressIndicator _radialProgressIndicator;
    private bool _selectionRayEnabled = true;
    public LayerMask LaserLineLayer
    {
        get => _laserLineLayer;
        set
        {
            _laserLineLayer = value;
        }
    }
    public bool SelectionRayEnabled {
        get => _selectionRayEnabled;
        set
        {
            if (value == _selectionRayEnabled) { return; }
            _selectionRayEnabled = value;
            if (!_selectionRayEnabled)
            {
                _laserLineL.SetActive(false);
                _laserLineR.SetActive(false);
            }
        }
    }
    public Vector3 LaserHitPointL;
    public bool LaserHitPointLValid => _laserLineL.activeSelf;
    public Vector3 LaserHitPointR;
    public bool LaserHitPointRValid => _laserLineR.activeSelf;

    private void Start()
    {
        Services.Get<ModelLoadingService>().ModelSpawnedEvent += OnNewModelSpawned;
        _laserLineL = Instantiate(_laserLinePrefab);
        _laserLineR = Instantiate(_laserLinePrefab);
        _laserLineLayer = LayerMask.GetMask("Models");
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
        //Debug.Log($"object unhovered.");
    }

    private void OnModelHovered(GameObject ob, PointerEvent pe)
    {
        //Debug.Log("object hovered");
    }

    private OVRHand.Hand IdToHand(int identifier)
    {
        if (identifier == _handRayInteractorLeft.Identifier)
        {
            return OVRHand.Hand.HandLeft;
        }
        else if (identifier == _handRayInteractorRight.Identifier)
        {
            return OVRHand.Hand.HandRight;
        }
        else
        {
            Debug.LogError($"Couldn't match identifier {identifier}");
            return OVRHand.Hand.None;
        }
    }

    // when your pinch stops
    private void OnModelUnselected(GameObject ob, PointerEvent pe)
    {
        if (!SelectionRayEnabled) { return; }
        Debug.Log("<color=blue>object unselected</color>");
        if (_radialProgressIndicator != null)
        {
            Destroy(_radialProgressIndicator.gameObject);
            _radialProgressIndicator = null;
        }
        _isHoldingPinch = false;
    }

    // when your pinch starts
    private void OnModelSelected(GameObject ob, PointerEvent pe)
    {
        if (!SelectionRayEnabled) { return; }
        Debug.Log("<color=green>object selected!</color>");
        _holdStartTime = Time.time;
        _pinchHoldingHand = IdToHand(pe.Identifier);
        Quaternion rot = _pinchHoldingHand == OVRHand.Hand.HandLeft ?
            Services.Get<UiManagerService>().LeftPointerRot :
            Services.Get<UiManagerService>().RightPointerRot;
        Vector3 pos = _pinchHoldingHand == OVRHand.Hand.HandLeft ?
            Services.Get<UiManagerService>().LeftPointerPos :
            Services.Get<UiManagerService>().RightPointerPos;
        pos += rot * Vector3.forward * 0.1f;
        _radialProgressIndicator = Instantiate(_progressIndicatorPrefab, pos, rot)
            .GetComponent<RadialProgressIndicator>();
        _isHoldingPinch = true;
        _hoveredObject = ob;
    }

    private void ShowContextMenu(GameObject currentHoveredObject, OVRHand.Hand pinchHoldingHand)
    {
        Vector3 pointerP = pinchHoldingHand switch
        {
            OVRHand.Hand.HandLeft => Services.Get<UiManagerService>().LeftPointerPos,
            OVRHand.Hand.HandRight => Services.Get<UiManagerService>().RightPointerPos,
            _ => throw new Exception("No hand?")
        };
        SelectionRayEnabled = false;
        Services.Get<UiManagerService>().ShowContextMenu(
            currentHoveredObject,
            pointerP + (Camera.main.transform.forward * 0.1f),
            () => { SelectionRayEnabled = true; }
        );
    }

    private void UpdateLaser(bool left)
    {
        Vector3 ptrPos = left ? 
            Services.Get<UiManagerService>().LeftPointerPos :
            Services.Get<UiManagerService>().RightPointerPos;
        Quaternion ptrRot = left ? 
            Services.Get<UiManagerService>().LeftPointerRot :
            Services.Get<UiManagerService>().RightPointerRot;
        GameObject laser = left ? _laserLineL : _laserLineR;
        if (Physics.Raycast(
            ptrPos,
            ptrRot * Vector3.forward,
            out RaycastHit hitInfo,
            10.0f,
            _laserLineLayer))
        {
            if (left)
            {
                LaserHitPointL = hitInfo.point;
            }
            else
            {
                LaserHitPointR = hitInfo.point;
            }
            laser.SetActive(true);
            laser.transform.localScale = new Vector3(
                laser.transform.localScale.x,
                laser.transform.localScale.y,
                hitInfo.distance
                );
            laser.transform.position = ptrPos;
            laser.transform.rotation = ptrRot;
            if (hitInfo.collider.gameObject.TryGetComponent<SelectableModel>(out var smCmp))
            {
                if (smCmp.GetVisualizerChild() != null)
                {
                    smCmp.GetVisualizerChild().TransitionTo(ColliderVisualizer.State.Hover);
                } else
                {
                    Debug.LogError("updatelaser couldnt get vis child");
                }
            } else
            {
                
            }
        }
        else
        {
            laser.SetActive(false);
        }
    }

    private void Update()
    {
        if (SelectionRayEnabled)
        {
            UpdateLaser(false);
            UpdateLaser(true);
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
                    if (_radialProgressIndicator != null ? _radialProgressIndicator.gameObject : null != null)
                    {
                        Destroy(_radialProgressIndicator.gameObject);
                    }
                }
            }
        }
    }
}
