using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class SoundtrackController : MonoBehaviour
{
    public const string TRACK_OUTDOOR = "outdoor";
    public const string TRACK_INDOOR = "indoor";
    public const string TRACK_BATTLE = "battle";

    // Assignment Variables
    public AudioMixerSnapshot defaultSnapshot;
    public AudioMixerSnapshot muteSnapshot;
    public AudioMixerSnapshot outdoorSnapshot;
    public AudioMixerSnapshot indoorSnapshot;
    public AudioMixerSnapshot battleSnapshot;

    // Object Variables
    private AudioSource outSource;
    private AudioSource inSource;
    private AudioSource batSource;

    private void Awake()
    {
        AudioSource[] sources = GetComponents<AudioSource>();
        outSource = sources[0];
        inSource = sources[1];
        batSource = sources[2];
    }

	public void FadeOut(float delay)
    {
        muteSnapshot.TransitionTo(delay);
    }

    public void SwitchToTrack(string track, float delay)
    {
        switch (track)
        {
            case TRACK_OUTDOOR:
                outSource.time = 0;
                outdoorSnapshot.TransitionTo(delay);
                break;
            case TRACK_INDOOR:
                inSource.time = 0;
                indoorSnapshot.TransitionTo(delay);
                break;
            case TRACK_BATTLE:
                batSource.time = 0;
                battleSnapshot.TransitionTo(delay);
                break;
        }
    }
}
