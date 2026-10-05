using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlanetAxialRotation : MonoBehaviour
{
    [Header("Real Values (reference)")]
    [Tooltip("Axial tilt in degrees. Mercury 0.03, Venus 177.4 (retrograde), Earth 23.4, Mars 25.2, Jupiter 3.1, Saturn 26.7, Uranus 97.8 (sideways), Neptune 28.3.")]
    public float axialTiltDegrees = 23.4f;

    [Tooltip("Real rotation period in Earth days. Use a NEGATIVE value for retrograde (backward) spin - e.g. Venus = -243, Uranus = -0.72. Mercury 58.6, Venus -243, Earth 1, Mars 1.03, Jupiter 0.41, Saturn 0.44, Uranus -0.72, Neptune 0.67.")]
    public float rotationPeriodDays = 1f;

    [Header("Visual Pacing")]
    [Tooltip("Global multiplier so slow real periods (Venus: 243 days) are actually visible in a reasonable play session. Shared conceptually with any orbit time-scale control you add later.")]
    public float speedMultiplier = 50f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Axis of rotation, tilted from world-up by the axial tilt.
        Vector3 tiltedAxis = Quaternion.AngleAxis(axialTiltDegrees, Vector3.forward) * Vector3.up;

        // Convert period (days) to an angular speed in radians/second.
        // Negative period = retrograde (spins the other way).
        float periodSeconds = Mathf.Abs(rotationPeriodDays) * 86400f; // 86400 sec/day
        float dirSign = Mathf.Sign(rotationPeriodDays);
        float angularSpeedRadPerSec = (2f * Mathf.PI / periodSeconds) * dirSign * speedMultiplier;

        rb.angularVelocity = tiltedAxis.normalized * angularSpeedRadPerSec;
    }
}