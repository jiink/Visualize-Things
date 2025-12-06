using TMPro;
using UnityEngine;

public class AboutMenu : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _versionLabel;
    public void OnDebugButtonPress()
    {
        Services.Get<UiManagerService>().ToggleDebugMode();
    }

    public void OnVisitWebsitePress()
    {
        Application.OpenURL("https://jiink.github.io/visualize-things-site/");
    }

    private void Start()
    {
        _versionLabel.text = $"v{Application.version}";
    }
}
