using UnityEngine;
using System.Collections;

public class MonkeyFruit : MonoBehaviour
{
    private int damage = 10;
    private float fadeTime = 5f;

    /// <summary>
    /// Auto-destroy after some time
    /// </summary>
    private void Awake()
    {
        Destroy(gameObject, fadeTime);
    }

    /// <summary>
    /// Deals damage on the player and destroy self
    /// </summary>
    /// <param name="other">The other collider</param>
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Ignore Raycast") &&
            other.gameObject.tag != "Enemy")
        {
            if (other.gameObject.tag == "Player")
                other.gameObject.SendMessage("TakeDamage", damage);

            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Adjusts its rigidbody velocity to match a certain distance until the target
    /// </summary>
    /// <param name="target">The target position to reach</param>
    public void ToTarget(Vector3 target)
    {
        Vector3 dir = target - transform.position;
        //GetComponent<Rigidbody>().velocity = Utils.RigidbodySpeedTo(dir.x, Mathf.Abs(dir.y), dir.z);

        float g = Physics.gravity.magnitude;

        //float maxHeight = Mathf.Abs(dir.y/2);
        //float ySpeed = Mathf.Sqrt(2 * g * (maxHeight >= 1 ? maxHeight : 1));
        //float time = 2 * ySpeed / g;
        float time = Mathf.Sqrt(2 * Mathf.Abs(dir.y) / g);
        float xSpeed = dir.x / time;
        float zSpeed = dir.z / time;

        GetComponent<Rigidbody>().velocity = new Vector3(xSpeed, 0, zSpeed);
    }
}
