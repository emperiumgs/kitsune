using UnityEngine;
using System.Collections;

public class Cinematic : Trigger
{
    [Range(0.5f, 3f)]
    public float fadeTime = 1.5f;
    public int lastPhase = 2;
    public Camera cam;

    private UIController ui
    {
        get { return FindObjectOfType<UIController>(); }
    }
    private Camera m_Cam
    {
        get { return cam == null ? transform.FindChild("Camera").GetComponent<Camera>() : cam; }
    }
    private Animator anim
    {
        get { return m_Cam.GetComponent<Animator>(); }
    }

    private Player player;
    private int phase = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            m_Cam.gameObject.SetActive(true);
            Camera.main.GetComponent<AudioListener>().enabled = false;
            player = other.GetComponent<Player>();
            player.CinematicMode(true);
            ui.ToggleCinematic(true);
            ui.CinematicFade(fadeTime, UIController.FadeType.Out, this);
            GetComponent<BoxCollider>().enabled = false;                  
        }
    }

    private void OnTransition()
    {
        phase++;
        anim.SetInteger("sequence", phase);
        if (phase == lastPhase)
        {
            player.CinematicMode(false);
            Destroy(m_Cam.gameObject);
            Camera.main.GetComponent<AudioListener>().enabled = true;
        }
    }

    private void OnComplete()
    {        
        if (phase == lastPhase)
        {
            ui.ToggleCinematic(false);
            Destroy(this);
        }
        else
            StartCoroutine(UntilDone());
    }

    private void MiddleTransition()
    {
        ui.CinematicFade(fadeTime, UIController.FadeType.InOut, this);
    }

    private IEnumerator UntilDone()
    {
        yield return null;
        while (anim.IsInTransition(0) || anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
            yield return null;

        MiddleTransition();
    }
}
