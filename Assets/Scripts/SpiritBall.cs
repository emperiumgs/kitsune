using UnityEngine;
using System.Collections;
using System;

public class SpiritBall : MonoBehaviour
{
    // Customizeable Variables
    [Range(125, 225)]
    [SerializeField]
    private int velocityMultiplier = 180;

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

    private void Shoot()
    {
        col.isTrigger = false;
        rb.isKinematic = false;

        target = transform.parent.position + transform.parent.TransformDirection(0, 0.5f, 2);
        Vector3 force = target - transform.position;
        transform.parent = null;

        rb.velocity = force * Time.deltaTime * velocityMultiplier;
        //rb.MovePosition(target);
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
