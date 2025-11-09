using System;
using Unity.VisualScripting;
using UnityEngine;

public class RadialMenu : MonoBehaviour
{
    public GameObject ButtonPrefab;
    public Transform ButtonsParent;
    [SerializeField]
    private OVROverlayCanvas _ovrCanvas;

    private RadialButtonData.RmSelection _currentSelection;

    public event EventHandler SelectionConfirmedEvent;


    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void Populate(RadialMenuDefinition def)
    {
        int numBtns = def.buttons.Count;

        for (int i = 0; i < numBtns; i++)
        {
            float angle = (float)(i / (float)numBtns * (360.0f));
            //float angle = 30;
            RadialMenuOption newb = Instantiate(ButtonPrefab, ButtonsParent).GetComponent<RadialMenuOption>();
            newb.Populate(def.buttons[i], angle);
        }
        _ovrCanvas.SetFrameDirty();
    }

    public void ConfirmSelection()
    {

    }
}
