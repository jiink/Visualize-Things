using System;
using UnityEngine;

public class SelectableModel : MonoBehaviour
{
    public event EventHandler Selected;

    private Quaternion lockedRotation; // write down rotation once lock happens
    private bool isRotationLocked = false;
    private ColliderVisualizer _visualizerChildCache;
    public string modelFileSourcePath;

    void FixedUpdate()
    {
        if (isRotationLocked)
        {
            gameObject.transform.rotation = lockedRotation;
        }
    }

    public void Select()
    {
        Debug.Log($">>>> CLICKED ON {gameObject.name}!");
        Selected?.Invoke(gameObject, EventArgs.Empty);
    }

    public void ShowVisual()
    {
        if (GetVisualizerChild() != null)
        {
            GetVisualizerChild().gameObject.SetActive(true);
        }
    }

    public void HideVisual()
    {
        if (GetVisualizerChild() != null)
        {
            GetVisualizerChild().gameObject.SetActive(false);
        }
    }

    public ColliderVisualizer GetVisualizerChild()
    {
        if (_visualizerChildCache == null)
        {
            bool found = false;
            foreach (Transform child in transform)
            {
                if (child.CompareTag("SelectionVisualizer"))
                {
                    if (child.gameObject.TryGetComponent<ColliderVisualizer>(out var visCmp))
                    {
                        _visualizerChildCache = visCmp;
                    } else
                    {
                        Debug.LogError("Couldnt find collider vis component");
                    }
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                Debug.LogError("Couldn't find selectionvisualizer child");
            }
        }
        return _visualizerChildCache;
    }

    internal void LockRotation(bool rotationLock)
    {
        // Freeze rotation on rigidbody component if there is one
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.freezeRotation = rotationLock;
        }
        else
        {
            Debug.LogError("SelectableModel: No rigidbody found");
        }
        if (rotationLock)
        {
            // write down the current rotation so we can keep it there
            lockedRotation = gameObject.transform.rotation;
        }
        isRotationLocked = rotationLock;
    }
}
