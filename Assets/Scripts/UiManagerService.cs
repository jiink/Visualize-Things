using UnityEngine;

public class UiManagerService : MonoBehaviour
{
    [SerializeField] private GameObject _fileBrowserPrefab;
    [SerializeField] GameObject _pcConnectionPromptPrefab;
    [SerializeField] private OVRHand _ovrHandLeft;
    [SerializeField] private OVRHand _ovrHandRight;
    [SerializeField] private OVRCameraRig _ovrCamRig;

    // global pos
    public Vector3 LeftPointerPos => _ovrCamRig.trackingSpace.TransformPoint(_ovrHandLeft.PointerPose.position);
    public Vector3 RightPointerPos => _ovrCamRig.trackingSpace.TransformPoint(_ovrHandRight.PointerPose.position);

    void OnValidate()
    {
        if (_ovrCamRig == null)
        {
            _ovrCamRig = FindFirstObjectByType<OVRCameraRig>();
        }
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
}
