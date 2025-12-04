using UnityEngine;

public class AboutMenu : MonoBehaviour
{
    public void OnDebugButtonPress()
    {
        Services.Get<UiManagerService>().ToggleDebugMode();
    }
}
