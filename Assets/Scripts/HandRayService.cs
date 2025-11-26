using Oculus.Interaction;
using System;
using UnityEngine;

public class HandRayService : MonoBehaviour
{
    [SerializeField] private RayInteractor _handRayInteractorLeft;
    [SerializeField] private RayInteractor _handRayInteractorRight;
    private void Start()
    {
        Services.Get<ModelLoadingService>().ModelSpawnedEvent += OnNewModelSpawned;
    }

    private void OnNewModelSpawned(object sender, ModelSpawnedEventArgs e)
    {
        GameObject ob = e.SpawnedModel;
        if (!ob.TryGetComponent<PointableUnityEventWrapper>(out var evWrap))
        {
            Debug.LogError("no event wrapper on new model");
            return;
        }
        evWrap.WhenSelect.AddListener((pe) => OnModelSelected(ob, pe));
        evWrap.WhenUnselect.AddListener((pe) => OnModelUnselected(ob, pe));
        evWrap.WhenHover.AddListener((pe) => OnModelHovered(ob, pe));
        evWrap.WhenUnhover.AddListener((pe) => OnModelUnhovered(ob, pe));
        Debug.Log("new model subscribed stuff");
    }

    // could happen twice in a row
    private void OnModelUnhovered(GameObject ob, PointerEvent pe)
    {
        Debug.Log($"object unhovered.");
    }

    private void OnModelHovered(GameObject ob, PointerEvent pe)
    {
        Debug.Log("object hovered");
    }

    // when your pinch stops
    private void OnModelUnselected(GameObject ob, PointerEvent pe)
    {
        Debug.Log("<color=blue>object unselected</color>");
    }

    // when your pinch starts
    private void OnModelSelected(GameObject ob, PointerEvent pe)
    {
        Debug.Log("<color=green>object selected!</color>");
        Vector3 pointerP;
        if (pe.Identifier == _handRayInteractorLeft.Identifier)
        {
            pointerP = Services.Get<UiManagerService>().LeftPointerPos;
        }
        else if (pe.Identifier == _handRayInteractorRight.Identifier)
        {
            pointerP = Services.Get<UiManagerService>().RightPointerPos;
        }
        else
        {
            Debug.LogError($"Couldn't match pointerevent identifier {pe.Identifier}");
            pointerP = Services.Get<UiManagerService>().RightPointerPos;
        }
        Services.Get<UiManagerService>().ShowContextMenu(
            ob,
            pointerP + (Camera.main.transform.forward * 0.1f)
        );
    }

    private void Update()
    {
    }
}
