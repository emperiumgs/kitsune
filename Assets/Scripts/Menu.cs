using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour
{
    /// <summary>
    /// Starts the game from the beggining
    /// </summary>
    public void NewGame()
    {
        Application.LoadLevel(1);
    }

    /// <summary>
    /// Quits the game
    /// </summary>
    public void Quit()
    {
        Application.Quit();
    }
}
