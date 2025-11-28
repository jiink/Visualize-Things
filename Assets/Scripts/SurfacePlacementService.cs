using Meta.XR.MRUtilityKit;
using Oculus.Interaction.Surfaces;
using Oculus.Interaction;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;

public class SurfacePlacementService : MonoBehaviour
{
    [SerializeField] private EffectMesh _effectMesh;
    private bool _mrukRoomLayersAlreadySetUp = false;
    private bool _mrukRoomInteractablesAlreadySetUp = false;
    private bool _placementActive = false;
    private GameObject _currentGo = null;

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
        _placementActive = true;
        _currentGo = go;
    }

    private void Update()
    {
        if (_placementActive)
        {
            if (_currentGo == null)
            {
                Debug.LogError("current game object is null");
                _placementActive = false;
                return;
            }
            // todo: instead of hard-coding a hand, should get
            // what hand was used to press the button in the radial
            // menu through it being passed in the Begin() method
            Pose placementPose = MRUK.Instance.GetCurrentRoom().GetBestPoseFromRaycast(
                new Ray(
                    Services.Get<UiManagerService>().LeftPointerPos,
                    Services.Get<UiManagerService>().LeftPointerRot * Vector3.forward),
                20.0f,
                new LabelFilter(),
                out MRUKAnchor anchor,
                out Vector3 sNormal
                );
            if (placementPose != null)
            {
                _currentGo.transform.SetPositionAndRotation(
                    placementPose.position, placementPose.rotation);
                PreventClipping(_currentGo, placementPose.position, sNormal );
            }
            else
            {
                Debug.LogError("placementpose or anchor is null");
            }
        }
    }

    // AI did this one
    private void PreventClipping(GameObject go, Vector3 hitPoint, Vector3 surfaceNormal)
    {
        BoxCollider boxCol = go.GetComponent<BoxCollider>();
        if (boxCol == null) return;

        // 1. Convert the surface normal into the object's local space
        // If the normal is pointing straight out of a wall, and the object 
        // is facing away, this local vector points "Backwards" relative to the object.
        Vector3 localNormal = go.transform.InverseTransformDirection(surfaceNormal);

        // 2. Find the corner of the box that is 'furthest behind' the normal direction.
        // Basically, we are looking for the point on the box that wants to be 
        // touching the wall/floor.
        // We use the box center and extents (half-size) to find this corner.
        Vector3 localDeepestPoint = boxCol.center;

        // For each axis (x, y, z), move to the edge that is opposite to the normal direction
        Vector3 extents = boxCol.size * 0.5f;

        // If the normal points Positive X, we want the Negative X edge, etc.
        localDeepestPoint.x += (localNormal.x > 0 ? -extents.x : extents.x);
        localDeepestPoint.y += (localNormal.y > 0 ? -extents.y : extents.y);
        localDeepestPoint.z += (localNormal.z > 0 ? -extents.z : extents.z);

        // 3. Convert that local corner point back to World Space
        Vector3 worldDeepestPoint = go.transform.TransformPoint(localDeepestPoint);

        // 4. Create a geometric plane representing the wall/floor surface
        Plane surfacePlane = new(surfaceNormal, hitPoint);

        // 5. Check how far 'behind' the plane that deepest point is.
        // GetDistanceToPoint returns negative values if the point is behind the plane.
        float distance = surfacePlane.GetDistanceToPoint(worldDeepestPoint);

        // 6. If it's negative (clipping), push the object out along the normal
        if (distance < 0)
        {
            go.transform.position += surfaceNormal * Mathf.Abs(distance);
        }
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
                        if (!_placementActive) { return; }
                        if (evt.Type == PointerEventType.Select)
                        {
                            Debug.Log($"MRUK Interactable Selected: {childGo.name}");
                            End();
                        }
                    };
                }
            }
        }
        _mrukRoomInteractablesAlreadySetUp = true;
    }

    private void End()
    {
        _placementActive = false;
        _currentGo = null;
        Services.Get<UiManagerService>().SetAllLoadedModelCollisions(true);
        Services.Get<HandRayService>().LaserLineLayer = LayerMask.GetMask("Models");
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
