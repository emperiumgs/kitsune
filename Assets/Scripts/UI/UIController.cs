using UnityEngine;

public class UIController : MonoBehaviour
{
    // Customizeable Variables
    public GameObject progressBarPrefab;
    public GameObject seedPrefab;

    // Object Variables
    private ProgressBar progressBar;
    private GameObject seed;

    private void Start()
    {
        Player.ProgressBar += UpdateProgressBar;
        Player.ItemHold += UpdatePlayerItems;
    }    

    // Progress Bar Event Handler
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

    // Player Items Event Handler
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