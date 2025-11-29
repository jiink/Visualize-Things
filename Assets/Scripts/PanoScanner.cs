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
    [SerializeField] private CaptureSphere _captureSphere;

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
        Vector3 directionFromCam = (_photoBall.position - Camera.main.transform.position).normalized;
        Vector3 localDirection = _photoBall.InverseTransformDirection(directionFromCam);
        pt.transform.localPosition = localDirection * 1.0f;
        pt.transform.forward = directionFromCam;
        float photoWidth = GetFrustumWidthAtDistance(_cameraAccess, 1.0f);
        float photoScaleFactor = photoWidth / pt.transform.localScale.x;
        pt.transform.localScale = new Vector3(
            photoWidth, 
            pt.transform.localScale.y * photoScaleFactor,
            1.0f);

        var renderer = pt.GetComponentInChildren<Renderer>();
        if (renderer == null)
        {
            Debug.LogError("couldn't find Renderer in photo template");
            return;
        }
        renderer.material.mainTexture = tex;
        _captureSphere.CaptureAndApply();
    }

    private IEnumerator DoPhotoLoop()
    {
        const float period = 2.0f;
        yield return new WaitForSeconds(period);
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

    private static float GetFrustumWidthAtDistance(PassthroughCameraAccess camera, float distance)
    {
        Ray leftRay = camera.ViewportPointToRay(new Vector2(0f, 0.5f));
        Ray rightRay = camera.ViewportPointToRay(new Vector2(1f, 0.5f));
        float horizontalFovDegrees = Vector3.Angle(leftRay.direction, rightRay.direction);
        float horizontalFovRadians = horizontalFovDegrees * Mathf.Deg2Rad;
        return 2f * distance * Mathf.Tan(horizontalFovRadians / 2f);
    }
}
