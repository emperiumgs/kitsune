using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Menu : MonoBehaviour
{
    /// <summary>
    /// Starts the game from the beggining
    /// </summary>
    public void NewGame()
    {
        SceneManager.LoadScene(2);
    }

    /// <summary>
    /// Quits the game
    /// </summary>
    public void Quit()
    {
        Application.Quit();
    }
}
