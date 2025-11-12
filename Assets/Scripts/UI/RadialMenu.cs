using Oculus.Interaction;
using System;
using Unity.VisualScripting;
using UnityEngine;
using static RadialButtonData;

public delegate void RadialButtonEventHandler(object sender, RmSelection id);
public class RadialMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject _buttonPrefab;
    [SerializeField]
    private Transform _buttonsParent;
    [SerializeField]
    private InteractableUnityEventWrapper _closeButton;

    private RadialButtonData.RmSelection _currentSelection;

    public event RadialButtonEventHandler SelectionEvent;


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

    public void Populate(RadialMenuDefinition def)
    {
        int numBtns = def.buttons.Count;

        for (int i = 0; i < numBtns; i++)
        {
            float angle = (float)(i / (float)numBtns * (360.0f));
            RadialMenuOption newb = Instantiate(_buttonPrefab, _buttonsParent).GetComponent<RadialMenuOption>();
            newb.Populate(def.buttons[i], angle);
            newb.Button.WhenUnselect.AddListener(() => { 
                SelectionEvent.Invoke(this, newb.Id);
            });
        }
    }

    public void ConfirmSelection()
    {

    }
}
