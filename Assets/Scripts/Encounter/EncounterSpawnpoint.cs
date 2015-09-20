using UnityEngine;
using System.Collections;

public class EncounterSpawnpoint : MonoBehaviour
{
    // Object Variables
    private Color gizmoColor = new Color(1, 1, 0.08f, 0.35f);
    private Vector3 gizmoCube = new Vector3(1, 2, 1);

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawCube(transform.position + Vector3.up, gizmoCube);
    }
}
