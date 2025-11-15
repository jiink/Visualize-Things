using UnityEngine;

public class UiManagerService : MonoBehaviour
{
    [SerializeField] private GameObject _fileBrowserPrefab;

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
}
