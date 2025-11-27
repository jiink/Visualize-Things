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
    [SerializeField] private OVRHand _ovrHandLeft;
    [SerializeField] private OVRHand _ovrHandRight;
    [SerializeField] private OVRCameraRig _ovrCamRig;
    [SerializeField] private PinchDetector _pinchDetector;
    [SerializeField] private RadialMenuDefinition _primaryRadialMenuDef;
    [SerializeField] private RadialMenuDefinition _contextRadialMenuDef;
    private GameObject _currentRadialMenu;

    // global pos
    public Vector3 LeftPointerPos => _ovrCamRig.trackingSpace.TransformPoint(_ovrHandLeft.PointerPose.position);
    public Vector3 RightPointerPos => _ovrCamRig.trackingSpace.TransformPoint(_ovrHandRight.PointerPose.position);
    public Quaternion LeftPointerRot => _ovrHandLeft.PointerPose.rotation;
    public Quaternion RightPointerRot => _ovrHandRight.PointerPose.rotation;

    void OnValidate()
    {
        if (_ovrCamRig == null)
        {
            _ovrCamRig = FindFirstObjectByType<OVRCameraRig>();
        }
    }

    private void Start()
    {
        _pinchDetector.PinchEvent += OnPinch;
    }

    public GameObject ShowFileBrowser(System.Action<string, Vector3> onFileSelectedCallback)
    {
        GameObject fb = Instantiate(_fileBrowserPrefab);
        Transform head = Camera.main.transform;
        fb.transform.position = head.position + head.forward * 0.35f;
        fb.transform.LookAt(head);
        fb.transform.Rotate(0, 180, 0);
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
                Destroy(contextObj);
                Debug.Log("destroyed model");
                break;
            default:
                Debug.Log($"Unimplemented selection {id}");
                break;
        }
    }

}
