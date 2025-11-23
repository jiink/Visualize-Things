using UnityEngine;

public class ChildrenLineColorChanger : MonoBehaviour
{
    private Color lineColor = Color.white;
    private Material _mat;

    public Color LineColor
    {
        get { return lineColor; }
        set
        {
            lineColor = value;
            UpdateLineColors();
        }
    }

    void Start()
    {
        LineColor = Color.white;
        _mat = GetComponent<Renderer>().material;
    }

    void Update()
    {
        Vector3 l = Services.Get<UiManagerService>().LeftPointerPos;
        Vector3 r = Services.Get<UiManagerService>().RightPointerPos;
        Vector4[] fadePoints = {
            new Vector4(l.x, l.y, l.z, 1.0f),
            new Vector4(r.x, r.y, r.z, 1.0f),
            Vector4.zero,
            Vector4.zero,
            Vector4.zero,
            Vector4.zero,
        };
        _mat.SetVectorArray("_WorldSpaceFadePoints", fadePoints);
        _mat.SetInt("_UsedPointCount", 2);
        _mat.SetFloat("_OpacityMultiplier", 1.0f);
    }

    private void UpdateLineColors()
    {
        LineRenderer[] lineRenderers = GetComponentsInChildren<LineRenderer>();
        foreach (LineRenderer lineRenderer in lineRenderers)
        {
            lineRenderer.startColor = lineColor;
            lineRenderer.endColor = lineColor;
        }
    }
}
