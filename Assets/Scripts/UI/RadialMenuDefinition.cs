using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RadialMenuDefinition", menuName = "Scriptable Objects/RadialMenuDefinition")]
public class RadialMenuDefinition : ScriptableObject
{
    public List<RadialButtonData> buttons;    
}

[System.Serializable]
public struct RadialButtonData
{
    public enum RmSelection
    {
        LoadModel,
        DeleteModel,
        PlaceOnSurface,
        Adjust,
        Settings
    }
    public RmSelection id;
    public Sprite icon;
}
