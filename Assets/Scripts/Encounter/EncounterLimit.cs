using UnityEngine;
using System.Collections;

public class EncounterLimit : MonoBehaviour
{
    // Reference Variables
    private BoxCollider col
    {
        get { return GetComponent<BoxCollider>(); }
    }
    private Vector3 gizmoCube
    {
        get { return new Vector3(col.size.x, col.size.y, col.size.z); }
    }
    private GameObject encounter
    {
        get { return transform.parent.gameObject; }
    }

    // Object Variables
    private Color gizmoColor = new Color(1, 0.08f, 0.08f, 0.3f); // Transparent Red
    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!triggered && other.tag == "Player")
        {
            triggered = true;
            Destroy(encounter);
        }
    }

    /// <summary>
    /// Draws a visible object to interact in the scene editor
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawCube(transform.position + Vector3.up / 2, gizmoCube);
    }
}
