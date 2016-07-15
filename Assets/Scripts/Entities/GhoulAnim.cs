using UnityEngine;
using System.Collections;

public class GhoulAnim : MonoBehaviour
{
	void DoAttack()
    {
        transform.parent.SendMessage("DoAttack");
    }

    void EndAttack()
    {
        transform.parent.SendMessage("EndAttack");
    }
}
