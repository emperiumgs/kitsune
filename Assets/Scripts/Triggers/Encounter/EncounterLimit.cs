using UnityEngine;
using System.Collections;

public class EncounterLimit : Trigger
{
    // Reference Variables
    private GameObject encounter
    {
        get { return transform.parent.gameObject; }
    }

    // Object Variables
    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!triggered && other.tag == "Player")
        {
            triggered = true;
            Destroy(encounter);
        }
    }
}
