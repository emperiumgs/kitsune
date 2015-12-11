using UnityEngine;
using System.Collections;

public class Hack : MonoBehaviour
{
    public Player player;
    public Transform tp1;
    public Transform tp2;
    public Transform tp3;
    public Transform tp4;
    public Transform tp5;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            player.health = Player.MAX_HEALTH;
        if (Input.GetKeyDown(KeyCode.Alpha2))
            player.health = 1;
        if (Input.GetKeyDown(KeyCode.Alpha3))
            Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Ignore Raycast"), LayerMask.NameToLayer("Player"), true);
        if (Input.GetKeyDown(KeyCode.Alpha4))
            player.transform.position = tp1.transform.position;
        if (Input.GetKeyDown(KeyCode.Alpha5))
            player.transform.position = tp2.transform.position;
        if (Input.GetKeyDown(KeyCode.Alpha6))
            player.transform.position = tp3.transform.position;
        if (Input.GetKeyDown(KeyCode.Alpha7))
            player.transform.position = tp4.transform.position;
        if (Input.GetKeyDown(KeyCode.Alpha8))
            player.transform.position = tp5.transform.position;
    }
}