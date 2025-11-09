using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class RadialMenuOption : MonoBehaviour
{
    public TextMeshProUGUI TextPro;
    public UnityEngine.UI.Image Icon;
    public Transform KeepUpright;

    void Start()
    {
        
    }
    void Update()
    {
        
    }

    public void Populate(RadialButtonData data, float rotationDeg)
    {
        TextPro.text = data.id.ToString();
        Icon.sprite = data.icon;
        transform.Rotate(0, 0, rotationDeg);
        KeepUpright.Rotate(0, 0, -rotationDeg);
    }
}
