using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager m_Instance;
    public static GameManager instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = FindObjectOfType<GameManager>();
                DontDestroyOnLoad(m_Instance.gameObject);
            }
            return m_Instance;
        }
    }

    // Customizeable Variables
    [SerializeField]
    private GameObject humanPrefab;

    // Reference Variables
    private AbstractMultiWorld[] multiWorld
    {
        get { return FindObjectsOfType<AbstractMultiWorld>(); }
    }

    private void Awake()
    {
        if (m_Instance == null)
        {
            m_Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            if (this != m_Instance)
                Destroy(gameObject);
        }

        Instantiate(humanPrefab);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
            BroadcastToggleWorlds("AbortToggleWorlds");
    }

    /// <summary>
    /// Broadcasts a ToggleWorlds Event to all Multi-World objects
    /// </summary>
    /// <param name="eventName">InitToggleWorlds if initiating or AbortToggleWorlds if aborting process</param>
    private void BroadcastToggleWorlds(string eventName)
    {
        int max = multiWorld.Length;
        for (int i = 0; i < max; i++)
        {
            multiWorld[i].SendMessage(eventName);
        }
    }
}