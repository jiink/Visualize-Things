using UnityEngine;

public class ProximityMaterialService : MonoBehaviour
{
    [SerializeField] private Material _mat;

    void Start()
    {
        
    }

    // all objects with this material applied should share shader variables
    void Update()
    {
        Vector3 l = Services.Get<UiManagerService>().LeftPointerPos;
        Vector3 r = Services.Get<UiManagerService>().RightPointerPos;
        Vector4[] fadePoints = {
            new(l.x, l.y, l.z, 1.0f),
            new(r.x, r.y, r.z, 1.0f),
            Vector4.zero,
            Vector4.zero,
            Vector4.zero,
            Vector4.zero,
        };
        _mat.SetVectorArray("_WorldSpaceFadePoints", fadePoints);
        _mat.SetInt("_UsedPointCount", 2);
        _mat.SetFloat("_OpacityMultiplier", 1.0f);
    }
}
