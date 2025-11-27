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
        //_mat = GetComponent<Renderer>().material;
    }

    void Update()
    {
        
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
