using UnityEngine;
using System.Collections;

public class Bindweed : AbstractMultiWorld
{
    // Customizeable Variables
    [Range(0.1f, 5f)]
    public float growTime = 3f;

    // Reference Variables
    private MeshRenderer ren
    {
        get { return GetComponentInParent<MeshRenderer>(); }
    }
    private Collider col
    {
        get { return GetComponent<Collider>(); }
    }

    // Object Variables
    [HideInInspector]
    public bool growable;
    private bool growed;
    private bool climbeable;

    /// <summary>
    /// Toggles the climbing state of the player
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (climbeable && other.tag == "Player")
            other.GetComponent<Player>().ToggleClimb(col);
    }
    private void OnTriggerExit(Collider other)
    {
        if (climbeable && other.tag == "Player")
            other.GetComponent<Player>().ToggleClimb(null);
    }

    /// <summary>
    /// Stops being climbeable for a while
    /// </summary>
    private void Drop()
    {
        climbeable = false;
        StartCoroutine(Cooldown());
    }

    /// <summary>
    /// Makes the bindweed slowly appears, and then enables interaction with it
    /// </summary>
    private IEnumerator Grow()
    {
        float time = 0;
        ren.enabled = true;
        climbeable = true;
        col.enabled = true;
        Color initColor = ren.material.color;

        while (time < growTime)
        {
            time += Time.deltaTime;
            ren.material.color = initColor - (1 - time / growTime) * Color.black;
            yield return null;
        }

        growed = true;        
    }

    /// <summary>
    /// Makes it climbeable again after a short time
    /// </summary>
    private IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(growTime);
        climbeable = true;
    }

    // Multi-World Content

    /// <summary>
    /// Initiates the transition process
    /// </summary>
    protected override void InitToggleWorlds()
    {
        base.InitToggleWorlds();
        StartCoroutine(OnToggleWorlds());
    }

    /// <summary>
    /// Aborts the transition process
    /// </summary>
    protected override void AbortToggleWorlds()
    {
        base.AbortToggleWorlds();
        StopCoroutine("OnToggleWorlds");
    }

    /// <summary>
    /// Will once allow to grow the bindweed
    /// </summary>
    protected override void ToggleWorlds()
    {
        base.ToggleWorlds();
        if (growable && !growed)
            StartCoroutine(Grow());
    }

    /// <summary>
    /// Controls the transition process
    /// </summary>
    private IEnumerator OnToggleWorlds()
    {
        float time = 0;

        while (time < transitionTime)
        {
            time += Time.deltaTime;
            yield return null;
        }

        if (onTransition)
            ToggleWorlds();
    }
}
