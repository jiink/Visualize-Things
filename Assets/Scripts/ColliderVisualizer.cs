using System;
using UnityEngine;

public class ColliderVisualizer : MonoBehaviour
{
    [SerializeField] private Material _proxMat;
    [SerializeField] private Material _hoverMat;
    [SerializeField] private Material _selectionMat;
    private MeshRenderer _meshRenderer;
    public enum State
    {
        Proximity,
        Hover,
        Selected
    }
    private State _state;   
    public State MState {
        get => _state;
        set {
            _state = value;
            switch (_state)
            {
                case State.Proximity:
                    _meshRenderer.sharedMaterial = _proxMat;
                    break;
                case State.Hover:
                    _meshRenderer.sharedMaterial = _hoverMat;
                    break;
                case State.Selected:
                    _meshRenderer.sharedMaterial = _selectionMat;
                    break;
                default:
                    Debug.LogError($"bad state {_state}");
                    break;
            }
        }
    }

    void Start()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    void Update()
    {
        
    }

    internal void OnHover()
    {
        Debug.Log("hoverrrr");
        switch (MState)
        {
            case State.Proximity:
                MState = State.Hover;
                break;
            case State.Hover:
                break;
            case State.Selected:
                break;
            default:
                Debug.LogError($"bad state {_state}");
                break;
        }
    }

    internal void OnUnhover()
    {
        switch (MState)
        {
            case State.Proximity:
                break;
            case State.Hover:
                MState = State.Proximity;
                break;
            case State.Selected:
                break;
            default:
                Debug.LogError($"bad state {_state}");
                break;
        }
    }

    internal void OnSelect()
    {
        switch (MState)
        {
            case State.Proximity:
                MState = State.Selected;
                break;
            case State.Hover:
                MState = State.Selected;
                break;
            case State.Selected:
                break;
            default:
                Debug.LogError($"bad state {_state}");
                break;
        }
    }

    internal void OnUnselect()
    {
        switch (MState)
        {
            case State.Proximity:
                break;
            case State.Hover:
                break;
            case State.Selected:
                MState = State.Proximity;
                break;
            default:
                Debug.LogError($"bad state {_state}");
                break;
        }
    }
}
