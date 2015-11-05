using UnityEngine;
using UnityEngine.UI;

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

    // Object Variables
    private ProgressBar progressBar;
    private GameObject seed;

    private void Awake()
    {
        // Player HUD Event Listeners
        Player.HealthUpdate += UpdatePlayerHealth;
        Player.ProgressBar += UpdateProgressBar;
        Player.ItemHold += UpdatePlayerItems;        
    }

    // EVENT HANDLERS

    // Player Health
    private void UpdatePlayerHealth(float healthRatio)
    {
        health.value = healthRatio;
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