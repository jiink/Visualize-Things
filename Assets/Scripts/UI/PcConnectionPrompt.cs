using TMPro;
using UnityEngine;

public class PcConnectionPrompt : MonoBehaviour
{
    [SerializeField] private TextMeshPro _label;
    //public EventHandler ConfirmEvent;
    enum BtnState
    {
        Connect,
        Close
    }
    private BtnState _btnState = BtnState.Connect;
    private string _pcIpAddr = string.Empty;

    public void Populate(string pcIpAddr, string pcHostname)
    {
        _pcIpAddr = pcIpAddr;
        _label.text = $"{pcHostname} ({_pcIpAddr})";
        _btnState = BtnState.Connect;
    }

    public void BtnPress()
    {
        switch (_btnState)
        {
            case BtnState.Connect:
                Confirm();
                break;
            case BtnState.Close:
                Destroy(gameObject);
                break;
            default:
                Debug.LogError("Unhandled state");
                break;
        }
    }

    private async void Confirm()
    {
        //ConfirmEvent?.Invoke(this, EventArgs.Empty);
        string err = await Services.Get<CommsService>().InitConnection(_pcIpAddr);
        if (!string.IsNullOrWhiteSpace(err))
        {
            _label.text = err;
            _btnState = BtnState.Close;
        }
        else
        {
            Debug.Log("nice it worked, bye");
            Destroy(gameObject);
        }
    }
}
