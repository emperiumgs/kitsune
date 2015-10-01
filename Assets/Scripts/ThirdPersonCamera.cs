using UnityEngine;
using System.Collections;

public class ThirdPersonCamera : MonoBehaviour
{
    // Customizeable Variables
    public Vector3 offsetVector = new Vector3(0, 1.5f, -2.5f);
    public float offsetSpeed = 6f;
    
    // Reference Variables
    private Transform target
    {
        get { return FindObjectOfType<Player>().transform; }
    }        

    private void Start()
    {
        transform.position = target.position + target.TransformDirection(offsetVector);
        transform.rotation = Quaternion.Euler(0, target.eulerAngles.y, 0);
    }

    private void FixedUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, target.position + target.TransformDirection(offsetVector), Time.deltaTime * offsetSpeed);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, target.eulerAngles.y, 0), Time.deltaTime * offsetSpeed);
    }
}
