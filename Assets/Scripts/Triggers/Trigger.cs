using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]
public class Trigger : MonoBehaviour
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

    // Object Variables
    public Color gizmoColor;

    /// <summary>
    /// Draws a visible object to interact in the scene editor
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawCube(transform.position + (col.bounds.center - transform.position), gizmoCube);
    }
}
