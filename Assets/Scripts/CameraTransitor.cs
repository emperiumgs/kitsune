using UnityEngine;
using UnityStandardAssets.ImageEffects;
using System.Collections;

public class CameraTransitor : AbstractMultiWorld
{
    // Customizeable Variables
    [Range(0.1f, 5)]
    [SerializeField]
    private float bloomTargetIntensity = 0.8f;
    [Range(0.1f, 3)]
    [SerializeField]
    private float bloomTime = 0.5f;
    [Range(0.1f, 1)]
    [SerializeField]
    private float colorTargetSaturation = 0.6f;
    [Range(0.1f, 1)]
    [SerializeField]
    private float targetBlueValue = 0.4f;
    [Range(0.1f, 1)]
    [SerializeField]
    private float targetRedValue = 0.7f;
    [SerializeField]
    private float vortexAngle = 20;

    // Reference Variables
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
    private bool soulForm;

    protected override void InitToggleWorlds()
    {
        if (soulForm)
            StartCoroutine(Unsoulify());
        else
        {
            bloom.enabled = true;
            colorCurves.enabled = true;
            StartCoroutine(Soulify());
        }
        vortex.enabled = true;
    }

    protected override void ToggleWorlds()
    {
        StartCoroutine(SmoothBloom(soulForm));
        if (soulForm)
        {
            soulForm = false;
            colorCurves.enabled = false;
        }
        else
        {
            soulForm = true;
        }
    }

    private IEnumerator Soulify()
    {
        float time = 0;
        float maxTime = GameManager.transitionTime;
        float percentage;

        float prevSat = colorCurves.saturation;

        Keyframe blueKey = colorCurves.blueChannel.keys[0];
        Keyframe redKey = colorCurves.redChannel.keys[1];

        while (time < maxTime)
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

        ToggleWorlds();
    }

    private IEnumerator Unsoulify()
    {
        float time = 0;
        float maxTime = GameManager.transitionTime;
        float percentage;
        
        float prevSat = colorCurves.saturation;

        Keyframe blueKey = colorCurves.blueChannel.keys[0];
        Keyframe redKey = colorCurves.redChannel.keys[1];

        float prevBloom = bloom.bloomIntensity;

        while (time < maxTime)
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
