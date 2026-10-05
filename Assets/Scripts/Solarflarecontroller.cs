using System.Collections;
using UnityEngine;

/// <summary>
/// Randomly triggers solar flare events: a brief spike in the Sun's
/// emission intensity (a "flash") plus an optional particle burst
/// launched from a random point on the Sun's surface.
/// </summary>
public class SolarFlareController : MonoBehaviour
{
    [Header("References")]
    public Renderer sunRenderer;        // the Sun's MeshRenderer (drag Son here)
    public ParticleSystem flareParticles; // optional - see setup notes below

    [Header("Timing")]
    [Tooltip("Random delay range (seconds) between flare events.")]
    public Vector2 intervalRange = new Vector2(8f, 25f);

    [Header("Flash")]
    [Tooltip("How much brighter than baseline emission the flash gets, as a multiplier.")]
    public float flashIntensityMultiplier = 3f;
    public float flashRiseTime = 0.15f;
    public float flashFallTime = 0.6f;

    [Header("Particle burst")]
    public int minParticles = 20;
    public int maxParticles = 60;

    private Material sunMat;
    private Color baseEmissionColor;
    private float baseEmissionIntensity;

    void Start()
    {
        if (sunRenderer != null)
        {
            sunMat = sunRenderer.material; // instance, safe to modify at runtime
            if (sunMat.IsKeywordEnabled("_EMISSION"))
            {
                baseEmissionColor = sunMat.GetColor("_EmissionColor");
            }
        }

        StartCoroutine(FlareLoop());
    }

    private IEnumerator FlareLoop()
    {
        while (true)
        {
            float wait = Random.Range(intervalRange.x, intervalRange.y);
            yield return new WaitForSeconds(wait);
            yield return StartCoroutine(TriggerFlare());
        }
    }

    private IEnumerator TriggerFlare()
    {
        // --- Particle burst, launched from a random point on the sphere surface ---
        if (flareParticles != null && sunRenderer != null)
        {
            Vector3 randomDir = Random.onUnitSphere;
            float sunRadius = sunRenderer.bounds.extents.magnitude;
            Vector3 flarePoint = sunRenderer.transform.position + randomDir * sunRadius;

            flareParticles.transform.position = flarePoint;
            flareParticles.transform.rotation = Quaternion.LookRotation(randomDir);

            int count = Random.Range(minParticles, maxParticles);
            flareParticles.Emit(count);
        }

        // --- Emission flash ---
        if (sunMat != null)
        {
            Color flashColor = baseEmissionColor * flashIntensityMultiplier;

            float t = 0f;
            while (t < flashRiseTime)
            {
                t += Time.deltaTime;
                sunMat.SetColor("_EmissionColor", Color.Lerp(baseEmissionColor, flashColor, t / flashRiseTime));
                yield return null;
            }

            t = 0f;
            while (t < flashFallTime)
            {
                t += Time.deltaTime;
                sunMat.SetColor("_EmissionColor", Color.Lerp(flashColor, baseEmissionColor, t / flashFallTime));
                yield return null;
            }

            sunMat.SetColor("_EmissionColor", baseEmissionColor);
        }
    }
}