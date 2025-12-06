using UnityEngine;

public class SmoothTrackToTarget : MonoBehaviour
{
    public Transform Target;
    
    public bool trackPosition = true;
    public float posSmoothTime = 0.5f;
    public Vector3 positionOffset;
    
    public bool keepUpright = true; 
    public float rotSpeed = 10.0f;

    private Vector3 _currentVelocity;

    private void LateUpdate()
    {
        if (Target == null) return;
        if (trackPosition)
        {
            Vector3 targetPosition = Target.position + positionOffset;
            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPosition,
                ref _currentVelocity,
                posSmoothTime
            );
        }
        Quaternion targetRotation;

        if (keepUpright)
        {
            targetRotation = Quaternion.Euler(0, Target.eulerAngles.y, 0);
        }
        else
        {
            targetRotation = Target.rotation;
        }

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * rotSpeed
        );
    }

    public void SnapToTarget()
    {
        if (Target == null) return;
        Quaternion snapRot = keepUpright 
            ? Quaternion.Euler(0, Target.eulerAngles.y, 0) 
            : Target.rotation;

        transform.SetPositionAndRotation(Target.position + positionOffset, snapRot);
        _currentVelocity = Vector3.zero;
    }
}