using UnityEngine;

public class UiManagerService : MonoBehaviour
{
    [SerializeField] private GameObject _fileBrowserPrefab;

    public GameObject ShowFileBrowser()
    {
        GameObject fb = Instantiate(_fileBrowserPrefab);
        Transform head = Camera.main.transform;
        fb.transform.position = head.position + head.forward * 0.35f;
        fb.transform.LookAt(head);
        return fb;
    }
}
