using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIController : MonoBehaviour
{
    // Fade enum
    public enum FadeType
    {
        In, // Clear to Black
        Out, // Black to Clear
        InOut // Clear to Black to Clear
    }

    // Customizeable Variables
    public GameObject progressBarPrefab;
    public GameObject seedPrefab;
    public Sprite comHealth;
    public Sprite lowHealth;
    public Sprite seedUI;

    // Reference Variables
    private GameObject gameInterface
    {
        get { return transform.FindChild("GameInterface").gameObject; }
    }
    private GameObject animInterface
    {
        get { return transform.FindChild("CinematicInterface").gameObject; }
    }
    private Transform health
    {
        get { return gameInterface.transform.FindChild("Health").FindChild("Fill"); }
    }
    private Image animFader
    {
        get { return animInterface.transform.FindChild("Fader").GetComponent<Image>(); }
    }
    private Image fader
    {
        get { return gameInterface.transform.FindChild("Fader").GetComponent<Image>(); }
    }

    // Object Variables
    private ProgressBar progressBar;
    private GameObject seed;

    private void Awake()
    {
        // Player HUD Event Listeners
        Player.HealthUpdate += UpdatePlayerHealth;
        Player.Death += PlayerDeath;
        Player.ProgressBar += UpdateProgressBar;
        Player.ItemHold += UpdatePlayerItems;
    }

    /// <summary>
    /// Toggles the appearing UI between cinematic and game
    /// </summary>
    public void ToggleCinematic(bool toCinematic)
    {
        gameInterface.SetActive(!toCinematic);
        animInterface.SetActive(toCinematic);
    }

    /// <summary>
    /// Enables the cinematic fader with options
    /// </summary>
    /// <param name="fadeTime">The duration of the fade</param>
    /// <param name="type">The fade type</param>
    /// <param name="origin">The origin cinematic to respond</param>
    public void CinematicFade(float fadeTime, FadeType type, Cinematic origin)
    {
        if (type != FadeType.InOut)
            origin.SendMessage("OnTransition");
        StartCoroutine(CinematicFading(fadeTime, type, origin));
    }
    private IEnumerator CinematicFading(float fadeTime, FadeType type, Cinematic origin)
    {
        float time = 0;
        animFader.gameObject.SetActive(true);
        animFader.color = type == FadeType.Out ? Color.black : Color.clear;
        if (type == FadeType.InOut)
            fadeTime /= 2;

        while (time < fadeTime)
        {
            time += Time.deltaTime;
            animFader.color = type == FadeType.Out ? Color.black * (1 - time / fadeTime) : Color.black * time / fadeTime;

            if (type == FadeType.InOut && time >= fadeTime)
            {
                type = FadeType.Out;
                time = 0;
                origin.SendMessage("OnTransition");
            }
            yield return null;
        }

        if (origin != null)
            origin.SendMessage("OnComplete");
    }

    // EVENT HANDLERS

    // Player Health
    private void UpdatePlayerHealth(float healthRatio)
    {
        health.localScale = healthRatio * (Vector3.one * 0.75f) + (Vector3.one * 0.25f);
        if (healthRatio < 0.3f)
            health.GetComponent<Image>().sprite = lowHealth;
        else
            health.GetComponent<Image>().sprite = comHealth;

        if (healthRatio == 0)
            health.gameObject.SetActive(false);
        else
            health.gameObject.SetActive(true);
    }

    // Player Death
    private void PlayerDeath(Player dead)
    {
        StartCoroutine(DeathFade(dead));
    }
    private IEnumerator DeathFade(Player dead)
    {
        fader.gameObject.SetActive(true);
        float fadeTime = 3f;
        float time = 0;

        // Fade to white
        while (time < fadeTime)
        {
            time += Time.deltaTime;
            fader.color = new Color(1, 1, 1, time / fadeTime);
            yield return null;
        }
        dead.SendMessage("Respawn");
        time = 0;
        // Fade to clear
        while (time < fadeTime)
        {
            time += Time.deltaTime;
            fader.color = new Color(1, 1, 1, (1 - time / fadeTime));
            yield return null;
        }
        fader.gameObject.SetActive(false);
    }

    // Progress Bar
    private void UpdateProgressBar(ProgressEventArgs progressEvent)
    {
        if (progressEvent == null)
            Destroy(progressBar.gameObject, 0.1f);
        else
        {
            progressBar = Instantiate(progressBarPrefab).GetComponent<ProgressBar>();
            progressBar.transform.SetParent(gameInterface.transform, false);
            progressBar.text = progressEvent.text;
            progressBar.StartCoroutine("Running", progressEvent.timer);
        }
    }

    // Player Items
    private void UpdatePlayerItems(ItemEventArgs itemEvent)
    {
        if (itemEvent.get)
        {
            if (itemEvent.itemName == "seed")
            {
                seed = Instantiate(seedPrefab);
                seed.transform.SetParent(gameInterface.transform, false);
                seed.GetComponent<Image>().sprite = seedUI;
            }
        }
        else
        {
            if (itemEvent.itemName == "seed")
                Destroy(seed);
        }
    }
}