using TMPro;
using UnityEngine;

public class AboutMenu : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _versionLabel;
    public void OnDebugButtonPress()
    {
        Services.Get<UiManagerService>().ToggleDebugMode();
    }
    private void Start()
    {
        _versionLabel.text = $"v{Application.version}";
    }
}
