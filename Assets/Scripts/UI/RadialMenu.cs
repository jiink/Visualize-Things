using Oculus.Interaction;
using System;
using UnityEngine;
using static RadialButtonData;

public delegate void RadialButtonEventHandler(object sender, RmSelection id, GameObject contextObj, OVRHand.Hand hand);
public class RadialMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject _buttonPrefab;
    [SerializeField]
    private Transform _buttonsParent;
    [SerializeField]
    private InteractableUnityEventWrapper _closeButton;
    private RadialButtonData.RmSelection _currentSelection;
    private GameObject _contextObj; // only for context menu

    public event RadialButtonEventHandler SelectionEvent;
    public event EventHandler DestructionEvent;

    private void OnDestroy()
    {
        DestructionEvent?.Invoke(this, EventArgs.Empty);
    }


    void Start()
    {
        _closeButton.WhenUnselect.AddListener(Thingthatdestroysthis);
    }

    private void Thingthatdestroysthis()
    {
        Destroy(gameObject);
    }

    void Update()
    {
    }

    public void Populate(RadialMenuDefinition def, GameObject contextObj)
    {
        _contextObj = contextObj;
        int numBtns = def.buttons.Count;

        for (int i = 0; i < numBtns; i++)
        {
            float angle = (float)(i / (float)numBtns * (360.0f));
            RadialMenuOption newb = Instantiate(_buttonPrefab, _buttonsParent).GetComponent<RadialMenuOption>();
            newb.Populate(def.buttons[i], angle);
            newb.Button.WhenUnselect.AddListener(() => {
                SelectionEvent.Invoke(this, newb.Id, _contextObj, newb.LastInteractingHand);
                Destroy(gameObject);
            });
        }
    }

    public void ConfirmSelection()
    {

    }
}
