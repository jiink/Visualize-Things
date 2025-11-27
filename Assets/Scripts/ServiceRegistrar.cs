using UnityEngine;

public class ServiceRegistrar : MonoBehaviour
{
    void Awake()
    {
        RegisterService<UiManagerService>();
        RegisterService<ModelLoadingService>();
        RegisterService<CommsService>();
        RegisterService<QrService>();
        RegisterService<ProximityMaterialService>();
        RegisterService<OcclusionService>();
    }

    private void RegisterService<T>() where T : class
    {
        T service = GetComponent<T>();
        if (service != null)
        {
            Services.Register(service);
        }
        else
        {
            Debug.LogError($"Service of type {typeof(T).Name} not found on ServiceRegistrar GameObject.");
        }
    }
}
