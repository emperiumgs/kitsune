using UnityEngine;
using System.Collections;

public class SeedPlot : MonoBehaviour
{
    // Customizeable Variables
    public Bindweed bindweed;

    // Object Variables
    private Player player;
    private bool done;

    private void OnTriggerStay(Collider other)
    {
        if (!done && other.tag == "Player")
        {
            player = other.GetComponent<Player>();

            if (player.hasSeed && Input.GetButtonDown("Action"))
            {
                other.SendMessage("DropSeed");
                bindweed.growable = true;           
                done = true;
            }
        }
    }
}
