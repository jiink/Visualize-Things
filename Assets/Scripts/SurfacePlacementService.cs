using Meta.XR.MRUtilityKit;
using Oculus.Interaction.Surfaces;
using Oculus.Interaction;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class SurfacePlacementService : MonoBehaviour
{
    [SerializeField] private EffectMesh _effectMesh;
    private bool _mrukRoomLayersAlreadySetUp = false;
    private bool _mrukRoomInteractablesAlreadySetUp = false;
    private bool _roomClickListeningEnabled = false;

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
        SetupMrukRoomInteractables();
        // make all loaded models transparent to your pointer.
        Services.Get<UiManagerService>().SetAllLoadedModelCollisions(false);
        Services.Get<HandRayService>().LaserLineLayer = LayerMask.GetMask("EffectMesh");
        // start listening to selection events of the effect mesh...
        _roomClickListeningEnabled = true;
    }



    void SetMrukRoomLayers()
    {
        if (_mrukRoomLayersAlreadySetUp) { return; }
        const string TargetLayerName = "EffectMesh";
        int targetLayer = LayerMask.NameToLayer(TargetLayerName);
        if (targetLayer == -1)
        {
            Debug.LogError($"Layer '{TargetLayerName}' not found in the project's Tag and Layers settings");
            return;
        }
        MRUKRoom[] roomComponents = FindObjectsByType<MRUKRoom>(FindObjectsSortMode.None);

        Debug.Log($"Found {roomComponents.Length} objects with the MRUKRoom component.");
        foreach (MRUKRoom room in roomComponents)
        {
            GameObject rootObject = room.gameObject;
            SetLayerRecursively(rootObject, targetLayer);
        }
        Debug.Log($"Successfully set the layer to '{TargetLayerName}' for all MRUKRoom objects and their children.");
        _mrukRoomLayersAlreadySetUp = true;
    }

    void SetupMrukRoomInteractables()
    {
        if (_mrukRoomInteractablesAlreadySetUp) { return; }
        // fill this in
        MRUKAnchor[] anchors = FindObjectsByType<MRUKAnchor>(FindObjectsSortMode.None);
        foreach (MRUKAnchor anchor in anchors)
        {
            if (anchor.transform.childCount > 0)
            {
                Transform firstChild = anchor.transform.GetChild(0);
                GameObject childGo = firstChild.gameObject;

                Collider existingBoxCollider = childGo.GetComponent<Collider>();

                if (existingBoxCollider != null)
                {
                    ColliderSurface colliderSurface = childGo.AddComponent<ColliderSurface>();
                    colliderSurface.InjectCollider(existingBoxCollider);
                    RayInteractable rayInteractable = childGo.AddComponent<RayInteractable>();
                    rayInteractable.InjectSurface(colliderSurface);
                    rayInteractable.WhenPointerEventRaised += (PointerEvent evt) =>
                    {
                        if (!_roomClickListeningEnabled) { return; }
                        if (evt.Type == PointerEventType.Select)
                        {
                            Debug.Log($"MRUK Interactable Selected: {childGo.name}");
                        }
                    };
                }
            }
        }
        _mrukRoomInteractablesAlreadySetUp = true;
    }

    private void SetLayerRecursively(GameObject targetObject, int layer)
    {
        if (targetObject == null)
        {
            return;
        }
        targetObject.layer = layer;
        foreach (Transform child in targetObject.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}
