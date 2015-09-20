using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]
public class Encounter : MonoBehaviour
{
    [Header("Encounter Options")]
    // Customizeable Variables
    public GameObject enemyPrefab;
    public int quantity;    

    // Reference Variables
    private BoxCollider col
    {
        get { return GetComponent<BoxCollider>(); }
    }
    private Vector3 gizmoCube
    {
        get { return new Vector3(col.size.x, col.size.y, col.size.z); }
    }
    private EncounterSpawnpoint[] spawnpoints
    {
        get { return GetComponentsInChildren<EncounterSpawnpoint>(); }
    }

    // Object Variables
    private Color gizmoColor = new Color(1, 0.5f, 0.08f, 0.3f); // Transparent Yellowed Red
    private GameObject[] enemies;
    private bool triggered;
    
    /// <summary>
    /// Initializes the enemies array
    /// </summary>
    private void Awake()
    {
        enemies = new GameObject[quantity];
    }

    /// <summary>
    /// Spawns the enemies along the encounter's spawnpoints
    /// </summary>
    private void TriggerEncounter()
    {
        triggered = true;        

        int spMax = spawnpoints.Length;
        int i = 0;
        int r = 0;
        Vector3 pos;
        Vector3 randomPos;
        while (i < quantity)
        {
            randomPos = Random.insideUnitSphere;
            if (i < spMax)
            {
                pos = spawnpoints[i].transform.position;
                randomPos.y = pos.y;                
            }
            else
            {
                r = Random.Range(0, spMax);
                pos = spawnpoints[r].transform.position;
                randomPos.y = pos.y;
            }

            enemies[i] = Instantiate(enemyPrefab, pos + randomPos, Random.rotation) as GameObject;
            enemies[i].transform.SetParent(transform);

            i++;
        }

        // When all enemies die, clear the encounter
        StartCoroutine(OnCleared());
    }

    /// <summary>
    /// Checks if all enemies in this encounter are dead
    /// </summary>
    /// <returns></returns>
    private bool Clear()
    {
        int n = 0;
        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] == null)
                n++;
        }

        if (n == enemies.Length)
            return true;
        else
            return false;
    }

    /// <summary>
    /// Removes the encounter if all enemies are dead
    /// </summary>
    private IEnumerator OnCleared()
    {
        while (!Clear())
            yield return new WaitForSeconds(0.5f);

        Destroy(gameObject);
    }

    /// <summary>
    /// Triggers the encounter if not triggered already and the collider is the player
    /// </summary>
    /// <param name="other">The other collider to check collision</param>
    private void OnTriggerEnter(Collider other)
    {
        if (!triggered && other.tag == "Player")
            TriggerEncounter();
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
