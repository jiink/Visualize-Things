using Meta.XR.EnvironmentDepth;
using Meta.XR.MRUtilityKit;
using UnityEngine;

public class OcclusionService : MonoBehaviour
{
    [SerializeField] private EnvironmentDepthManager _environmentDepthManager;
    [SerializeField] private SkinnedMeshRenderer _leftHandRenderer; // see OVRInteractionComprehensive>OVRLeftHandVisual>OpenXRLeftHand>LeftHand
    [SerializeField] private SkinnedMeshRenderer _rightHandRenderer;
    [SerializeField] private Material _handPassthroughMaterial; // For when occlusion is enabled, hand is masked out
    [SerializeField] private Material _handStandardMaterial; // For when occlusion is off
    [SerializeField] private EffectMesh _effectMesh;
    [SerializeField] private Material _roomPassthroughMaterial;
    [SerializeField] private Material _roomVisbileMaterial;
    private bool _occlusionEnabled = true;
    public void ToggleOcclusion()
    {
        SetOcclusion(!_occlusionEnabled);
    }
    public void SetOcclusion(bool state)
    {
        _occlusionEnabled = state;
        Material handMat = _occlusionEnabled ? 
            _handPassthroughMaterial : _handStandardMaterial;
        _leftHandRenderer.sharedMaterial = handMat;
        _rightHandRenderer.sharedMaterial = handMat;
        _environmentDepthManager.enabled = _occlusionEnabled;
        _environmentDepthManager.OcclusionShadersMode = _occlusionEnabled ?
            OcclusionShadersMode.SoftOcclusion : OcclusionShadersMode.None;
        _effectMesh.OverrideEffectMaterial(_occlusionEnabled ? _roomPassthroughMaterial : _roomVisbileMaterial);
    }
    private void Start()
    {
        SetOcclusion(_occlusionEnabled);
    }
}
