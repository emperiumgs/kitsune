using UnityEngine;
using System.Collections;

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
