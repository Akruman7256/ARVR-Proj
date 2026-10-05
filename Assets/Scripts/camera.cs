using UnityEngine;
[RequireComponent(typeof(Camera))]
public class FreeFlyCamera : MonoBehaviour
{
    [Header("Base Movement")]
    public float moveSpeed = 10f;
    public float fastMultiplier = 4f;
    public float lookSensitivity = 2f;
    [Header("Scale Feel — Proximity Response")]
    [Tooltip("Layer(s) considered 'celestial bodies' for proximity checks. Leave as Everything if you don't use layers.")]
    public LayerMask bodyLayerMask = ~0;
    [Tooltip("Radius around the camera to search for nearby bodies each frame.")]
    public float proximityCheckRadius = 200f;
    [Tooltip("When within this many multiples of a body's own radius, speed/FOV start reacting.")]
    public float reactionRadiusMultiplier = 6f;
    [Tooltip("Minimum move speed multiplier when right up against a huge body (sells scale via slow traversal).")]
    public float minSpeedMultiplierNearBody = 0.08f;
    [Tooltip("Base field of view in open space.")]
    public float baseFOV = 60f;
    [Tooltip("Narrowest FOV allowed when very close to a large body.")]
    public float minFOVNearBody = 42f;
    [Header("Clipping")]
    [Tooltip("Very small so you can get right up to a surface before clipping through it.")]
    public float nearClipPlane = 0.01f;
    private float yaw;
    private float pitch;
    private Camera cam;
    void Start()
    {
        cam = GetComponent<Camera>();
        cam.nearClipPlane = nearClipPlane;
        cam.fieldOfView = baseFOV;
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
        Cursor.lockState = CursorLockMode.Locked;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked
                ? CursorLockMode.None : CursorLockMode.Locked;
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            yaw += Input.GetAxis("Mouse X") * lookSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * lookSensitivity;
            pitch = Mathf.Clamp(pitch, -89f, 89f);
            transform.eulerAngles = new Vector3(pitch, yaw, 0f);
        }
        float proximityFactor = GetProximityFactor(); // 1 = open space, 0 = right at a huge body's surface
        float speed = moveSpeed * Mathf.Lerp(minSpeedMultiplierNearBody, 1f, proximityFactor);
        if (Input.GetKey(KeyCode.LeftShift)) speed *= fastMultiplier;
        Vector3 move = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) move += transform.forward;
        if (Input.GetKey(KeyCode.S)) move -= transform.forward;
        if (Input.GetKey(KeyCode.D)) move += transform.right;
        if (Input.GetKey(KeyCode.A)) move -= transform.right;
        if (Input.GetKey(KeyCode.E)) move += transform.up;
        if (Input.GetKey(KeyCode.Q)) move -= transform.up;
        transform.position += move.normalized * speed * Time.deltaTime;
        float targetFOV = Mathf.Lerp(minFOVNearBody, baseFOV, proximityFactor);
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * 3f);
    }
    // Returns 1 in open space, tapering to 0 as the camera nears a large body's surface.
    // Uses each collider's bounds extent as a stand-in for "how big is this thing",
    // so bigger bodies start influencing speed/FOV from farther away.
    private float GetProximityFactor()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, proximityCheckRadius, bodyLayerMask);
        if (hits.Length == 0) return 1f;
        float closestFactor = 1f;
        foreach (var col in hits)
        {
            float bodyRadius = col.bounds.extents.magnitude;
            if (bodyRadius <= 0.001f) continue;
            float distance = Vector3.Distance(transform.position, col.ClosestPoint(transform.position));
            float reactionDistance = bodyRadius * reactionRadiusMultiplier;
            if (distance < reactionDistance)
            {
                float factor = Mathf.Clamp01(distance / reactionDistance);
                if (factor < closestFactor) closestFactor = factor;
            }
        }
        return closestFactor;
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.15f);
        Gizmos.DrawWireSphere(transform.position, proximityCheckRadius);
    }
}