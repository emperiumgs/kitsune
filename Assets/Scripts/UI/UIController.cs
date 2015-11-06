using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIController : MonoBehaviour
{
    // Customizeable Variables
    public GameObject progressBarPrefab;
    public GameObject seedPrefab;

    // Reference Variables
    private Slider health
    {
        get { return transform.FindChild("Health").GetComponent<Slider>(); }
    }
    private Image fader
    {
        get { return transform.FindChild("Fader").GetComponent<Image>(); }
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

    // EVENT HANDLERS

    // Player Health
    private void UpdatePlayerHealth(float healthRatio)
    {
        health.value = healthRatio;
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
            progressBar.transform.SetParent(transform, false);
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
                seed.transform.SetParent(transform, false);
            }
        }
        else
        {
            if (itemEvent.itemName == "seed")
                Destroy(seed);
        }
    }
}