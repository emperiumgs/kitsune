using UnityEngine;
using System.Collections;
using System;

public class SpiritBall : MonoBehaviour
{
    // Customizeable Variables
    [Range(125, 225)]
    public int velocityMultiplier = 200;

    // Reference Variables
    private Rigidbody rb
    {
        get { return GetComponent<Rigidbody>(); }
    }
    private SphereCollider col
    {
        get { return GetComponent<SphereCollider>(); }
    }
    private Light lightHalo
    {
        get { return GetComponent<Light>(); }
    }

    // Object Variables
    private Vector3 target;
    private bool damageable;

    private void OnTriggerEnter(Collider other)
    {
        if (damageable && other.tag == "GhostlyEnemy")
        {
            other.SendMessage("TakeDamage", transform.position);

            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        print("damaged " + other.gameObject.name);
        if (damageable && other.gameObject.tag == "Enemy")
        {
            other.gameObject.SendMessage("TakeDamage");            
        }

        Destroy(gameObject);
    }

    private void Shoot()
    {
        rb.isKinematic = false;
        col.isTrigger = false;

        target = transform.parent.parent.position + transform.parent.parent.TransformDirection(0, 0.5f, 2);
        transform.LookAt(target);

        transform.SetParent(null);

        rb.velocity = transform.TransformDirection(Vector3.forward) * Time.deltaTime * velocityMultiplier;

        damageable = true;

        StartCoroutine(UntilTarget());
    }

    private IEnumerator UntilTarget()
    {
        lightHalo.range = 0.2f;
        float dist = Vector3.Distance(transform.position, target);
        while (dist > 0.2f)
        {
            dist = Vector3.Distance(transform.position, target);
            yield return null;
        }
        Destroy(gameObject);
    }
}
