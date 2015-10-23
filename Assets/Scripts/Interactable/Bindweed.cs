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
        get { return GetComponent<MeshRenderer>(); }
    }
    private BoxCollider col
    {
        get { return GetComponent<BoxCollider>(); }
    }

    // Object Variables
    [HideInInspector]
    public bool growable;
    private bool growed;

    /// <summary>
    /// Toggles the climbing state of the player
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
            if (Input.GetButtonDown("Action"))
                other.SendMessage("ToggleClimb", col);
        }
    }

    /// <summary>
    /// Makes the bindweed slowly appears, and then enables interaction with it
    /// </summary>
    private IEnumerator Grow()
    {
        float time = 0;
        ren.enabled = true;
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
