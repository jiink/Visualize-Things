using System;
using TMPro;
using UnityEngine;

public class MaterialListing : MonoBehaviour
{
    public TextMeshProUGUI nameLabel;
    private Texture2D _texture;
    private Material _material;
    public Material Material
    {
        get
        {
            return _material;
        }
        set
        {
            _material = value;
            UpdateLabel();
        }
    }
    private int _materialNum;
    public int MaterialNum
    {
        get
        {
            return _materialNum;
        }
        set
        {
            _materialNum = value;
            UpdateLabel();
        }
    }

    void UpdateLabel()
    {
        if (Material == null)
        {
            nameLabel.text = $"Material #{MaterialNum}: (none)";
            return;
        }
        nameLabel.text = $"Material #{MaterialNum}: {Material.name}";
    }

    public void SetMetallic(float value)
    {
        Material.SetFloat("_Metallic", value);
    }

    public void SetSmoothness(float value)
    {
        Material.SetFloat("_Glossiness", value);
    }

    public void SetHue(float h)
    {
        Color.RGBToHSV(Material.color, out _, out float s, out float v);
        Material.color = Color.HSVToRGB(h, s, v);
    }

    public void SetSaturation(float s)
    {
        Color.RGBToHSV(Material.color, out float h, out _, out float v);
        Material.color = Color.HSVToRGB(h, s, v);
    }

    public void SetLightness(float v)
    {
        Color.RGBToHSV(Material.color, out float h, out float s, out _);
        Material.color = Color.HSVToRGB(h, s, v);
    }
}
