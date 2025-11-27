using UnityEngine;

public class OcclusionService : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer _leftHandRenderer; // see OVRInteractionComprehensive>OVRLeftHandVisual>OpenXRLeftHand>LeftHand
    [SerializeField] private SkinnedMeshRenderer _rightHandRenderer;
    [SerializeField] private Material _handPassthroughMaterial; // For when occlusion is enabled, hand is masked out
    [SerializeField] private Material _handStandardMaterial; // For when occlusion is off
    private bool _occlusionEnabled = true;
    public void ToggleOcclusion()
    {
        SetOcclusion(!_occlusionEnabled);
    }
    public void SetOcclusion(bool state)
    {
        Debug.Log("TOGGLING!");
        _occlusionEnabled = state;
        Material handMat = _occlusionEnabled ? 
            _handPassthroughMaterial : _handStandardMaterial;
        _leftHandRenderer.sharedMaterial = handMat;
        _rightHandRenderer.sharedMaterial = handMat;
    }
    private void Start()
    {
        SetOcclusion(_occlusionEnabled);
    }
}
