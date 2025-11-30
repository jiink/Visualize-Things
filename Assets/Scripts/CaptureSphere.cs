using UnityEngine;

public class CaptureSphere : MonoBehaviour
{
    const int resolutionW = 2048;
    [SerializeField] private GameObject _displaySphere; // Assign the Sphere object here
    private Camera _cam;
    private bool _camSetup = false;

    void SetupCaptureCamera()
    {
        if (_camSetup) return;
        _cam = GetComponent<Camera>();
        if (_cam == null)
        {
            _cam = gameObject.AddComponent<Camera>();
        }
        _cam.enabled = false;
        _cam.stereoTargetEye = StereoTargetEyeMask.None;
        _cam.fieldOfView = 90;
        _cam.nearClipPlane = 0.001f;
        _cam.farClipPlane = 5.0f;
        _cam.clearFlags = CameraClearFlags.SolidColor;
        _cam.backgroundColor = Color.black;
        _cam.cullingMask = LayerMask.GetMask("Photo");
        _camSetup = true;
    }

    public void CaptureAndApply()
    {
        SetupCaptureCamera();
        RenderTexture cubemapRT = new(resolutionW, resolutionW, 16)
        {
            dimension = UnityEngine.Rendering.TextureDimension.Cube
        };
        _cam.RenderToCubemap(cubemapRT);
        RenderTexture equirectRT = new(resolutionW, resolutionW / 2, 16);
        cubemapRT.ConvertToEquirect(equirectRT);
        Texture2D finalTex = new(equirectRT.width, equirectRT.height, TextureFormat.RGB24, false)
        {
            wrapMode = TextureWrapMode.Clamp
        };
        RenderTexture.active = equirectRT;
        finalTex.ReadPixels(new Rect(0, 0, equirectRT.width, equirectRT.height), 0, 0);
        finalTex.Apply();
        RenderTexture.active = null;
        if (_displaySphere != null)
        {
            Renderer sphereRenderer = _displaySphere.GetComponent<Renderer>();
            sphereRenderer.material.mainTexture = finalTex;
        }
        Destroy(cubemapRT);
        Destroy(equirectRT);
        Debug.Log("sphere capture done");
    }

}
