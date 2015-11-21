using UnityEngine;

public class ParticleDestroyer : MonoBehaviour
{
    [Range(0, 30f)]
    public float destroyTimer = 5f;

    private void Awake()
    {
        AudioSource source = GetComponent<AudioSource>();

        if (source != null)
            source.pitch = Random.Range(0.95f, 1.05f);

        Destroy(gameObject, destroyTimer);
    }
}
