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

    /*private void Update()
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
                camForward = Vector3.Scale(cam.transform.forward, new Vector3(1, 0, 1)).normalized;
                print(camForward);
                move = v * camForward + h * cam.transform.right;

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
    }*/

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
