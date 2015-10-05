using UnityEngine;
using System.Collections;

public class ThirdPersonCamera : MonoBehaviour
{
    // Customizeable Variables
    public Vector3 offsetVector = new Vector3(0, 1.5f, -2.5f);
    public float offsetSpeed = 6f;
    
    // Reference Variables
    private Transform pivot
    {
        get { return transform.parent; }
    }
    private Transform target
    {
        get { return FindObjectOfType<Player>().transform; }
    }

    // Object Variables
    private bool m_MouseOriented;

    // Public Reference Variables
    public bool mouseOriented
    {
        get { return m_MouseOriented; }
    }

    private void Start()
    {
        transform.position = target.position + target.TransformDirection(offsetVector);
        transform.rotation = Quaternion.Euler(0, target.eulerAngles.y, 0);
    }

    private void Update()
    {
        float x = Input.GetAxis("MouseX");
        float y = Input.GetAxis("MouseY");

        if (x != 0 || y != 0)
        {
            m_MouseOriented = true;

            transform.RotateAround(pivot.position, Vector3.up, x * 5);
        }
    }

    private void FixedUpdate()
    {
        UpdatePivot();     

        if (!mouseOriented)
        {
            //transform.position = Vector3.Lerp(transform.position, target.position + target.TransformDirection(offsetVector), Time.deltaTime * offsetSpeed);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, target.eulerAngles.y, 0), Time.deltaTime * offsetSpeed);
        }
    }

    private void UpdatePivot()
    {
        pivot.position = Vector3.Lerp(pivot.position, target.position, Time.deltaTime * offsetSpeed);
        // Smoother rotation
        //pivot.rotation = Quaternion.Lerp(pivot.rotation, target.rotation, Time.deltaTime * offsetSpeed);
        pivot.rotation = target.rotation;
    }
}
