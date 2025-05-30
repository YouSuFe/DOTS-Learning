using UnityEngine;

public class MouseWorldPosition : MonoBehaviour
{
    public static MouseWorldPosition Instance;

    private void Awake()
    {
        Instance = this;
    }

    public Vector3 GetPosition()
    {
        Ray mouseCameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        //Physics.Raycast(mouseCameraRay, out RaycastHit raycastHit) // if we want to use Physics Ray
        Plane plane = new Plane(Vector3.up, Vector3.zero);

        if(plane.Raycast(mouseCameraRay, out float distance))
        {
            return mouseCameraRay.GetPoint(distance);
        }
        else
        {
            return Vector3.zero;
        }
    }
}
