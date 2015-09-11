using UnityEngine;

public abstract class AbstractMultiWorld : MonoBehaviour
{
    // First method invoked when the transition starts
    protected abstract void InitToggleWorlds();
    // Finishes the world transition
    protected abstract void ToggleWorlds();
}