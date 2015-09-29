using UnityEngine;
using System.Collections;

public class HostileBranch : MonoBehaviour
{
    // Static Variables
    public static float rotationSpeed = 200f;

    // Customizeable Variables
    [Range(0, 90f)]
    public float maxAngle = 45f;
    [Range(-90f, 0)]
    public float minAngle = -45f;

    private void Awake()
    {
        transform.rotation = Quaternion.AngleAxis(Random.Range(minAngle, maxAngle), Vector3.right);

        bool random = true;
        if (Random.value > 0.5f)
            random = false;

        StartCoroutine(Rotate(random));
    }

    private IEnumerator Rotate(bool onward)
    {        
        float initX = transform.eulerAngles.x;
        Vector3 curAngles = Vector3.zero;

        curAngles.x = initX > 270 ? initX - 360 : initX;

        if (onward)
        {
            while (curAngles.x < maxAngle)
            {
                curAngles.x += Time.deltaTime * rotationSpeed;
                transform.rotation = Quaternion.Euler(curAngles);
                yield return null;
            }
        }
        else
        {
            while (curAngles.x > minAngle)
            {
                curAngles.x -= Time.deltaTime * rotationSpeed;
                transform.rotation = Quaternion.Euler(curAngles);
                yield return null;
            }
        }

        StartCoroutine(Rotate(!onward));
    }
}
