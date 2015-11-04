using UnityEngine;
using System.Collections;
using UnityStandardAssets.ImageEffects;

public class ThirdPersonCamera : AbstractMultiWorld
{
    // Customizeable Variables
    public Vector3 offsetVector = new Vector3(0, 1.5f, -2.5f);
    public float offsetSpeed = 6f;
    public float resetTime = 5f;
    [Header("Transition Variables")]
    [Range(0.1f, 5)]
    public float bloomTargetIntensity = 0.8f;
    [Range(0.1f, 3)]
    public float bloomTime = 0.5f;
    [Range(0.1f, 1)]
    public float colorTargetSaturation = 0.6f;
    [Range(0.1f, 1)]
    public float targetBlueValue = 0.4f;
    [Range(0.1f, 1)]
    public float targetRedValue = 0.7f;
    public float vortexAngle = 20;

    // Reference Variables
    private Transform pivot
    {
        get { return transform.parent; }
    }
    private Transform target
    {
        get { return FindObjectOfType<Player>().transform; }
    }
    // Transition
    private Bloom bloom
    {
        get { return GetComponent<Bloom>(); }
    }
    private ColorCorrectionCurves colorCurves
    {
        get { return GetComponent<ColorCorrectionCurves>(); }
    }
    private Vortex vortex
    {
        get { return GetComponent<Vortex>(); }
    }

    // Object Variables
    private bool m_MouseOriented;

    // Public Reference Variables
    public bool mouseOriented
    {
        get { return m_MouseOriented; }
    }

    private void Start()
    {
        transform.position = target.position + target.TransformDirection(offsetVector);
        transform.rotation = Quaternion.Euler(0, target.eulerAngles.y, 0);
    }

    private void LateUpdate()
    {
        ControlRotation();        
    }

    private void FixedUpdate()
    {
        UpdatePivot();

        transform.localPosition = offsetVector;

        Vector3 newPos = Vector3.zero;
        // check to see if there is anything behind the target
        RaycastHit hit;
        Vector3 dir = (transform.position - target.position - Vector3.up / 2).normalized;

        Debug.DrawRay(target.position + Vector3.up / 2, dir, Color.red);

        // cast the bumper ray out from rear and check to see if there is anything behind
        if (Physics.SphereCast(target.position + Vector3.up / 2, 0.3f, dir, out hit, offsetVector.magnitude) && hit.transform != target && !hit.collider.isTrigger)
        {
            newPos.z = transform.InverseTransformPoint(hit.point).z;
            transform.localPosition += newPos;
        }
    }

    /// <summary>
    /// Updates the pivot position to match the target's
    /// </summary>
    private void UpdatePivot()
    {
        pivot.position = Vector3.Lerp(pivot.position, target.position, Time.deltaTime * offsetSpeed);
        // Smoother rotation
        //pivot.rotation = Quaternion.Lerp(pivot.rotation, target.rotation, Time.deltaTime * offsetSpeed);
        //if (!m_MouseOriented)
        //    pivot.rotation = target.rotation;
    }

    /// <summary>
    /// Controls the camera rotation both by mouse and automatic
    /// </summary>
    private void ControlRotation()
    {
        float x = Input.GetAxis("MouseX");
        float y = Input.GetAxis("MouseY");

            float vRot = pivot.localEulerAngles.x + y * -4;
            if (vRot > 180)
                vRot -= 360;
            vRot = Mathf.Clamp(vRot, -30f, 15f);
            pivot.localEulerAngles = new Vector3(vRot, pivot.localEulerAngles.y + x * 4);

        transform.rotation = Quaternion.LookRotation(pivot.position - transform.position);
        transform.localEulerAngles = new Vector3(10, transform.localEulerAngles.y, 0);
    }

    // MULTI-WORLD FUNCTIONS

    protected override void InitToggleWorlds()
    {
        base.InitToggleWorlds();
        if (spiritRealm)
            StartCoroutine(Unsoulify());
        else
        {
            bloom.enabled = true;
            colorCurves.enabled = true;
            StartCoroutine(Soulify());
        }
        vortex.enabled = true;
    }

    protected override void AbortToggleWorlds()
    {
        base.AbortToggleWorlds();
        StopAllCoroutines();

        // Vortex disable
        vortex.enabled = false;
        vortex.angle = 0;

        // Color correction curves keyframes
        Keyframe blueKey = colorCurves.blueChannel.keys[0];
        Keyframe redKey = colorCurves.redChannel.keys[1];

        // Auxiliar variable
        int spirit;

        if (spiritRealm)
        {
            spirit = 1;
        }
        else
        {
            spirit = 0;
            bloom.enabled = false;
            colorCurves.enabled = false;
        }

        // Get back to spirit bloom intensity
        bloom.bloomIntensity = spirit * bloomTargetIntensity;
        // Get back to spirit color correction curves values
        colorCurves.saturation = 1 - spirit * (1 - colorTargetSaturation);
        blueKey.value = spirit * targetBlueValue;
        redKey.value = targetRedValue - spirit * (1 - targetRedValue);
        colorCurves.blueChannel.MoveKey(0, blueKey);
        colorCurves.redChannel.MoveKey(1, redKey);
        colorCurves.UpdateParameters();
    }

    protected override void ToggleWorlds()
    {
        StartCoroutine(SmoothBloom(spiritRealm));
        if (spiritRealm)
            colorCurves.enabled = false;

        transform.position = target.position + target.TransformDirection(offsetVector);

        base.ToggleWorlds();
    }

    private IEnumerator Soulify()
    {
        float time = 0;
        float maxTime = transitionTime;
        float percentage;

        float prevSat = colorCurves.saturation;

        Keyframe blueKey = colorCurves.blueChannel.keys[0];
        Keyframe redKey = colorCurves.redChannel.keys[1];

        while (onTransition && time < maxTime)
        {
            time += Time.deltaTime;
            percentage = time / maxTime;
            // Vortex Effect
            if (time < maxTime / 2)
                vortex.angle += Time.deltaTime * vortexAngle;
            else
                vortex.angle -= Time.deltaTime * vortexAngle;
            // Color Correction Curves Effect
            colorCurves.saturation = prevSat - percentage * (1 - colorTargetSaturation);
            // Update the blue color curve
            blueKey.value = percentage * targetBlueValue;
            redKey.value = 1 - percentage * (1 - targetRedValue);
            colorCurves.blueChannel.MoveKey(0, blueKey);
            colorCurves.redChannel.MoveKey(1, redKey);
            colorCurves.UpdateParameters();
            // Bloom effect
            bloom.bloomIntensity = 2 * bloomTargetIntensity * percentage;
            yield return null;
        }

        if (onTransition)
            ToggleWorlds();
    }

    private IEnumerator Unsoulify()

    {
        float time = 0;
        float maxTime = transitionTime;
        float percentage;

        float prevSat = colorCurves.saturation;

        Keyframe blueKey = colorCurves.blueChannel.keys[0];
        Keyframe redKey = colorCurves.redChannel.keys[1];

        float prevBloom = bloom.bloomIntensity;

        while (onTransition && time < maxTime)
        {
            time += Time.deltaTime;
            percentage = time / maxTime;
            // Vortex Effect
            if (time < maxTime / 2)
                vortex.angle += Time.deltaTime * vortexAngle;
            else
                vortex.angle -= Time.deltaTime * vortexAngle;
            // Color Correction Curves Effect
            colorCurves.saturation = prevSat + (1 - prevSat) * percentage;
            // Update blue color curve
            blueKey.value = targetBlueValue - targetBlueValue * percentage;
            redKey.value = targetRedValue + percentage * (1 - targetRedValue);
            colorCurves.blueChannel.MoveKey(0, blueKey);
            colorCurves.redChannel.MoveKey(1, redKey);
            colorCurves.UpdateParameters();
            // Bloom Effect
            bloom.bloomIntensity = prevBloom + prevBloom * percentage;
            yield return null;
        }

        if (onTransition)
            ToggleWorlds();
    }

    private IEnumerator SmoothBloom(bool toZero)
    {
        float time = 0;
        float maxBloom = bloom.bloomIntensity;
        float subtract = toZero == true ? maxBloom : bloomTargetIntensity;

        while (time < bloomTime)
        {
            time += Time.deltaTime;
            bloom.bloomIntensity = maxBloom - (time / bloomTime) * subtract;
            // Vortex Effect
            if (time < bloomTime / 2)
                vortex.angle -= Time.deltaTime * vortexAngle;
            else
                vortex.angle += Time.deltaTime * vortexAngle;
            yield return null;
        }

        vortex.enabled = false;
        if (toZero)
            bloom.enabled = false;
    }
}
