using Oculus.Interaction;
using System;
using UnityEngine;

public class HandRayService : MonoBehaviour
{
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
        Debug.Log("object unhovered");
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
        Services.Get<UiManagerService>().ShowContextMenu(ob);
    }

    private void Update()
    {
    }
}
