using TMPro;
using UnityEngine;

public class LoadingIndicator : MonoBehaviour
{
    public TextMeshPro text;
    public string Text
    {
        set
        {
            text.text = value;
        }
    }
}
