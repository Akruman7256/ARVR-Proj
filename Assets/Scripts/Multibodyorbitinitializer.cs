using UnityEngine;
public class MultiBodyOrbitInitializer : MonoBehaviour
{
    [System.Serializable]
    public struct OrbitingBody
    {
        public GravityBody body;
        [Tooltip("Real average orbital inclination in degrees, for reference: Mercury 7.0, Venus 3.4, Earth 0, Mars 1.9, Jupiter 1.3, Saturn 2.5, Uranus 0.8, Neptune 1.8. Exaggerate this if you want the tilt to actually be visible.")]
        public float inclinationDegrees;
    }
    public GravityBody primary;                // the Sun
    public OrbitingBody[] orbitingBodies;       // the Planets
    [Tooltip("Base orbit plane before inclination is applied. Vector3.up = base plane is XZ.")]
    public Vector3 baseOrbitPlaneNormal = Vector3.up;
    void Start()
    {
        Rigidbody primaryRb = primary.GetComponent<Rigidbody>();
        Vector3 totalMomentum = Vector3.zero;
        foreach (OrbitingBody entry in orbitingBodies)
        {
            GravityBody body = entry.body;
            Rigidbody rb = body.GetComponent<Rigidbody>();
            Vector3 r = body.transform.position - primary.transform.position;
            float R = r.magnitude;
            if (R < 0.001f)
            {
                Debug.LogWarning($"[MultiBodyOrbitInitializer] {body.name} is at (almost) the same position as {primary.name} - skipping.");
                continue;
            }
            // Tilt the orbit plane's normal by this body's inclination,
            // rotating around the line-of-nodes axis (here just X, which
            // is fine for visual believability - real longitude-of-node
            // per planet isn't something you'd notice without a labeled HUD).
            Vector3 tiltedNormal = Quaternion.AngleAxis(entry.inclinationDegrees, Vector3.right) * baseOrbitPlaneNormal.normalized;
            Vector3 tangent = Vector3.Cross(r.normalized, tiltedNormal);
            if (tangent.sqrMagnitude < 0.0001f)
            {
                Vector3 fallback = Mathf.Abs(Vector3.Dot(r.normalized, Vector3.forward)) > 0.999f ? Vector3.right : Vector3.forward;
                tangent = Vector3.Cross(r.normalized, fallback);
            }
            tangent.Normalize();
            float speed = Mathf.Sqrt(GravityBody.G * (primary.mass + body.mass) / R);
            Vector3 velocity = tangent * speed;
            rb.linearVelocity = velocity;
            totalMomentum += body.mass * velocity;
            Debug.Log($"[MultiBodyOrbitInitializer] {body.name}: R={R:F2}, speed={speed:F2}, " +
                      $"inclination={entry.inclinationDegrees}°, velocity={velocity}");
        }
        Vector3 primaryVelocity = -totalMomentum / primary.mass;
        primaryRb.linearVelocity = primaryVelocity;
        Debug.Log($"[MultiBodyOrbitInitializer] {primary.name} corrective velocity: {primaryVelocity}");
    }
}