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
        _dirLight.intensity = _sunThing.Magnitude * 5.0f;
        _dirLight.transform.rotation = _sunThing.Direction;
    }
}
