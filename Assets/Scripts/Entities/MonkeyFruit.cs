using UnityEngine;
using System.Collections;

public class MonkeyFruit : MonoBehaviour 
{
    /// <summary>
    /// Auto-destroy after some time
    /// </summary>
    private void Awake()
    {
        Destroy(gameObject, 5f);
    }

    /// <summary>
    /// Deals damage on the player and destroy self
    /// </summary>
    /// <param name="other">The other collider</param>
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Player")
        {
            other.gameObject.SendMessage("TakeDamage", 1);
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
        GetComponent<Rigidbody>().velocity = Utils.RigidbodySpeedTo(dir.x, dir.magnitude - transform.position.y + target.y, dir.z);
    }
}
