using UnityEngine;
using System.Collections;

public class SeedBush : MonoBehaviour
{
	private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
            if (Input.GetButtonDown("Attack"))
                other.SendMessage("TakeSeed");
        }
    }
}
