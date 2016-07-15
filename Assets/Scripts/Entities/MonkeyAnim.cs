using UnityEngine;
using System.Collections;

public class MonkeyAnim : MonoBehaviour {

    void DoThrow()
    {
        transform.parent.SendMessage("DoThrow");
    }

    void GetFruit()
    {
        transform.parent.SendMessage("GetFruit");
    }
}
