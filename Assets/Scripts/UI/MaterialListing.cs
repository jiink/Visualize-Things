using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MaterialListing : MonoBehaviour
{
    public Slider hueSlider;
    public Slider saturationSlider;
    public Slider lightnessSlider;
    public Slider metallicSlider;
    public Slider smoothnessSlider;
    public TextMeshProUGUI nameLabel;
    private Material _material;
    private int _materialNum;
    public event EventHandler BackEvent;

    
    public void SetMetallic(float value)
    {
        _material.SetFloat("_Metallic", value);
    }

    public void SetSmoothness(float value)
    {
        _material.SetFloat("_Glossiness", value);
    }

    public void SetHue(float h)
    {
        Color.RGBToHSV(_material.color, out _, out float s, out float v);
        _material.color = Color.HSVToRGB(h, s, v);
    }

    public void SetSaturation(float s)
    {
        Color.RGBToHSV(_material.color, out float h, out _, out float v);
        _material.color = Color.HSVToRGB(h, s, v);
    }

    public void SetLightness(float v)
    {
        Color.RGBToHSV(_material.color, out float h, out float s, out _);
        _material.color = Color.HSVToRGB(h, s, v);
    }

    internal void Setup(int materialNum, Material currentMat)
    {
        if (currentMat == null)
        {
            Debug.LogError("null material given");
            return;
        }
        _material = currentMat;
        nameLabel.text = $"#{materialNum}";
        float currentMetallic = _material.HasProperty("_Metallic") ? _material.GetFloat("_Metallic") : 0f;
        float currentSmoothness = _material.HasProperty("_Glossiness") ? _material.GetFloat("_Glossiness") : 0f;
        Color.RGBToHSV(_material.color, out float h, out float s, out float v);
        if (metallicSlider != null) metallicSlider.SetValueWithoutNotify(currentMetallic);
        if (smoothnessSlider != null) smoothnessSlider.SetValueWithoutNotify(currentSmoothness);
        if (hueSlider != null) hueSlider.SetValueWithoutNotify(h);
        if (saturationSlider != null) saturationSlider.SetValueWithoutNotify(s);
        if (lightnessSlider != null) lightnessSlider.SetValueWithoutNotify(v);

    }

    public void OnBack() {
        BackEvent?.Invoke(this, EventArgs.Empty);
    }
}
