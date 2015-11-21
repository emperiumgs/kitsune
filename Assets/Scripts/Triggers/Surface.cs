using UnityEngine;

public class Surface : Trigger
{
    public Player.Surface enterSurface;
    public Player.Surface exitSurface;

    private bool playable = true;
    private AudioSource source
    {
        get { return GetComponent<AudioSource>(); }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if (enterSurface != Player.Surface.None)
                other.GetComponent<Player>().surface = enterSurface;
            if (playable && source.clip != null)
            {
                playable = false;
                source.pitch = Random.Range(0.95f, 1.05f);
                source.PlayOneShot(source.clip);
                Invoke("TogglePlayable", 0.5f);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            if (exitSurface != Player.Surface.None)
                other.GetComponent<Player>().surface = exitSurface;
        }
    }

    private void TogglePlayable()
    {
        playable = !playable;
    }
}
