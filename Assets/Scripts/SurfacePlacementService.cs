using Meta.XR.MRUtilityKit;
using UnityEngine;

public class SurfacePlacementService : MonoBehaviour
{
    [SerializeField] private EffectMesh _effectMesh;

    void OnValidate()
    {
        if (_effectMesh == null)
        {
            _effectMesh = FindFirstObjectByType<EffectMesh>();
        }
    }

    public void Begin(GameObject go)
    {
        if (go == null)
        {
            Debug.LogError("can't begin surface placement for null");
            return;
        }
        SetMrukRoomLayers();
        // make the GO transparent to your pointer
        //if (go.TryGetComponent<Collider>(out var goCollider))
        //{
        //    goCollider.enabled = false;
        //}
        //else
        //{
        //    Debug.LogError("model for surface placement has no collider");
        //}
        // actually, make all loaded models transparent to your pointer.
        Services.Get<UiManagerService>().SetAllLoadedModelCollisions(false);
        Services.Get<HandRayService>().LaserLineLayer = LayerMask.GetMask("EffectMesh");
        // start listening to selection events of the effect mesh... too hard... instead just wait for a pinch from either hand?
        
    }


    
    void SetMrukRoomLayers()
    {
        const string TargetLayerName = "EffectMesh";
        // 1. Get the integer ID for the target layer name.
        int targetLayer = LayerMask.NameToLayer(TargetLayerName);
        if (targetLayer == -1)
        {
            Debug.LogError($"Layer '{TargetLayerName}' not found in the project's Tag and Layers settings. Please create it.");
            return;
        }
        MRUKRoom[] roomComponents = FindObjectsByType<MRUKRoom>(FindObjectsSortMode.None);

        Debug.Log($"Found {roomComponents.Length} objects with the MRUKRoom component.");

        // 3. Iterate through each object and recursively set the layer.
        foreach (MRUKRoom room in roomComponents)
        {
            GameObject rootObject = room.gameObject;
            SetLayerRecursively(rootObject, targetLayer);
        }

        Debug.Log($"Successfully set the layer to '{TargetLayerName}' for all MRUKRoom objects and their children.");
    }

    /// <summary>
    /// Recursively sets the layer of the given GameObject and all its children.
    /// </summary>
    /// <param name="targetObject">The object to start from.</param>
    /// <param name="layer">The integer ID of the target layer.</param>
    private void SetLayerRecursively(GameObject targetObject, int layer)
    {
        if (targetObject == null)
        {
            return;
        }

        // Set the layer for the current object.
        targetObject.layer = layer;

        // Iterate through all children and call the function recursively.
        foreach (Transform child in targetObject.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}
