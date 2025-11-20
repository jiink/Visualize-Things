using Meta.XR.MRUtilityKit;
using System;
using UnityEngine;

public class QrService : MonoBehaviour
{
    [SerializeField]
    private MRUK _mrukInstance;

    public static bool IsSupported
            => OVRAnchor.TrackerConfiguration.QRCodeTrackingSupported;
    public static bool HasPermissions
#if UNITY_EDITOR
        => true;
#else
            => UnityEngine.Android.Permission.HasUserAuthorizedPermission(OVRPermissionsRequester.ScenePermission);
#endif

    void OnValidate()
    {
        if (!_mrukInstance && FindAnyObjectByType<MRUK>() is { } mruk && mruk.gameObject.scene == gameObject.scene)
        {
            _mrukInstance = mruk;
        }
    }

    void Start()
    {
        RequestRequiredPermissions((hasPerms) => {
            if (!hasPerms)
            {
                Debug.LogError($"Didn't get QR scanning perms.");
                return;
            }
            Debug.Log($"Got QR scanning perms.");
            EnableQrTracking();
        });
    }

    public void EnableQrTracking()
    {
        _mrukInstance.SceneSettings.TrackableAdded.AddListener(OnTrackableAdded);
        _mrukInstance.SceneSettings.TrackableRemoved.AddListener(OnTrackableRemoved);
        var config = _mrukInstance.SceneSettings.TrackerConfiguration;
        config.QRCodeTrackingEnabled = true;
        _mrukInstance.SceneSettings.TrackerConfiguration = config;
        Debug.Log("Enabled QR code tracking");
    }

    // mostly taken from Meta MRUK QR example scene
    public void RequestRequiredPermissions(Action<bool> onRequestComplete)
    {
#if UNITY_EDITOR
        const string kCantRequestMsg =
            "Cannot request Android permission when using Link or XR Sim. " +
            "For Link, enable the spatial data permission from the Link app under Settings > Beta > Spatial Data over Meta Quest Link. " +
            "For XR Sim, no permission is necessary.";

        Debug.LogWarning(kCantRequestMsg);

        onRequestComplete?.Invoke(HasPermissions);
#else
        Debug.Log($"Requesting {OVRPermissionsRequester.ScenePermission} ... (currently: {HasPermissions})");

        var callbacks = new UnityEngine.Android.PermissionCallbacks();
        callbacks.PermissionGranted += perm => Debug.Log($"{perm} granted");

        var msgDenied = $"{OVRPermissionsRequester.ScenePermission} denied. Please press the 'Request Permission' button again.";
        var msgDeniedPermanently = $"{OVRPermissionsRequester.ScenePermission} permanently denied. To enable:\n" +
                                    $"    1. Uninstall and reinstall the app, OR\n" +
                                    $"    2. Manually grant permission in device Settings > Privacy & Safety > App Permissions.";

        callbacks.PermissionDenied += perm =>
        {
            // ShouldShowRequestPermissionRationale returns false only if
            // the user selected 'Never ask again' or if the user has never
            // been asked for the permission (which can't be the case here).
            Debug.LogError(
                UnityEngine.Android.Permission.ShouldShowRequestPermissionRationale(perm)
                    ? msgDenied : msgDeniedPermanently);
        };

        if (onRequestComplete is not null)
        {
            callbacks.PermissionGranted += _ => onRequestComplete(HasPermissions);
            callbacks.PermissionDenied += _ => onRequestComplete(HasPermissions);
        }

        UnityEngine.Android.Permission.RequestUserPermission(OVRPermissionsRequester.ScenePermission, callbacks);
#endif // UNITY_EDITOR
    }

    public void OnTrackableAdded(MRUKTrackable trackable)
    {
        if (trackable.TrackableType != OVRAnchor.TrackableType.QRCode)
        {
            return;
        }
        //var instance = Instantiate(_qrCodePrefab, trackable.transform);
        //var qrCode = instance.GetComponent<QRCode>();
        //qrCode.Initialize(trackable);
        //instance.GetComponent<Bounded2DVisualizer>().Initialize(trackable);
        Debug.Log($"{nameof(OnTrackableAdded)}: QRCode tracked!\nUUID={trackable.Anchor.Uuid}\nData={trackable.MarkerPayloadString}");
    }

    public void OnTrackableRemoved(MRUKTrackable trackable)
    {
        if (trackable.TrackableType != OVRAnchor.TrackableType.QRCode)
        {
            return;
        }
        Debug.Log($"{nameof(OnTrackableRemoved)}: {trackable.Anchor.Uuid.ToString("N").Remove(8)}[..]");
        Destroy(trackable.gameObject);
    }

}
