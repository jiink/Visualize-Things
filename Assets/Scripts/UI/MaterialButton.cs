using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class MaterialButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Material _material;
    [SerializeField] private TextMeshProUGUI _label;
    public Material Material => _material;
    public event EventHandler HoverEvent;
    public event EventHandler UnhoverEvent;
    public event EventHandler PressEvent;

    public void OnPointerEnter(PointerEventData eventData)
    {
        HoverEvent?.Invoke(this, EventArgs.Empty);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UnhoverEvent?.Invoke(this, EventArgs.Empty);
    }

    public void OnPress()
    {
        PressEvent?.Invoke(this, EventArgs.Empty);
    }

    public void Setup(int number, Material material)
    {
        _label.text = $"#{number}";
        _material = material;
    }
}
