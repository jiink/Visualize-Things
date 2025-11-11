using Oculus.Interaction;
using System;
using Unity.VisualScripting;
using UnityEngine;

public class RadialMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject _buttonPrefab;
    [SerializeField]
    private Transform _buttonsParent;
    [SerializeField]
    private InteractableUnityEventWrapper _closeButton;

    private RadialButtonData.RmSelection _currentSelection;

    public event EventHandler SelectionConfirmedEvent;


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
            //float angle = 30;
            RadialMenuOption newb = Instantiate(_buttonPrefab, _buttonsParent).GetComponent<RadialMenuOption>();
            newb.Populate(def.buttons[i], angle);
        }
    }

    public void ConfirmSelection()
    {

    }
}
