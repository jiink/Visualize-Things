using UnityEngine;
using UnityEngine.UI;

public class RadialProgressIndicator : MonoBehaviour
{
    [SerializeField] private Image _image;
    public float Progress
    {
        get => _image.fillAmount;
        set
        {
            _image.fillAmount = value;
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _image.fillAmount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
