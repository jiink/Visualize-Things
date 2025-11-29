using Meta.XR;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;

public class PanoScanner : MonoBehaviour
{
    private const string HEADSET_CAMERA_PERMISSION = "horizonos.permission.HEADSET_CAMERA";
    private PassthroughCameraAccess _cameraAccess;
    [SerializeField] private GameObject _photoTemplate;
    [SerializeField] private Transform _photoBall;

    public event EventHandler FinishEvent;

    public void Begin(PassthroughCameraAccess camAccess)
    {
        _cameraAccess = camAccess;
        if (_cameraAccess == null)
        {
            Debug.LogError("Cam access is null");
            End();
            return;
        }
        if (!OVRPermissionsRequester.IsPermissionGranted(OVRPermissionsRequester.Permission.PassthroughCameraAccess))
        {
            Debug.LogError("I don't have permission for PassthroughCameraAccess");
            if (!Permission.HasUserAuthorizedPermission(HEADSET_CAMERA_PERMISSION))
            {
                Debug.Log("Requesting camera permission");
                Permission.RequestUserPermission(HEADSET_CAMERA_PERMISSION);
                if (!Permission.HasUserAuthorizedPermission(HEADSET_CAMERA_PERMISSION))
                {
                    Debug.LogError("Got denied permission");
                    End();
                    return;
                }
            }
            else
            {
                Debug.LogError("Weird, PassthroughCameraAccess isn't granted, but HEADSET_CAMERA is?");
                End();
                return;
            }
        }
        if (!_cameraAccess.enabled)
        {
            Debug.LogError("Cam access isn't enabled");
            End();
            return;
        }
        StartCoroutine(DoPhotoLoop());
    }
    
    public void End()
    {
        Debug.Log("PanoScanner ending");
        StopCoroutine(DoPhotoLoop());
        FinishEvent?.Invoke(this, EventArgs.Empty);
        Destroy(gameObject);
    }

    private void DoPhoto()
    {
        if (!_cameraAccess.IsPlaying)
        {
            Debug.LogWarning("Cam isn't playing yet");
            return;
        }
        Debug.Log("Snip....");
        Texture2D tex = GetPhotoTex();
        if (tex == null)
        {
            Debug.LogError("photo texture is null...");
            return;
        }
        var pt = Instantiate(_photoTemplate, _photoBall);
        pt.transform.localPosition = Vector3.zero;
        pt.transform.Translate(pt.transform.forward * 1.0f, Space.Self);
        var rawImg = pt.GetComponentInChildren<RawImage>();
        if (rawImg == null)
        {
            Debug.LogError("couldn't find rawimage in photo template");
            return;
        }
        rawImg.texture = tex;
    }

    private IEnumerator DoPhotoLoop()
    {
        const float period = 2.0f;
        while (true)
        {
            DoPhoto();
            yield return new WaitForSeconds(period);
        }
    }

    public Texture2D GetPhotoTex()
    {
        if (!_cameraAccess.IsPlaying)
        {
            Debug.LogError("cam isnt playing");
            return null;
        }
        var size = _cameraAccess.CurrentResolution;
        Texture2D snapshot = new(size.x, size.y, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp
        };
        var pixels = _cameraAccess.GetColors();
        snapshot.LoadRawTextureData(pixels);
        snapshot.Apply();
        return snapshot;
    }
}
