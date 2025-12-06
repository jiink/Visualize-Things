using Oculus.Interaction;
using Oculus.Interaction.Input;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static RadialButtonData;


public class RadialMenuOption : MonoBehaviour
{
    public TextMeshProUGUI TextPro;
    public MeshRenderer IconRenderer;
    public List<Transform> KeepUpright;
    public InteractableUnityEventWrapper Button;
    public GameObject LabelRoot;
    public RmSelection Id;
    public OVRHand.Hand LastInteractingHand { get; private set; } = OVRHand.Hand.None;
    private IInteractableView _interactableView;

    private void Start()
    {
        _interactableView = Button.GetComponent<IInteractableView>();
        if (_interactableView == null)
        {
            Debug.LogError("RadialMenuOption: Wrapper is assigned, but no IInteractableView found on it");
            return;
        }
        Button.WhenSelect.AddListener(CaptureHand);
        Button.WhenHover.AddListener(ShowLabel);
        Button.WhenUnhover.AddListener(HideLabel);
        LabelRoot.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Button != null) { Button.WhenSelect.RemoveListener(CaptureHand); }
    }

    public void Populate(RadialButtonData data, float rotationDeg)
    {
        Id = data.id;
        TextPro.text = data.id.ToString();
        if (IconRenderer != null && data.icon != null)
        {
            IconRenderer.material.mainTexture = data.icon.texture;
        }
        transform.Rotate(0, 0, rotationDeg);
        foreach (Transform t in KeepUpright)
        {
            t.Rotate(0, 0, rotationDeg);
        }
    }

    private void ShowLabel()
    {
        LabelRoot.SetActive(true);
    }

    private void HideLabel()
    {
        LabelRoot.SetActive(false);
    }

    private void CaptureHand()
    {
        //Debug.Log($"[RadialMenuOption] '{Id}' Pressed (Select). Searching for hand...");
        if (_interactableView == null) return;

        foreach (var interactorView in _interactableView.SelectingInteractorViews)
        {
            if (interactorView is MonoBehaviour mb)
            {
                var handInterface = mb.GetComponentInParent<IHand>();

                if (handInterface != null)
                {
                    LastInteractingHand = ConvertHandedness(handInterface.Handedness);
                    //Debug.Log($"[RadialMenuOption] Found Hand: {LastInteractingHand} on Interactor: {mb.name}");
                    return;
                }
                else
                {
                    //Debug.LogWarning($"[RadialMenuOption] Found Interactor '{mb.name}' but could not find IHand in parent.");
                }
            }
        }
        //Debug.LogWarning($"[RadialMenuOption] No Hand found in SelectingInteractors. Defaulting.");
        LastInteractingHand = OVRHand.Hand.HandRight;
    }


    private OVRHand.Hand ConvertHandedness(Handedness handedness)
    {
        return handedness switch
        {
            Handedness.Left => OVRHand.Hand.HandLeft,
            Handedness.Right => OVRHand.Hand.HandRight,
            _ => OVRHand.Hand.None
        };
    }

}
