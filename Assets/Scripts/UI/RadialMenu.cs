using System;
using Unity.VisualScripting;
using UnityEngine;

public class RadialMenu : MonoBehaviour
{
    // global position of where the user's selection point is (at finger tip)
    public Vector3 CurrentHitPoint; 
    [SerializeField]
    private GameObject _buttonPrefab;
    [SerializeField]
    private Transform _buttonsParent;
    [SerializeField]
    private OVROverlayCanvas _ovrCanvas;

    private RadialButtonData.RmSelection _currentSelection;

    public event EventHandler SelectionConfirmedEvent;


    void Start()
    {
        
    }

    void Update()
    {
        // get difference between center of radial menu and CurrentHitPoint
        // then project that onto the menu's plane
        // but reject if too far from plane,
        // or if too close to the center,
        // do some math on the projected point to find
        // where on the pie it lands and set the corresponding
        // option as the _currentSelection
    }

    public void Populate(RadialMenuDefinition def)
    {
        int numBtns = def.buttons.Count;

        for (int i = 0; i < numBtns; i++)
        {
            float angle = (float)(i / (float)numBtns * (360.0f));
            //float angle = 30;
            RadialMenuOption newb = Instantiate(_buttonPrefab, _buttonsParent).GetComponent<RadialMenuOption>();
            newb.Populate(def.buttons[i], angle);
        }
        //_ovrCanvas.SetFrameDirty();
    }

    public void ConfirmSelection()
    {

    }
}
