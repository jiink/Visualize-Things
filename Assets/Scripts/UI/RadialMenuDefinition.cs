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
        OpenSettings,
        Save,
        Load,
        ChangeOcclusion,
        EditMaterials,
        EditLight,
        SetReflections,
        AboutApp,
        Reset
    }
    public RmSelection id;
    public Sprite icon;
}

public static class RmSelectionExtensions
{
    public static string GetDisplayName(this RadialButtonData.RmSelection selection) => selection switch
    {
        RadialButtonData.RmSelection.LoadModel => "Load Model",
        RadialButtonData.RmSelection.DeleteModel => "Delete Model",
        RadialButtonData.RmSelection.PlaceOnSurface => "Place on Surface",
        RadialButtonData.RmSelection.Adjust => "Adjust",
        RadialButtonData.RmSelection.OpenSettings => "Settings",
        RadialButtonData.RmSelection.Save => "Save Scene",
        RadialButtonData.RmSelection.Load => "Load Scene",
        RadialButtonData.RmSelection.ChangeOcclusion => "Change Occlusion",
        RadialButtonData.RmSelection.EditMaterials => "Edit Materials",
        RadialButtonData.RmSelection.EditLight => "Edit Lighting",
        RadialButtonData.RmSelection.SetReflections => "Set Reflections",
        RadialButtonData.RmSelection.AboutApp => "About",
        RadialButtonData.RmSelection.Reset => "Reset",
        _ => selection.ToString().Replace("_", "?")
    };
}
