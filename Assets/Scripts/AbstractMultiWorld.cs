using UnityEngine;

public abstract class AbstractMultiWorld : MonoBehaviour
{
    // Static Properties
    public static float transitionTime = 1.5f;

    // Object Properties
    protected bool onTransition = false;
    protected bool spiritRealm = false;

    // First method invoked when the transition starts
    protected virtual void InitToggleWorlds()
    {
        onTransition = true;        
    }
    // Cancels the world transition
    protected virtual void AbortToggleWorlds()
    {
        onTransition = false;
    }
    // Finishes the world transition
    protected virtual void ToggleWorlds()
    {
        spiritRealm = !spiritRealm;
        onTransition = false;
    }
}