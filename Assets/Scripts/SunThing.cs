using UnityEngine;

public class SunThing : MonoBehaviour
{
    [SerializeField] private GameObject _arrowTip;
    [SerializeField] private GameObject _arrow;
    [SerializeField] private GameObject _body;
    private Vector3 _diff = Vector3.zero;
    public Quaternion Direction {
        get
        {
            if (Magnitude > 0.0001f)
            {
                return Quaternion.LookRotation(_diff);
            }
            return Quaternion.identity;
        }
    }
    public float Magnitude => _diff.magnitude;
    private void Update()
    {
        _diff = _arrowTip.transform.position - transform.position;
        _body.transform.LookAt(_arrowTip.transform.position);
        _arrow.transform.LookAt(_arrowTip.transform.position);
        _arrow.transform.localScale = new Vector3(
            _arrow.transform.localScale.x,
            _arrow.transform.localScale.y,
            Magnitude
            );
    }
}
