using UnityEngine;

public class FloatingMenu : MonoBehaviour
{
    public void Close()
    { 
        // todo: make this do a closing animation before destruction
        Destroy(gameObject);
    }
}
