using UnityEngine;
using System.Collections.Generic;
public class GravityBody : MonoBehaviour {
    public Rigidbody rb;
    public float mass = 1000f;
    public static float G = 2.0f;
    [SerializeField] private float softening = 0.5f;
     public static readonly List<GravityBody> bodies = new List<GravityBody>();
    void Awake() {
        if (rb == null) rb = GetComponent<Rigidbody>();
    }
    void OnEnable() { bodies.Add(this); }
    void OnDisable() { bodies.Remove(this); }
    void FixedUpdate() {
        if (rb.mass != mass) rb.mass = mass;
        foreach (GravityBody target in bodies) {
            if (target == this) continue;
            Vector3 direction = target.rb.position - rb.position;
            float distanceSqr = direction.sqrMagnitude + softening * softening;
            float forceMagnitude = G * (mass * target.mass) / distanceSqr;
            Vector3 forceVector = direction.normalized * forceMagnitude;
            rb.AddForce(forceVector, ForceMode.Force);
        }
    }
}