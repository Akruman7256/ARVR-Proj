using UnityEngine;

public class BarycentricInitializer : MonoBehaviour {
    public GravityBody bodyA; // e.g. Sun
    public GravityBody bodyB; // e.g. Mars

    void Start() {
        Rigidbody rbA = bodyA.GetComponent<Rigidbody>();
        Rigidbody rbB = bodyB.GetComponent<Rigidbody>();

        Vector3 r = bodyB.transform.position - bodyA.transform.position;
        float R = r.magnitude;
        if (R == 0f) return;

        float mA = bodyA.mass;
        float mB = bodyB.mass;
        float totalMass = mA + mB;

        // Tangential direction, perpendicular to the separation vector.
        // Falls back to Vector3.forward if r happens to be parallel to
        // Vector3.up (cross product would otherwise be zero-length,
        // leaving no tangential push at all).
        Vector3 refUp = Vector3.up;
        if (Mathf.Abs(Vector3.Dot(r.normalized, refUp)) > 0.999f) refUp = Vector3.forward;
        Vector3 vDir = Vector3.Cross(r.normalized, refUp).normalized;

        // Correct two-body circular orbit: this is the RELATIVE speed
        // between the two bodies, valid for ANY mass ratio (not just
        // when one body is much heavier than the other).
        float relativeSpeed = Mathf.Sqrt((GravityBody.G * totalMass) / R);

        // Split the relative velocity between both bodies so the system's
        // total momentum is zero and the barycenter stays put, instead of
        // assuming bodyA is fixed.
        Vector3 velocityA = -vDir * relativeSpeed * (mB / totalMass);
        Vector3 velocityB = vDir * relativeSpeed * (mA / totalMass);

        rbA.linearVelocity = velocityA;
        rbB.linearVelocity = velocityB;

        Debug.Log($"[BarycentricInitializer] R={R:F2}, relativeSpeed={relativeSpeed:F2}, " +
                   $"vDir={vDir}, velocityA={velocityA}, velocityB={velocityB}");
    }
}