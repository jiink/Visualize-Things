using Oculus.Interaction;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static RadialButtonData;


public class RadialMenuOption : MonoBehaviour
{
    public TextMeshProUGUI TextPro;
    public UnityEngine.UI.Image Icon;
    public List<Transform> KeepUpright;
    public InteractableUnityEventWrapper Button;
    public RmSelection Id;    

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void Populate(RadialButtonData data, float rotationDeg)
    {
        Id = data.id;
        TextPro.text = data.id.ToString();
        Icon.sprite = data.icon;
        transform.Rotate(0, 0, rotationDeg);
        foreach (Transform t in KeepUpright)
        {
            t.Rotate(0, 0, rotationDeg);
        }
    }
}
