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
    private float movingTurnSpeed = 360;
    [SerializeField]
    private float stationaryTurnSpeed = 180;
    [SerializeField]
    private float jumpPower = 8f;
    [Range(1f, 4f)]
    [SerializeField]
    private float gravityMultiplier = 2f;
    [SerializeField]
    private float runCycleLegOffset = 0.2f;
    [SerializeField]
    private float moveSpeedMultiplier = 1f;
    [SerializeField]
    private float animSpeedMultiplier = 1f;
    [SerializeField]
    private float groundCheckDistance = 0.3f;
    public GameObject barPrefab;
    public GameObject uiSeedPrefab;
    public Vector3 foxCamPivotOffset = Vector3.down / 2;
    public float foxCamOffset = 1.5f;
    public Vector3 humCamPivotOffset = Vector3.up / 2;
    public float humCamOffset = 2.5f;
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
    private Transform camPivot
    {
        get { return cam.transform.parent; }
    }
    private GameObject camRig
    {
        get { return camPivot.parent.gameObject; }
    }
    private Bloom camBloom
    {
        get { return cam.GetComponent<Bloom>(); }
    }
    private ColorCorrectionCurves camColor
    {
        get { return cam.GetComponent<ColorCorrectionCurves>(); }
    }
    private CapsuleCollider col
    {
        get { return GetComponent<CapsuleCollider>(); }
    }
    private Animator anim
    {
        get { return GetComponent<Animator>(); }
    }
    private Rigidbody rb
    {
        get { return GetComponent<Rigidbody>(); }
    }
    [HideInInspector]
    public bool hasSeed
    {
        get { return seed; }
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
    private Vector3 camForward;
    private Vector3 move;
    private Vector3 groundNormal;
    private bool isGrounded;
    private bool inactive;
    private bool jump;
    private bool climbing;
    private float origGroundCheckDist;
    private float turnAmount;
    private float forwardAmount;
    private Collider targetClimb;
    // Fox Variables
    private List<GameObject> spiritBalls = new List<GameObject>();
    private bool seed;

    // Public Object variables
    public bool grounded
    {
        get { return isGrounded; }
    }

    private void Awake()
    {
        origGroundCheckDist = groundCheckDistance;
    }

    private void Update()
    {
        if (!inactive)
        {
            if (!jump)
                jump = Input.GetButtonDown("Jump");

            if (isGrounded && !climbing && !onTransition && Input.GetButtonDown("Toggle Worlds"))
            {
                manager.SendMessage("BroadcastToggleWorlds", "InitToggleWorlds");
            }

            if (spiritRealm)
            {
                if (spiritBalls.Count > 0 && Input.GetButtonDown("Attack"))
                    ShootSpiritBall();
            }
        }
    }

    private void FixedUpdate()
    {
        if (!inactive)
        {
            // read inputs
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            if (!climbing)
            {
                // calculate move direction to pass to character
                if (cam != null)
                {
                    // calculate camera relative direction to move:
                    camForward = Vector3.Scale(cam.transform.forward, new Vector3(1, 0, 1)).normalized;
                    move = v * camForward + h * cam.transform.right;
                }
                else
                {
                    // we use world-relative directions in the case of no main camera
                    move = v * Vector3.forward + h * Vector3.right;
                }

                // walk speed multiplier
                if (Input.GetKey(KeyCode.LeftShift)) move *= 0.5f;

                // pass all parameters to the character control script
                Move(move, jump);
                jump = false;
            }
            else
            {
                move = h * Vector3.right + v * Vector3.up;
                move = transform.TransformDirection(move);
                move *= Time.deltaTime;
                move += transform.position;
                if (targetClimb.bounds.Contains(move))
                    rb.MovePosition(move);
                else
                    ToggleClimb(null);
            }
        }
        else
        {
            Move(Vector3.zero, false);
        }
    }

    private void Move(Vector3 move, bool jump)
    {
        // convert the world relative moveInput vector into a local-relative
        // turn amount and forward amount required to head in the desired
        // direction.
        if (move.magnitude > 1f) move.Normalize();
        move = transform.InverseTransformDirection(move);
        CheckGroundStatus();
        move = Vector3.ProjectOnPlane(move, groundNormal);
        turnAmount = Mathf.Atan2(move.x, move.z);
        forwardAmount = move.z;

        ApplyExtraTurnRotation();

        // control and velocity handling is different when grounded and airborne:
        if (isGrounded)
        {
            HandleGroundedMovement(jump);
        }
        else
        {
            HandleAirborneMovement();
        }

        // send input and other state parameters to the animator
        UpdateAnimator(move);
    }

    private void UpdateAnimator(Vector3 move)
    {
        // update the animator parameters
        anim.SetFloat("Forward", forwardAmount, 0.1f, Time.deltaTime);
        anim.SetFloat("Turn", turnAmount, 0.1f, Time.deltaTime);
        anim.SetBool("OnGround", isGrounded);
        if (!isGrounded)
        {
            anim.SetFloat("Jump", rb.velocity.y);
        }

        // calculate which leg is behind, so as to leave that leg trailing in the jump animation
        // (This code is reliant on the specific run cycle offset in our animations,
        // and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
        float runCycle =
            Mathf.Repeat(
                anim.GetCurrentAnimatorStateInfo(0).normalizedTime + runCycleLegOffset, 1);
        float jumpLeg = (runCycle < 0.5 ? 1 : -1) * forwardAmount;
        if (isGrounded)
        {
            anim.SetFloat("JumpLeg", jumpLeg);
        }

        // the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector,
        // which affects the movement speed because of the root motion.
        if (isGrounded && move.magnitude > 0)
        {
            anim.speed = animSpeedMultiplier;
        }
        else
        {
            // don't use that while airborne
            anim.speed = 1;
        }
    }

    private void HandleAirborneMovement()
    {
        // apply extra gravity from multiplier:
        Vector3 extraGravityForce = (Physics.gravity * gravityMultiplier) - Physics.gravity;
        rb.AddForce(extraGravityForce);

        groundCheckDistance = rb.velocity.y < 0 ? origGroundCheckDist : 0.01f;
    }

    private void HandleGroundedMovement(bool jump)
    {
        // check whether conditions are right to allow a jump:
        if (jump && anim.GetCurrentAnimatorStateInfo(0).IsName("Grounded"))
        {
            // jump!
            rb.velocity = new Vector3(rb.velocity.x, jumpPower, rb.velocity.z);
            isGrounded = false;
            anim.applyRootMotion = false;
            groundCheckDistance = 0.1f;
        }
    }

    private void ApplyExtraTurnRotation()
    {
        // help the character turn faster (this is in addition to root rotation in the animation)
        float turnSpeed = Mathf.Lerp(stationaryTurnSpeed, movingTurnSpeed, forwardAmount);
        transform.Rotate(0, turnAmount * turnSpeed * Time.deltaTime, 0);
    }

    private void CheckGroundStatus()
    {
        RaycastHit hitInfo;
#if UNITY_EDITOR
        // helper to visualise the ground check ray in the scene view
        Debug.DrawLine(transform.position + (Vector3.up * 0.1f), transform.position + (Vector3.up * 0.1f) + (Vector3.down * groundCheckDistance));
#endif
        // 0.1f is a small offset to start the ray from inside the character
        // it is also good to note that the transform position in the sample assets is at the base of the character
        if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, groundCheckDistance))
        {
            groundNormal = hitInfo.normal;
            isGrounded = true;
            anim.applyRootMotion = true;
        }
        else
        {
            isGrounded = false;
            groundNormal = Vector3.up;
            anim.applyRootMotion = false;
        }
    }

    private void OnAnimatorMove()
    {
        // we implement this function to override the default root motion.
        // this allows us to modify the positional speed before it's applied.
        if (isGrounded && Time.deltaTime > 0)
        {
            Vector3 v = (anim.deltaPosition * moveSpeedMultiplier) / Time.deltaTime;

            // we preserve the existing y part of the current velocity.
            v.y = rb.velocity.y;
            rb.velocity = v;
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
        rb.useGravity = !climbing;
        rb.isKinematic = climbing;
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
        rb.isKinematic = false;
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
        forwardAmount = 0;
        turnAmount = 0;
        rb.isKinematic = true;
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
        Vector3 camPivotOffset;
        float camOffset;

        // Toggle the active body
        if (spiritRealm)
        {
            // Prototyping purposes
            transform.localScale = Vector3.one;

            for (int i = 0; i < spiritBalls.Count; i++)
                Destroy(spiritBalls[i].gameObject);

            spiritBalls.Clear();

            // Camera adjustment
            camPivotOffset = humCamPivotOffset;
            camOffset = humCamOffset;
        }
        else
        {
            // Prototyping purposes
            transform.localScale = Vector3.one / 2;

            // Camera adjustment
            camPivotOffset = foxCamPivotOffset;
            camOffset = foxCamOffset;

            AddSpiritBall();
        }

        EndTransition();

        // Destroy the seed
        DropSeed();

        // Camera re-orientation
        camPivot.position += camPivotOffset;
        cam.transform.position = new Vector3(0, 0, camOffset);
        camRig.SendMessage("ResetOrigDist", camOffset);

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
