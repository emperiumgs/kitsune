using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : AbstractMultiWorld
{
    // Player States
    private enum State
    {
        None,
        Default,
        Transitioning,
        Dodging,
        Hit,
    }

    // Player Events
    public delegate void ProgressBarHandler(ProgressEventArgs progressEvent);
    public static event ProgressBarHandler ProgressBar;
    public delegate void ItemHandler(ItemEventArgs itemEvent);
    public static event ItemHandler ItemHold;

    // Customizeable Variables
    [Header("Player Variables")]
    [SerializeField]
    private float speed = 3.5f;
    [SerializeField]
    private float jumpForce = 8.8f;
    [SerializeField]
    private float gravityMultiplier = 0.7f;
    [SerializeField]
    private float rotateSensitivity = 100f;
    //public GameObject barPrefab;
    //public GameObject uiSeedPrefab;
    public Vector3 foxCamOffset = new Vector3(0, 1f, -1.5f);
    public Vector3 humCamOffset = new Vector3(0, 1.5f, -2.5f);
    // Fox Customizeable Variables
    [Header("Fox Variables")]
    public GameObject spiritBallPrefab;
    [SerializeField]
    private Vector3 spiritSlotHeight;
    [SerializeField]
    private Vector3 spiritSlotYOffset;
    [SerializeField]
    private Vector3 spiritSlotXOffset;
    [Range(0.5f, 5)]
    public float spiritRespawnTime = 1;
    [Range(0.1f, 1f)]
    public float dodgeTime = 0.5f;
    [Range(1f, 3f)]
    public float dodgeForce = 2.4f;
    [Range(0.05f, 0.5f)]
    public float dodgeGrav = 0.2f;

    // Reference Variables (read-only)
    private GameManager manager
    {
        get { return GameManager.instance; }
    }
    private Camera cam
    {
        get { return Camera.main; }
    }
    private ThirdPersonCamera camScript
    {
        get { return cam.GetComponent<ThirdPersonCamera>(); }
    }
    private Animator anim
    {
        get { return GetComponent<Animator>(); }
    }
    private CharacterController control
    {
        get { return GetComponent<CharacterController>(); }
    }

    // Fox Reference Variables
    private Vector3 spiritSlotBase
    {
        get { return transform.position + transform.TransformDirection(spiritSlotHeight); }
    }
    private Vector3 spiritSlotLeft
    {
        get { return spiritSlotBase - transform.TransformDirection(2 * spiritSlotYOffset - spiritSlotXOffset); }
    }
    private Vector3 spiritSlotRight
    {
        get { return spiritSlotBase - transform.TransformDirection(2 * spiritSlotYOffset + spiritSlotXOffset); }
    }

    // Object Variables
    private State state;
    private Coroutine current;
    private Vector3 move;
    private bool jump;
    private bool climbing;
    private Collider targetClimb;
    // Fox Variables
    private List<GameObject> spiritBalls = new List<GameObject>();
    private bool seed;

    // Public Object variables
    public bool grounded
    {
        get { return control.isGrounded; }
    }
    public bool hasSeed
    {
        get { return seed; }
    }

    /// <summary>
    /// Initializes the player actions
    /// </summary>
    private void Awake()
    {
        current = StartCoroutine(DefaultUpdate());
    }

    /// <summary>
    /// Handles player action inputs
    /// </summary>
    private void Update()
    {
        if (state == State.Default)
        {
            if (control.isGrounded && !climbing && !onTransition)
            {
                if (Input.GetButtonDown("Toggle Worlds"))
                    manager.SendMessage("BroadcastToggleWorlds", "InitToggleWorlds");

                if (Input.GetButtonDown("Jump"))
                    jump = true;

                if (spiritRealm)
                {
                    if (spiritBalls.Count > 0 && Input.GetButtonDown("Attack"))
                        ShootSpiritBall();

                    int dodge = (int)Input.GetAxis("Dodge");
                    if (dodge != 0)
                    {
                        StopCoroutine(current);
                        current = StartCoroutine(OnDodging(dodge));
                    }
                }
            }
        }
    }

    /// <summary>
    /// Inflicts the specified amount of damage
    /// </summary>
    /// <param name="amount">The amount of damage to inflict</param>
    private void TakeDamage(int amount)
    {
        if (onTransition)
            manager.SendMessage("BroadcastToggleWorlds", "AbortToggleWorlds");

        print("Took " + amount + " damage");
    }

    /// <summary>
    /// Inflicts damage on the player and knocks him
    /// </summary>
    private void BranchHit(Vector3 dir)
    {
        if (state != State.Hit)
        {
            TakeDamage(10);
            StopCoroutine(current);
            current = StartCoroutine(OnBranchHit(dir));
        }
    }

    /// <summary>
    /// Handles player movement
    /// </summary>
    private IEnumerator DefaultUpdate()
    {
        state = State.Default;
        while (state == State.Default)
        {
            // Read Inputs
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            if (!climbing)
            {
                transform.Rotate(h * Vector3.up * Time.deltaTime * rotateSensitivity);

                if (control.isGrounded)
                {
                    Vector3 direction = camScript.mouseOriented ? transform.forward : cam.transform.forward;

                    move = v * direction * speed;

                    if (jump)
                    {
                        move.y = jumpForce;
                        jump = false;
                    }
                }

                move.y -= gravityMultiplier;

                control.Move(move * Time.deltaTime);

                anim.SetBool("OnGround", control.isGrounded);
                if (!control.isGrounded)
                {
                    anim.SetFloat("Jump", move.y);
                }

                if (!onTransition)
                    anim.SetFloat("Forward", transform.InverseTransformDirection(move).z, 0.1f, Time.deltaTime);
                else
                    anim.SetFloat("Forward", 0);
            }
            else
            {
                move = h * Vector3.right + v * Vector3.up;
                move = transform.TransformDirection(move);
                if (move == Vector3.zero || targetClimb.bounds.Contains(transform.position))
                    control.Move(move * Time.deltaTime);
                else
                {
                    control.Move(-targetClimb.transform.up / 2);
                    ToggleClimb(null);
                }
            }
            yield return new WaitForFixedUpdate();
        }
    }

    /// <summary>
    /// Dodge by jumping sideways
    /// </summary>
    /// <param name="dir">The dir given by input to move</param>
    private IEnumerator OnDodging(int dir)
    {
        state = State.Dodging;
        Vector3 target = transform.TransformDirection(dir * Vector3.right);
        float time = 0;
        target.y = dodgeForce * dodgeTime;
        anim.SetBool("OnGround", false);
        while (time < dodgeTime && state == State.Dodging)
        {
            time += Time.deltaTime;
            target.y -= dodgeGrav * dodgeTime;
            anim.SetFloat("Jump", target.y);
            control.Move(target * Time.deltaTime / dodgeTime);
            yield return null;
        }

        current = StartCoroutine(DefaultUpdate());
    }

    /// <summary>
    /// Knocks the player off the surface
    /// </summary>
    private IEnumerator OnBranchHit(Vector3 dir)
    {
        state = State.Hit;
        dir *= 8;
        dir.y = jumpForce / 2;
        print(dir);
        float y = transform.position.y;
        anim.SetBool("OnGround", false);
        while ((dir.y > y || !control.isGrounded) && state == State.Hit)
        {
            dir.y -= gravityMultiplier;
            anim.SetFloat("Jump", dir.y);
            control.Move(dir * Time.deltaTime);
            yield return null;
        }

        current = StartCoroutine(DefaultUpdate());
    }

    // Seed-Bindweed Content

    /// <summary>
    /// Makes the player collect a seed to grow it afterwards
    /// </summary>
    private void TakeSeed()
    {
        if (spiritRealm)
        {
            if (!seed)
            {
                seed = true;
                ItemHold(new ItemEventArgs("seed"));
            }
            // Play sound
        }
    }

    /// <summary>
    /// Drops the seed, destroying its ui feedback
    /// </summary>
    private void DropSeed()
    {
        if (seed)
        {
            ItemHold(new ItemEventArgs("seed", false));
            seed = false;
        }
    }

    /// <summary>
    /// Grabs the bindweed, and stops using gravity
    /// </summary>
    private void ToggleClimb(Collider target)
    {
        climbing = !climbing;
        // Am I climbing?
        if (climbing)
        {
            targetClimb = target;
            Vector3 climbPos = target.transform.position + target.transform.TransformDirection(Vector3.up / 4);
            climbPos.y = transform.position.y + 0.1f;
            transform.LookAt(transform.position + target.transform.TransformDirection(Vector3.down));
            transform.position = climbPos;
        }
    }

    // Spirit Balls Content

    /// <summary>
    /// Adds a new spirit ball on the object
    /// </summary>
    private void AddSpiritBall()
    {
        GameObject spiritBall = Instantiate(spiritBallPrefab);
        spiritBalls.Add(spiritBall);

        int i = spiritBalls.IndexOf(spiritBall);
        spiritBalls[i].transform.parent = transform;
        spiritBalls[i].transform.rotation = transform.rotation;

        RepositionSpiritBalls();
    }

    /// <summary>
    /// Reposition all spirit balls according to their quantity
    /// </summary>
    private void RepositionSpiritBalls()
    {
        switch (spiritBalls.Count)
        {
            case 1:
                spiritBalls[0].transform.position = spiritSlotBase;
                break;
            case 2:
                spiritBalls[0].transform.position = spiritSlotLeft + spiritSlotYOffset;
                spiritBalls[1].transform.position = spiritSlotRight + spiritSlotYOffset;
                break;
            case 3:
                spiritBalls[0].transform.position = spiritSlotLeft;
                spiritBalls[1].transform.position = spiritSlotRight;
                spiritBalls[2].transform.position = spiritSlotBase;
                break;
        }
    }

    /// <summary>
    /// Shoots the spirit ball forward
    /// </summary>
    private void ShootSpiritBall()
    {
        GameObject shooterBall = spiritBalls[spiritBalls.Count - 1];
        spiritBalls.Remove(shooterBall);
        shooterBall.SendMessage("Shoot");

        RepositionSpiritBalls();
        StartCoroutine(RespawnSpiritBall());
    }

    /// <summary>
    /// Respawns a spirit ball after its respawn time
    /// </summary>
    private IEnumerator RespawnSpiritBall()
    {
        yield return new WaitForSeconds(spiritRespawnTime);

        AddSpiritBall();
    }

    // Multi-World Content

    /// <summary>
    /// Ends the transition process, by returning properties to their earlier states
    /// </summary>
    private void EndTransition()
    {
        ProgressBar(null);
        current = StartCoroutine(DefaultUpdate());
    }

    /// <summary>
    /// Prevents player from moving, and starts the world transition
    /// </summary>
    protected override void InitToggleWorlds()
    {
        base.InitToggleWorlds();
        state = State.Transitioning;
        StopCoroutine(current);
        ProgressBar(new ProgressEventArgs("CASTING", transitionTime));
        StartCoroutine(OnToggleWorlds());
    }

    /// <summary>
    /// Aborts the world transition process
    /// </summary>
    protected override void AbortToggleWorlds()
    {
        base.AbortToggleWorlds();
        EndTransition();
        StopCoroutine("OnToggleWorlds");
    }

    /// <summary>
    /// Finishes the world transition process
    /// </summary>
    protected override void ToggleWorlds()
    {
        spirit = !spirit;

        // Toggle the active body
        if (spiritRealm)
        {
            // Prototyping purposes
            transform.localScale = Vector3.one;

            camScript.offsetVector = humCamOffset;

            for (int i = 0; i < spiritBalls.Count; i++)
                Destroy(spiritBalls[i].gameObject);

            spiritBalls.Clear();
        }
        else
        {
            // Prototyping purposes
            transform.localScale = Vector3.one / 2;

            camScript.offsetVector = foxCamOffset;

            AddSpiritBall();
        }

        EndTransition();

        // Destroy the seed
        DropSeed();

        base.ToggleWorlds();
    }

    /// <summary>
    /// Occurs while transitioning worlds
    /// </summary>
    private IEnumerator OnToggleWorlds()
    {
        float time = 0;
        float maxTime = transitionTime;

        while (onTransition && time < maxTime)
        {
            time += Time.deltaTime;
            yield return null;
        }

        if (onTransition)
            ToggleWorlds();
    }
}
