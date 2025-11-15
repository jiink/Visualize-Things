using UnityEngine;

public class FaceUser : MonoBehaviour
{
    public bool backwards = false;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (mainCamera != null)
        {
            Vector3 direction = mainCamera.transform.position - transform.position;
            direction.y = 0; // Keep the object upright
            if (direction.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(direction);
                if (backwards)
                {
                    transform.Rotate(0, 180, 0);
                }
            }
        }
    }
}
