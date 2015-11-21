using UnityEngine;

public class SoundtrackTrigger : Trigger
{
    // Assignment Variables
    [Header("If blank, fade out")]
    public string track;
    [Range(0, 5f)]
    public float playDelay;

    // Reference Variables
    private SoundtrackController stController
    {
        get { return FindObjectOfType<SoundtrackController>(); }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if (track != "")
                stController.SwitchToTrack(track, playDelay);
            else
                stController.FadeOut(playDelay);

            Destroy(this);
        }
    }
}
