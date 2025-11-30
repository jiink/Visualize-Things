using System;
using UnityEngine;

public class ScanReticle : MonoBehaviour
{
    public event EventHandler HitEvent;
    public bool Killer = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ScanTarget"))
        {
            Debug.Log("hit!");
            HitEvent?.Invoke(this, EventArgs.Empty);
            if (Killer)
            {
                Destroy(other.gameObject);
            }
        }
    }
}
