using UnityEngine;

public class SunEditor : MonoBehaviour
{
    [SerializeField] private SunThing _sunThing;
    private Light _dirLight;

    public void Populate(Light light)
    {
        _dirLight = light;
    }

    private void Update()
    {
        if (_dirLight == null || _sunThing == null) { return; }
        _dirLight.intensity = _sunThing.Magnitude * 8.0f;
        _dirLight.transform.rotation = _sunThing.Direction;
    }

    public void SetLightHue(float h)
    {
        Color.RGBToHSV(_dirLight.color, out var _, out var s, out var v);
        _dirLight.color = Color.HSVToRGB(h, s, v);
    }

    public void SetLightSaturation(float s)
    {
        Color.RGBToHSV(_dirLight.color, out var h, out var _, out var v);
        _dirLight.color = Color.HSVToRGB(h, s, v);
    }

    public void SetShadows(bool s)
    {
        _dirLight.shadows = s ? LightShadows.Soft : LightShadows.None;
    }
}
