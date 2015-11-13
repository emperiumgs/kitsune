using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]
public class SpawnPoint : Trigger
{
	private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            other.GetComponent<Player>().spawnPoint = transform;
            GetComponent<BoxCollider>().enabled = false;
        }
    }
}
