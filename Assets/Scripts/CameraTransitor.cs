using System.Collections;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class CameraTransitor : AbstractMultiWorld
{
    // Customizeable Variables
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
