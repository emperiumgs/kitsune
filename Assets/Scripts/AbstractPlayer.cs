using UnityEngine;
using UnityStandardAssets.ImageEffects;
using System.Collections;
using System;

public abstract class AbstractPlayer : AbstractMultiWorld
{
    // Customizable Variables
    [Header("Player Variables")]
    [SerializeField]
    protected float movingTurnSpeed = 360;
    [SerializeField]
    protected float stationaryTurnSpeed = 180;
    [SerializeField]
    protected float jumpPower = 8f;
    [Range(1f, 4f)]
    [SerializeField]
    protected float gravityMultiplier = 2f;
    [SerializeField]
    protected float runCycleLegOffset = 0.2f;
    [SerializeField]
    protected float moveSpeedMultiplier = 1f;
    [SerializeField]
    protected float animSpeedMultiplier = 1f;
    [SerializeField]
    protected float groundCheckDistance = 0.3f;
    [SerializeField]
    protected GameObject barPrefab;
    [SerializeField]
    protected Vector3 transCamPivotOffset = Vector3.up / 2;
    [SerializeField]
    protected float transCamOffset = 2.5f;

    // Reference Variables (read-only)
    protected GameManager manager
    {
        get { return GameManager.instance; }
    }
    protected Camera cam
    {
        get { return Camera.main; }
    }
    protected Transform camPivot
    {
        get { return cam.transform.parent; }
    }
    protected GameObject camRig
    {
        get { return camPivot.parent.gameObject; }
    }
    protected Bloom camBloom
    {
        get { return cam.GetComponent<Bloom>(); }
    }
    protected ColorCorrectionCurves camColor
    {
        get { return cam.GetComponent<ColorCorrectionCurves>(); }
    }
    protected CapsuleCollider col
    {
        get { return GetComponent<CapsuleCollider>(); }
    }
    protected Animator anim
    {
        get { return GetComponent<Animator>(); }
    }
    protected Rigidbody rb
    {
        get { return GetComponent<Rigidbody>(); }
    }

    // Object Variables
    protected ProgressBar castingBar;
    protected Vector3 camForward;
    protected Vector3 move;
    protected Vector3 groundNormal;
    protected Vector3 colCenter;
    protected bool isGrounded;
    protected bool isCrouching;
    protected bool inTransition;
    protected bool inactive;
    protected bool jump;
    protected float colHeight;
    protected float origGroundCheckDist;
    protected float turnAmount;
    protected float forwardAmount;

    // Public Object variables
    public bool grounded
    {
        get { return isGrounded; }
    }
    public bool crouching
    {
        get { return isCrouching; }
    }
    [HideInInspector]
    public GameObject otherForm;

    protected virtual void Awake()
    {
        colHeight = col.height;
        colCenter = col.center;
        origGroundCheckDist = groundCheckDistance;
    }

    protected virtual void Update()
    {
        if (!inactive)
        {
            if (!jump)
                jump = Input.GetButtonDown("Jump");

            if (!isCrouching && isGrounded && !inTransition && Input.GetButtonDown("Toggle Worlds"))
            {
                manager.SendMessage("BroadcastToggleWorlds");
            }
        }
    }

    protected virtual void FixedUpdate()
    {
        if (!inactive)
        {
            // read inputs
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            bool crouch = Input.GetKey(KeyCode.C);

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
            Move(move, crouch, jump);
            jump = false;
        }
        else
        {
            Move(Vector3.zero, false, false);
        }
    }

    protected void Move(Vector3 move, bool crouch, bool jump)
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
            HandleGroundedMovement(crouch, jump);
        }
        else
        {
            HandleAirborneMovement();
        }

        ScaleCapsuleForCrouching(crouch);
        PreventStandingInLowHeadroom();

        // send input and other state parameters to the animator
        UpdateAnimator(move);
    }

    protected void ScaleCapsuleForCrouching(bool crouch)
    {
        if (isGrounded && crouch)
        {
            if (isCrouching) return;
            col.height /= 2;
            col.center /= 2;
            isCrouching = true;
        }
        else
        {
            Ray crouchRay = new Ray(rb.position + Vector3.up * col.radius / 2, Vector3.up);
            float crouchRayLength = colHeight - col.radius / 2;
            if (Physics.SphereCast(crouchRay, col.radius / 2, crouchRayLength))
            {
                isCrouching = true;
                return;
            }
            col.height = colHeight;
            col.center = colCenter;
            isCrouching = false;
        }
    }

    protected void PreventStandingInLowHeadroom()
    {
        // prevent standing up in crouch-only zones
        if (!isCrouching)
        {
            Ray crouchRay = new Ray(rb.position + Vector3.up * col.radius / 2, Vector3.up);
            float crouchRayLength = colHeight - col.radius / 2;
            if (Physics.SphereCast(crouchRay, col.radius / 2, crouchRayLength))
            {
                isCrouching = true;
            }
        }
    }

    protected virtual void UpdateAnimator(Vector3 move)
    {
        // update the animator parameters
        anim.SetFloat("Forward", forwardAmount, 0.1f, Time.deltaTime);
        anim.SetFloat("Turn", turnAmount, 0.1f, Time.deltaTime);
        anim.SetBool("Crouch", isCrouching);
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

    protected void HandleAirborneMovement()
    {
        // apply extra gravity from multiplier:
        Vector3 extraGravityForce = (Physics.gravity * gravityMultiplier) - Physics.gravity;
        rb.AddForce(extraGravityForce);

        groundCheckDistance = rb.velocity.y < 0 ? origGroundCheckDist : 0.01f;
    }

    protected void HandleGroundedMovement(bool crouch, bool jump)
    {
        // check whether conditions are right to allow a jump:
        if (jump && !crouch && anim.GetCurrentAnimatorStateInfo(0).IsName("Grounded"))
        {
            // jump!
            rb.velocity = new Vector3(rb.velocity.x, jumpPower, rb.velocity.z);
            isGrounded = false;
            anim.applyRootMotion = false;
            groundCheckDistance = 0.1f;
        }
    }

    protected void ApplyExtraTurnRotation()
    {
        // help the character turn faster (this is in addition to root rotation in the animation)
        float turnSpeed = Mathf.Lerp(stationaryTurnSpeed, movingTurnSpeed, forwardAmount);
        transform.Rotate(0, turnAmount * turnSpeed * Time.deltaTime, 0);
    }

    protected void CheckGroundStatus()
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

    protected void OnAnimatorMove()
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
    /// Prevents player from moving, and starts the world transition
    /// </summary>
    protected override void InitToggleWorlds()
    {
        inTransition = true;
        inactive = true;
        forwardAmount = 0;
        turnAmount = 0;
        rb.isKinematic = true;
        castingBar = Instantiate(barPrefab).GetComponent<ProgressBar>();
        castingBar.text = "CASTING";
        StartCoroutine(OnToggleWorlds());
    }

    /// <summary>
    /// Finishes the world transition process
    /// </summary>
    protected override void ToggleWorlds()
    {
        if (otherForm != null)
        {
            otherForm.transform.position = transform.position;
            otherForm.transform.rotation = transform.rotation;
            gameObject.SetActive(false);
            otherForm.SetActive(true);
        }

        rb.isKinematic = false;
        Destroy(castingBar.gameObject);
        inactive = false;
        inTransition = false;

        // Camera re-orientation
        camPivot.position += transCamPivotOffset;
        cam.transform.position = new Vector3(0, 0, -transCamOffset);
        camRig.SendMessage("ResetOrigDist", transCamOffset);
    }

    protected virtual IEnumerator OnToggleWorlds()
    {
        float time = 0;
        float maxTime = GameManager.transitionTime;

        while (time < maxTime)
        {
            time += Time.deltaTime;
            castingBar.curSize = new Vector2(castingBar.totalSize.x * time/maxTime, castingBar.totalSize.y);
            yield return null;
        }

        ToggleWorlds();
    }
}
