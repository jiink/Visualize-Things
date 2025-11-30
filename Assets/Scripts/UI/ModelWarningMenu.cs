using TMPro;
using UnityEngine;

public class ModelWarningMenu : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _warningDescLabel;
    private string _warningDesc = string.Empty;
    public GameObject ContextObject { get; internal set; }
    public string WarningDescription { 
        get => _warningDesc;
        set
        {
            _warningDesc = value;
            _warningDescLabel.text = _warningDesc;
        }
    }

    public void OnImportAnywaysChoice()
    {
        ContextObject.SetActive(true);
        Destroy(gameObject);
    }

    public void OnDeleteChoice()
    {
        Destroy(ContextObject);
        Destroy(gameObject);
    }
}
