using UnityEngine;
using UnityEngine.XR.ARSubsystems;

public class AffordanceMatTest : MonoBehaviour
{
    [SerializeField]
    private OVRHand _ovrHandLeft;
    [SerializeField] private OVRCameraRig _ovrCamRig;
    [SerializeField] private GameObject _follower;
    public Material targetMaterial;

    private const int MaxFadePoints = 6;
    private Vector4[] fadePoints = new Vector4[MaxFadePoints];

    void OnValidate()
    {
        if (_ovrCamRig == null)
        {
            _ovrCamRig = FindFirstObjectByType<OVRCameraRig>();
        }
    }

    void Start()
    {
        if (targetMaterial == null)
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                // Get an instance of the material to avoid modifying the asset
                targetMaterial = renderer.material;
                // for some reason setting shader params here doesnt do
                // anything for me, but it works in Update()
            }
        }
    }

    public void SetSingleHighlightPoint(Vector3 worldPosition, float intensity)
    {
        fadePoints[0] = new Vector4(worldPosition.x, worldPosition.y, worldPosition.z, intensity);
        targetMaterial.SetVectorArray("_WorldSpaceFadePoints", fadePoints);
        targetMaterial.SetInt("_UsedPointCount", 1);
        targetMaterial.SetFloat("_OpacityMultiplier", 1.0f);
    }

    private void Update()
    {
        Vector3 handPos = _ovrCamRig.trackingSpace.TransformPoint(_ovrHandLeft.PointerPose.position);
        _follower.transform.position = handPos;
        SetSingleHighlightPoint(handPos, 0.9f);
    }
}
