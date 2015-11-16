using UnityEngine;
using System.Collections;

public class Utils
{
    /// <summary>
    /// Calculates the necessary rigidbody velocity to reach a certain position, with a given maxHeight
    /// </summary>
    /// <param name="targetX">The target x position</param>
    /// <param name="maxHeight">The maximum height to reach</param>
    /// <param name="targetZ">The target z position</param>
    /// <returns>The required velocity to reach the destination</returns>
    public static Vector3 RigidbodySpeedTo(float targetX, float maxHeight, float targetZ)
    {
        float g = Physics.gravity.magnitude;
        float ySpeed = Mathf.Sqrt(2 * g * (maxHeight >= 1 ? maxHeight : 1));
        float time = 2 * ySpeed / g;
        float xSpeed = targetX / time;
        float zSpeed = targetZ / time;
        return new Vector3(xSpeed, ySpeed, zSpeed);
    }

    /// <summary>
    /// Finds an specified value of bool in an array
    /// </summary>
    /// <param name="array">The array to check</param>
    /// <param name="desiredValue">The desired bool value</param>
    /// <returns>True if value is found</returns>
    public static bool FindBool(bool[] array, bool desiredValue)
    {        
        foreach(bool value in array)
        {
            if (value == desiredValue)
                return true;
        }
        return false;
    }

    public static void PrintArray<T>(T[] array)
    {
        foreach (T item in array)
            Debug.Log(item.ToString());
    }
}
