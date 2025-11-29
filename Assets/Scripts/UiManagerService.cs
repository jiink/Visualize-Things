using Meta.XR;
using Oculus.Interaction;
using System;
using UnityEngine;

public class UiManagerService : MonoBehaviour
{
    private enum RadialMenuKind
    {
        Primary,
        Context
    };

    [SerializeField] private GameObject _fileBrowserPrefab;
    [SerializeField] private GameObject _pcConnectionPromptPrefab;
    [SerializeField] private GameObject _radialMenuPrefab;
    [SerializeField] private GameObject _sunEditorPrefab;
    [SerializeField] private GameObject _materialEditorPrefab;
    [SerializeField] private GameObject _panoScannerPrefab;
    [SerializeField] private OVRHand _ovrHandLeft;
    [SerializeField] private OVRHand _ovrHandRight;
    [SerializeField] private OVRCameraRig _ovrCamRig;
    [SerializeField] private PinchDetector _pinchDetector;
    [SerializeField] private RadialMenuDefinition _primaryRadialMenuDef;
    [SerializeField] private RadialMenuDefinition _contextRadialMenuDef;
    [SerializeField] private Light _directionalLight;
    [SerializeField] private PassthroughCameraAccess _cameraAccess;
    private GameObject _currentRadialMenu;
    private bool _panoScannerExists = false;

    // global pos
    public Vector3 LeftPointerPos => _ovrCamRig.trackingSpace.TransformPoint(_ovrHandLeft.PointerPose.position);
    public Vector3 RightPointerPos => _ovrCamRig.trackingSpace.TransformPoint(_ovrHandRight.PointerPose.position);
    public Quaternion LeftPointerRot => _ovrHandLeft.PointerPose.rotation;
    public Quaternion RightPointerRot => _ovrHandRight.PointerPose.rotation;

    public Pose GetInFaceSpawnPose(bool flipped = false)
    {
        Transform head = Camera.main.transform;
        Vector3 pos = head.position + head.forward * 0.35f;
        Quaternion rot = Quaternion.LookRotation(
            (flipped ? -1.0f : 1.0f) * head.forward);
        return new Pose(pos, rot);
    }

    void OnValidate()
    {
        if (_ovrCamRig == null)
        {
            _ovrCamRig = FindFirstObjectByType<OVRCameraRig>();
        }
        if (_cameraAccess == null)
        {
            _cameraAccess = FindFirstObjectByType<PassthroughCameraAccess>();
        }
    }

    private void Start()
    {
        _pinchDetector.PinchEvent += OnPinch;
    }

    public GameObject ShowFileBrowser(System.Action<string, Vector3> onFileSelectedCallback)
    {
        Pose p = GetInFaceSpawnPose(false);
        GameObject fb = Instantiate(_fileBrowserPrefab, p.position, p.rotation);
        FileBrowser fbc = fb.GetComponent<FileBrowser>();
        fbc.FileOpen += (string path, Vector3 p) =>
        {
            onFileSelectedCallback(path, p);
            Destroy(fb);
        };
        return fb;
    }

    internal void ShowConnectionPrompt(Transform spawnTransform, string ip, string hostname)
    {
        GameObject pr = Instantiate(_pcConnectionPromptPrefab, spawnTransform);
        PcConnectionPrompt prompt = pr.GetComponent<PcConnectionPrompt>();
        pr.transform.Rotate(0, 180, 0);
        pr.transform.position += pr.transform.forward * -0.1f;
        prompt.Populate(ip, hostname);
    }

    internal void ShowContextMenu(GameObject ob, Vector3 pos, Action onDestruction)
    {
        ShowRadialMenu(RadialMenuKind.Context, pos, ob, onDestruction);
    }

    private void OnPinch(object sender, OVRPlugin.Hand hand, OVRHand.HandFinger finger, bool state, Vector3 pointerPose)
    {
        if (state)
        {
            //Debug.Log($"PINCH BEGIN!! {hand} {finger}");
            Vector3 slidBackPose = pointerPose + (Camera.main.transform.forward * 0.1f);
            ShowRadialMenu(RadialMenuKind.Primary, slidBackPose, null, null);
        }
    }

    private void ShowRadialMenu(RadialMenuKind kind, Vector3 pos, GameObject contextObj, Action onDestruction)
    {
        if (contextObj == null && kind == RadialMenuKind.Context)
        {
            Debug.LogError("Context object is null even though this is a context menu.");
            return;
        }
        if (contextObj != null && kind != RadialMenuKind.Context)
        {
            Debug.LogWarning("Providing context object to a non-context menu.");
        }
        if (_currentRadialMenu != null)
        {
            HideRadialMenu();
        }
        _currentRadialMenu = Instantiate(_radialMenuPrefab);
        _currentRadialMenu.transform.position = pos;
        _currentRadialMenu.transform.LookAt(Camera.main.transform);
        RadialMenu rm = _currentRadialMenu.GetComponent<RadialMenu>();
        RadialMenuDefinition def = kind switch
        {
            RadialMenuKind.Primary => _primaryRadialMenuDef,
            RadialMenuKind.Context => _contextRadialMenuDef,
            _ => throw new Exception($"Unhandled radial menu kind {kind}"),
        };
        rm.Populate(def, contextObj);
        rm.SelectionEvent += OnRadialMenuSelection;
        rm.DestructionEvent += (_, _) => { onDestruction?.Invoke(); };
        if (contextObj != null)
        {
            if (!contextObj.TryGetComponent<SelectableModel>(out var selectableModel))
            {
                Debug.LogError("Has context object, but no selectablemodel component");
                return;
            }
            ColliderVisualizer visCh = selectableModel.GetVisualizerChild();
            if (visCh != null)
            {
                visCh.MState = ColliderVisualizer.State.Selected;
                rm.DestructionEvent += (_, _) => {
                    if (visCh != null) {
                        visCh.MState = ColliderVisualizer.State.Proximity;
                    }
                };
            }
            else
            {
                Debug.LogError("Has selectablemodel component, but no visualizer child");
            }
        }
    }

    public void ShowMaterialEditor(GameObject contextObj)
    {
        var p = GetInFaceSpawnPose();
        var m = Instantiate(_materialEditorPrefab, p.position, p.rotation)
            .GetComponent<MaterialEditorMenu>();
        m.InspectedObject = contextObj;
    }

    private void HideRadialMenu()
    {
        if (_currentRadialMenu != null)
        {
            Destroy(_currentRadialMenu);
        }
    }

    private void OnRadialMenuSelection(object sender, RadialButtonData.RmSelection id, GameObject contextObj)
    {
        switch (id)
        {
            case RadialButtonData.RmSelection.LoadModel:
                Services.Get<UiManagerService>().ShowFileBrowser(
                    (path, pos) => Services.Get<ModelLoadingService>().ImportModelAsync(path, pos).ConfigureAwait(false)
                );
                break;
            case RadialButtonData.RmSelection.DeleteModel:
                if (contextObj == null)
                {
                    Debug.LogError($"context object required for {id}");
                    break;
                }
                Destroy(contextObj);
                Debug.Log("destroyed model");
                break;
            case RadialButtonData.RmSelection.ChangeOcclusion:
                Services.Get<OcclusionService>().ToggleOcclusion();
                break;
            case RadialButtonData.RmSelection.PlaceOnSurface:
                if (contextObj == null)
                {
                    Debug.LogError($"context object required for {id}");
                    break;
                }
                Services.Get<SurfacePlacementService>().Begin(contextObj);
                break;
            case RadialButtonData.RmSelection.EditLight:
                {
                    Pose p = GetInFaceSpawnPose();
                    var s = Instantiate(_sunEditorPrefab, p.position, p.rotation).GetComponent<SunEditor>();
                    s.Populate(_directionalLight);
                    break;
                }
            case RadialButtonData.RmSelection.EditMaterials:
                if (contextObj == null)
                {
                    Debug.LogError($"context object required for {id}");
                    break;
                }
                ShowMaterialEditor(contextObj);
                break;
            case RadialButtonData.RmSelection.SetReflections:
                {
                    if (_panoScannerExists)
                    {
                        Debug.LogError("A panorama scanner already exists");
                        break;
                    }
                    Pose p = GetInFaceSpawnPose();
                    var go = Instantiate(_panoScannerPrefab, p.position, p.rotation);
                    go.transform.SetParent(Camera.main.transform);
                    var scanner = go.GetComponent<PanoScanner>();
                    _panoScannerExists = true;
                    scanner.FinishEvent += (_, _) => { _panoScannerExists = false; };
                    scanner.Begin(_cameraAccess);
                    break;
                }
            default:
                Debug.Log($"Unimplemented selection {id}");
                break;
        }
    }

    internal void SetAllLoadedModelCollisions(bool v)
    {
        GameObject[] selectableObjects = GameObject.FindGameObjectsWithTag("SelectableObject");
        foreach (GameObject obj in selectableObjects)
        {
            Collider collider = obj.GetComponent<Collider>();

            // 4. Check if a Collider component exists before trying to access it.
            if (collider != null)
            {
                // 5. Set the enabled state of the collider.
                collider.enabled = v;
                // Optional: Log the action for debugging
                // Debug.Log($"Collider on {obj.name} set to enabled: {v}");
            }
            else
            {
                // Optional: Log if a tagged object is missing a collider
                Debug.LogWarning($"GameObject '{obj.name}' with tag is missing a Collider component.");
            }
        }
    }
}
