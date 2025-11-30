using System;
using System.Collections;
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
            StartCoroutine(KillAndInvoke(other.gameObject.transform.parent.gameObject));
        }
    }

    private IEnumerator KillAndInvoke(GameObject go)
    {
        if (Killer)
        {
            Destroy(go);
            yield return new WaitForEndOfFrame();
        }
        HitEvent?.Invoke(this, EventArgs.Empty);
    }
}
