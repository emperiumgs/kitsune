using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager m_Instance;
    private static bool onTransition;

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

    public static float transitionTime = 1.5f;

    // Customizeable Variables
    [SerializeField]
    private GameObject humanPrefab;
    [SerializeField]
    private GameObject foxPrefab;

    // Reference Variables
    private Camera cam
    {
        get { return Camera.main; }
    }
    private Transform camPivot
    {
        get { return cam.transform.parent; }
    }
    private GameObject camRig
    {
        get { return camPivot.parent.gameObject; }
    }
    private GameObject human;
    private GameObject fox;
    private AbstractPlayer player(GameObject origin)
    {
        return origin.GetComponent<AbstractPlayer>();
    }
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

        fox = Instantiate(foxPrefab);
        human = Instantiate(humanPrefab);
        player(fox).otherForm = human;
        player(human).otherForm = fox;
    }

    private void BroadcastToggleWorlds()
    {
        for (int i = 0; i < multiWorld.Length; i++)
        {
            multiWorld[i].SendMessage("InitToggleWorlds");
        }
    }
}