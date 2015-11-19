using UnityEngine;
using System.Collections;

public class Monkey : MonoBehaviour
{
    private enum State
    {
        None,
        Idle,
        Talking,
        Moving,
        Engaged,
        Dying
    }

    public delegate void BranchesActivationHandler();
    public static event BranchesActivationHandler ActivateBranches;

    public static Monkey main;
    private static bool[] deadClones = new bool[2];
    private static bool doneTalk = false;

    // Customizeable Variables
    public GameObject fruitPrefab;
    public Transform[] spots = new Transform[3];
    [Range(0.5f, 5f)]
    public float throwCooldown = 3f;

    // Object Variables
    private State state;
    private Coroutine current;
    private Player player;

    // Reference Variables
    private Transform origin
    {
        get { return transform.FindChild("Origin"); }
    }
    private Rigidbody rb
    {
        get { return GetComponent<Rigidbody>(); }
    }

    /// <summary>
    /// Triggers default state
    /// </summary>
    private void Start()
    {
        state = State.None;
        if (main == null)
        {
            current = StartCoroutine(IdleUpdate());
            main = this;
        }
    }

    /// <summary>
    /// Triggers the cinematic conversation with the player
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (!doneTalk && state == State.Idle && other.tag == "Player")
        {
            player = other.GetComponent<Player>();
            player.CinematicMode(true);
            current = StartCoroutine(Talk(player.transform));            
        }
    }

    /// <summary>
    /// Engages the player
    /// </summary>
    private void OnTriggerStay(Collider other)
    {
        if (doneTalk && state == State.Idle && other.tag == "Player")
        {
            StopCoroutine(current);
            current = StartCoroutine(EngagedUpdate(other.transform));
        }
    }

    /// <summary>
    /// If the last one, fall from the cloud, else fade away
    /// </summary>
    private void TakeDamage()
    {
        StopCoroutine("ThrowFruit");
        StopCoroutine(current);
        if (Utils.FindBool(deadClones, false))
        {            
            if (deadClones[0] == false)
                deadClones[0] = true;
            else if (deadClones[1] == false)
                deadClones[1] = true;
                        
            current = StartCoroutine(CloneDeath());
        }
        else
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            Destroy(transform.FindChild("Cloud").gameObject);
            current = StartCoroutine(Death());
        }
    }

    /// <summary>
    /// Triggers movement to the desired destination
    /// </summary>
    /// <param name="destiny">The location to move to</param>
    /// <param name="faceIt">Should it face the destination?</param>
    /// <param name="toIdle">Should it return to idle state after?</param>
    public void TriggerMove(Vector3 destiny, bool faceIt, bool toIdle)
    {
        if (current != null)
            StopCoroutine(current);
        current = StartCoroutine(Move(destiny, faceIt, toIdle));
    }

    /// <summary>
    /// Moves to the desired destination and can return to idle state after
    /// </summary>
    /// <param name="destiny">The destination to move</param>
    /// <param name="faceIt">Should it face the destination?</param>
    /// <param name="toIdle">Should it automatically go to the Idle state?</param>
    private IEnumerator Move(Vector3 destiny, bool faceIt, bool toIdle)
    {
        state = State.Moving;
        Vector3 dir = destiny - transform.position;
        dir.Normalize();
        while (Vector3.Distance(transform.position, destiny) > 0.1f)
        {
            if (faceIt)
            {
                transform.LookAt(destiny);
                transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
            }
            transform.position = Vector3.Slerp(transform.position, destiny, Time.deltaTime);
            yield return null;
        }

        if (toIdle)
            current = StartCoroutine(IdleUpdate());
    }

    /// <summary>
    /// Default state update
    /// </summary>
    private IEnumerator IdleUpdate()
    {
        state = State.Idle;
        while (state == State.Idle)
        {
            yield return null;
        }
    }

    /// <summary>
    /// Looks to the player and throw fruits
    /// </summary>
    /// <param name="target">The player transform</param>
    private IEnumerator EngagedUpdate(Transform target)
    {
        state = State.Engaged;
        StartCoroutine("ThrowFruit", target);
        while (state == State.Engaged)
        {
            transform.LookAt(target);
            transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
            yield return null;
        }
    }

    /// <summary>
    /// Throws fruits on the target in regular intervals
    /// </summary>
    /// <param name="target">The target to throw at</param>
    private IEnumerator ThrowFruit(Transform target)
    {
        while (state == State.Engaged)
        {
            yield return new WaitForSeconds(throwCooldown);
            MonkeyFruit fruit = ((GameObject)Instantiate(fruitPrefab, origin.position, origin.rotation)).GetComponent<MonkeyFruit>();
            fruit.ToTarget(target.position);
        }
    }

    /// <summary>
    /// To be implemented
    /// </summary>
    private IEnumerator Death()
    {
        state = State.Dying;
        print("I'm dead, you win");
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    /// <summary>
    /// Fades the clone away
    /// </summary>
    private IEnumerator CloneDeath()
    {
        state = State.Dying;
        GetComponent<BoxCollider>().enabled = false;
        Material mat = GetComponent<MeshRenderer>().material;

        while (state == State.Dying && mat.color.a > 0)
        {
            mat.color -= Color.black * Time.deltaTime / 3;
            yield return null;
        }

        Destroy(gameObject);
    }

    // Cinematic Phase 1
    /// <summary>
    /// Turns to the player and talks to him
    /// </summary>
    /// <param name="target">The player</param>
    private IEnumerator Talk(Transform target)
    {
        state = State.Talking;
        bool talking = true;
        while (talking)
        {
            // Turn to the player
            transform.LookAt(target);
            transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
            // Talk to the player
            print("I like bananas");
            yield return null;
            ActivateBranches();
            // End Phase 1
            talking = false;
        }
        // Phase 2: Split into three
        Monkey clone1 = ((GameObject)Instantiate(gameObject, transform.position, transform.rotation)).GetComponent<Monkey>();
        Monkey clone2 = ((GameObject)Instantiate(gameObject, transform.position, transform.rotation)).GetComponent<Monkey>();
        // Move the clones each to one side
        clone1.TriggerMove(transform.position + transform.TransformDirection(Vector3.right) * 2, false, false);
        clone2.TriggerMove(transform.position + transform.TransformDirection(Vector3.left) * 2, false, false);
        yield return new WaitForSeconds(3f);
        // Phase 3: Talk again
        print("I'm gonna kill you");
        yield return null;
        // Phase 4: Go to final locations and engage combat
        clone1.TriggerMove(spots[0].position, true, true);
        clone2.TriggerMove(spots[1].position, true, true);
        current = StartCoroutine(Move(spots[2].position, true, true));
        doneTalk = true;
        player.CinematicMode(false);
    }
}
