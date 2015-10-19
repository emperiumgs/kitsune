using UnityEngine;
using UnityStandardAssets.ImageEffects;
using System.Collections;
using System.Collections.Generic;
using System;

public class Player : AbstractMultiWorld
{
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
    public GameObject barPrefab;
    public GameObject uiSeedPrefab;
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
    private Bloom camBloom
    {
        get { return cam.GetComponent<Bloom>(); }
    }
    private ColorCorrectionCurves camColor
    {
        get { return cam.GetComponent<ColorCorrectionCurves>(); }
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
    private ProgressBar castingBar;
    private GameObject uiSeed;
    private Vector3 move;
    private bool inactive;
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

    private void Update()
    {
        if (control.isGrounded && !climbing && !onTransition && Input.GetButtonDown("Toggle Worlds"))
            manager.SendMessage("BroadcastToggleWorlds", "InitToggleWorlds");

        if (Input.GetButtonDown("Jump"))
            jump = true;            
    }

    private void FixedUpdate()
    {
        if (!inactive)
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
                if (targetClimb.bounds.Contains(move))
                    control.Move(move * Time.deltaTime);
                else
                    ToggleClimb(null);
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
                uiSeed = Instantiate(uiSeedPrefab) as GameObject;
                uiSeed.transform.SetParent(FindObjectOfType<Canvas>().transform, false);
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
            seed = false;
            Destroy(uiSeed);
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
        Destroy(castingBar.gameObject);
        inactive = false;
    }

    /// <summary>
    /// Prevents player from moving, and starts the world transition
    /// </summary>
    protected override void InitToggleWorlds()
    {
        base.InitToggleWorlds();
        inactive = true;
        castingBar = Instantiate(barPrefab).GetComponent<ProgressBar>();
        castingBar.text = "CASTING";
        StartCoroutine(OnToggleWorlds());
    }

    /// <summary>
    /// Aborts the world transition process
    /// </summary>
    protected override void AbortToggleWorlds()
    {
        base.AbortToggleWorlds();
        EndTransition();
        StopCoroutine(OnToggleWorlds());
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
            if (castingBar != null)
                castingBar.curSize = new Vector2(castingBar.totalSize.x * time / maxTime, castingBar.totalSize.y);
            yield return null;
        }

        if (onTransition)
            ToggleWorlds();
    }
}
