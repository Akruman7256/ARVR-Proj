using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spaceship-style 6-DOF flight controller + camera rig.
///
/// SETUP
///  1. Create an empty GameObject "Ship" (scale 1,1,1) and put this script on it.
///  2. Make your Main Camera a CHILD of "Ship" (local position 0, rotation 0, scale 1).
///  3. Remove the old FreeFlyCamera script.
///  4. Drag the Sun into the "Sun" slot. Planets are found automatically.
///
/// Every speed, clip plane and safety distance is derived from the Sun's size,
/// so if you rescale the solar system again you only change the Sun's scale.
///
/// CONTROLS
///  W/S thrust, A/D strafe, Space/LeftCtrl up/down, Q/E roll, Mouse pitch+yaw,
///  LeftShift boost, X full stop, Z flight assist on/off, Scroll = speed cap, Esc = cursor.
/// </summary>
public class SpaceshipFlightController : MonoBehaviour
{
    // ------------------------------------------------------------------ scale
    [Header("World Scale (every speed is derived from the Sun's size)")]
    [Tooltip("Drag your Sun here. Its scale (diameter in units) defines the world scale.")]
    public Transform sun;
    [Tooltip("Used only if no Sun is assigned.")]
    public float sunDiameterUnitsFallback = 50f;
    [Tooltip("Real sizes divided by this (500 = planets at 1:500 of real size).")]
    public float compression = 500f;
    [Tooltip("Length of the ship you want to feel like you are piloting.")]
    public float shipLengthMeters = 200f;

    [Header("Bodies (Sun, planets, moons)")]
    [Tooltip("Leave empty to auto-find every SphereCollider above the min radius.")]
    public List<Transform> bodies = new List<Transform>();
    public float autoFindMinRadius = 0.05f;

    // ------------------------------------------------------------------ speed
    [Header("Speed")]
    [Tooltip("Seconds to fly from the Sun to the outermost body at full cruise speed.")]
    public float crossingTimeSeconds = 30f;
    [Tooltip("Slowest allowed top speed (right at a surface), in ship-lengths per second.")]
    public float minSpeedShipLengthsPerSec = 15f;
    [Tooltip("Top speed gained per unit of distance to the nearest surface. 0.4 = very natural.")]
    public float speedPerUnitDistance = 0.4f;
    [Tooltip("Seconds to reach top speed from standstill.")]
    public float timeToTopSpeed = 2.5f;
    public float boostMultiplier = 3f;
    [Tooltip("How strong the stabilising thrusters are compared to the main thrust.")]
    public float dampersStrength = 1.5f;
    [Tooltip("How quickly speed bleeds off when above the allowed limit (e.g. approaching a planet).")]
    public float overspeedBrake = 2.5f;
    [Tooltip("ON: thrusters automatically cancel drift. OFF: pure Newtonian, you keep drifting.")]
    public bool flightAssist = true;

    // --------------------------------------------------------------- rotation
    [Header("Rotation")]
    public float lookSensitivity = 2f;
    public float maxPitchYawRate = 70f;   // deg/sec
    public float maxRollRate = 90f;       // deg/sec
    [Tooltip("Higher = snappier, lower = heavier ship.")]
    public float rotationResponse = 6f;
    public bool invertY = false;
    public bool invertRoll = false;

    // ------------------------------------------------------------ camera feel
    [Header("Camera Feel")]
    [Tooltip("Leave empty to use the first Camera found on this object or its children.")]
    public Camera cam;
    public float baseFOV = 60f;
    [Tooltip("Extra FOV at full speed. Sells velocity without changing perceived size.")]
    public float boostFOVKick = 10f;
    public float fovResponse = 3f;
    [Tooltip("Camera trails behind the ship's turns (needs the camera to be a child).")]
    public float cameraLag = 0.02f;
    [Tooltip("Engine rumble while boosting, in degrees.")]
    public float shakeDegrees = 0.15f;
    [Tooltip("Near clip as a fraction of the ship length.")]
    public float nearClipShipFraction = 0.1f;
    [Tooltip("Far clip as a multiple of the solar system radius.")]
    public float farClipSystemMultiplier = 4f;

    // ----------------------------------------------------------------- safety
    [Header("Safety")]
    [Tooltip("The ship cannot get closer to a surface than this many ship-lengths.")]
    public float minAltitudeShipLengths = 0.5f;

    [Header("Floating Origin (turn on once your orbit script is confirmed compatible)")]
    public bool useFloatingOrigin = false;
    public float floatingOriginThreshold = 2000f;

    [Header("HUD")]
    public bool showHUD = true;

    // ------------------------------------------------------------------ state
    struct Body { public Transform t; public float r; }

    readonly List<Body> _bodies = new List<Body>();
    Transform _camT;

    float _metersPerUnit, _shipLen, _minSpeed, _maxSpeed, _systemRadius;
    Vector3 _velocity;
    Vector3 _angVel;           // deg/sec (pitch, yaw, roll) in local space
    float _throttleCap = 1f;
    float _shakeAmt;

    string _nearestName = "-";
    float _surfaceDist = float.MaxValue;
    float _currentLimit;

    // ------------------------------------------------------------------ setup
    void Start()
    {
        float sunD = sun != null ? Mathf.Abs(sun.lossyScale.x) : sunDiameterUnitsFallback;
        // Real Sun diameter 1,392,700,000 m, compressed, mapped onto the Sun's size in units.
        _metersPerUnit = 1_392_700_000f / compression / sunD;
        _shipLen = shipLengthMeters / _metersPerUnit;

        RefreshBodies();

        Vector3 centre = sun != null ? sun.position : Vector3.zero;
        float far = 0f;
        foreach (Body b in _bodies)
            far = Mathf.Max(far, Vector3.Distance(b.t.position, centre));
        _systemRadius = far > 0f ? far : sunD * 40f;

        _minSpeed = _shipLen * minSpeedShipLengthsPerSec;
        _maxSpeed = Mathf.Max(_systemRadius / crossingTimeSeconds, _minSpeed * 20f);

        if (cam == null) cam = GetComponentInChildren<Camera>();
        if (cam != null)
        {
            _camT = cam.transform;
            cam.nearClipPlane = Mathf.Max(_shipLen * nearClipShipFraction, 1e-5f);
            cam.farClipPlane = Mathf.Max(_systemRadius * farClipSystemMultiplier, 1000f);
            cam.fieldOfView = baseFOV;
        }

        Cursor.lockState = CursorLockMode.Locked;
    }

    [ContextMenu("Refresh Bodies")]
    void RefreshBodies()
    {
        _bodies.Clear();

        if (bodies != null && bodies.Count > 0)
        {
            foreach (Transform t in bodies)
                if (t != null)
                    _bodies.Add(new Body { t = t, r = Mathf.Abs(t.lossyScale.x) * 0.5f });
            return;
        }

        foreach (SphereCollider sc in FindObjectsByType<SphereCollider>(FindObjectsSortMode.None))
        {
            Vector3 s = sc.transform.lossyScale;
            float r = sc.radius * Mathf.Max(Mathf.Abs(s.x), Mathf.Abs(s.y), Mathf.Abs(s.z));
            if (r >= autoFindMinRadius)
                _bodies.Add(new Body { t = sc.transform, r = r });
        }

        if (_bodies.Count == 0)
            Debug.LogWarning("SpaceshipFlightController: no bodies found. Assign them in the 'Bodies' list.");
    }

    // ----------------------------------------------------------------- update
    void Update()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        HandleToggles();
        UpdateNearestBody();

        // Allowed top speed: grows with distance from the nearest surface.
        float limit = Mathf.Clamp(_surfaceDist * speedPerUnitDistance, _minSpeed, _maxSpeed);
        bool boost = Input.GetKey(KeyCode.LeftShift);
        float eff = limit * _throttleCap * (boost ? boostMultiplier : 1f);
        _currentLimit = eff;

        UpdateRotation(dt);
        UpdateTranslation(dt, eff);

        transform.position += _velocity * dt;
        ResolveCollisions();
    }

    void HandleToggles()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked
                ? CursorLockMode.None : CursorLockMode.Locked;

        if (Input.GetKeyDown(KeyCode.Z)) flightAssist = !flightAssist;

        float scroll = Input.mouseScrollDelta.y;
        if (scroll != 0f)
            _throttleCap = Mathf.Clamp(_throttleCap * Mathf.Pow(1.15f, scroll), 0.01f, 1f);
    }

    void UpdateRotation(float dt)
    {
        Vector2 mouse = Vector2.zero;
        if (Cursor.lockState == CursorLockMode.Locked)
            mouse = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        // Convert per-frame mouse movement to a turn RATE (deg/sec) -> framerate independent.
        float inv = 1f / dt;
        float pitchT = -mouse.y * lookSensitivity * inv * (invertY ? -1f : 1f);
        float yawT = mouse.x * lookSensitivity * inv;
        float rollIn = (Input.GetKey(KeyCode.Q) ? 1f : 0f) - (Input.GetKey(KeyCode.E) ? 1f : 0f);
        if (invertRoll) rollIn = -rollIn;

        Vector3 target = new Vector3(
            Mathf.Clamp(pitchT, -maxPitchYawRate, maxPitchYawRate),
            Mathf.Clamp(yawT, -maxPitchYawRate, maxPitchYawRate),
            rollIn * maxRollRate);

        // Angular inertia: the ship eases into and out of turns.
        _angVel = Vector3.Lerp(_angVel, target, 1f - Mathf.Exp(-rotationResponse * dt));
        transform.Rotate(_angVel * dt, Space.Self);
    }

    void UpdateTranslation(float dt, float eff)
    {
        Vector3 input = new Vector3(
            Axis(KeyCode.D, KeyCode.A),
            Axis(KeyCode.Space, KeyCode.LeftControl),
            Axis(KeyCode.W, KeyCode.S));
        if (input.sqrMagnitude > 1f) input.Normalize();

        float accel = eff / Mathf.Max(timeToTopSpeed, 0.05f);
        float damp = accel * dampersStrength;

        if (Input.GetKey(KeyCode.X))
        {
            // Full stop: all thrusters fire against the velocity.
            _velocity = Vector3.MoveTowards(_velocity, Vector3.zero, damp * 2f * dt);
        }
        else if (flightAssist)
        {
            // Per-axis thruster control in the ship's own frame. Velocity is kept in world
            // space, so after a turn the old velocity becomes drift that the dampers cancel.
            Vector3 v = transform.InverseTransformDirection(_velocity);
            v.x = AssistAxis(v.x, input.x * eff, accel, damp, dt);
            v.y = AssistAxis(v.y, input.y * eff, accel, damp, dt);
            v.z = AssistAxis(v.z, input.z * eff, accel, damp, dt);
            _velocity = transform.TransformDirection(v);
        }
        else
        {
            // Pure Newtonian: thrust only changes velocity, nothing slows you down.
            _velocity += transform.TransformDirection(input) * accel * dt;
        }

        // Global speed limiter (approaching a planet bleeds speed off smoothly).
        float speed = _velocity.magnitude;
        if (speed > eff * 1.001f)
        {
            float newSpeed = Mathf.Lerp(speed, eff, 1f - Mathf.Exp(-overspeedBrake * dt));
            _velocity *= newSpeed / speed;
        }
    }

    static float Axis(KeyCode pos, KeyCode neg)
    {
        return (Input.GetKey(pos) ? 1f : 0f) - (Input.GetKey(neg) ? 1f : 0f);
    }

    static float AssistAxis(float v, float target, float accel, float damp, float dt)
    {
        if (Mathf.Abs(target) > 1e-9f)
        {
            bool speedingUp = Mathf.Abs(target) > Mathf.Abs(v) && Mathf.Sign(target) == Mathf.Sign(v);
            return Mathf.MoveTowards(v, target, (speedingUp ? accel : damp) * dt);
        }
        return Mathf.MoveTowards(v, 0f, damp * dt);
    }

    // ----------------------------------------------------------------- bodies
    void UpdateNearestBody()
    {
        float best = float.MaxValue;
        string bestName = "-";
        Vector3 p = transform.position;

        foreach (Body b in _bodies)
        {
            if (b.t == null) continue;
            float d = Vector3.Distance(p, b.t.position) - b.r;
            if (d < best) { best = d; bestName = b.t.name; }
        }

        _surfaceDist = Mathf.Max(best, 0f);
        _nearestName = bestName;
    }

    void ResolveCollisions()
    {
        float minAlt = _shipLen * minAltitudeShipLengths;
        Vector3 pos = transform.position;

        foreach (Body b in _bodies)
        {
            if (b.t == null) continue;
            Vector3 c = b.t.position;
            float r = b.r + minAlt;
            Vector3 d = pos - c;
            float dist = d.magnitude;
            if (dist >= r || dist < 1e-9f) continue;

            Vector3 n = d / dist;
            pos = c + n * r;                       // push out to the minimum altitude
            float vn = Vector3.Dot(_velocity, n);
            if (vn < 0f) _velocity -= n * vn;      // cancel the inward velocity, keep sliding
        }
        transform.position = pos;
    }

    // ------------------------------------------------------------ camera feel
    void LateUpdate()
    {
        float dt = Time.deltaTime;

        if (useFloatingOrigin && transform.position.magnitude > floatingOriginThreshold)
        {
            Vector3 offset = transform.position;
            foreach (GameObject root in gameObject.scene.GetRootGameObjects())
                root.transform.position -= offset;
        }

        if (cam == null || dt <= 0f) return;

        // FOV kick scales with speed relative to deep-space cruise speed.
        float speedRatio = Mathf.Clamp01(_velocity.magnitude / (_maxSpeed * boostMultiplier));
        float targetFov = baseFOV + boostFOVKick * Mathf.Sqrt(speedRatio);
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFov, 1f - Mathf.Exp(-fovResponse * dt));

        // Rotational lag + engine rumble (only possible when the camera is a child).
        if (_camT != null && _camT != transform)
        {
            bool boosting = Input.GetKey(KeyCode.LeftShift) && _velocity.sqrMagnitude > 1e-12f;
            _shakeAmt = Mathf.MoveTowards(_shakeAmt, boosting ? speedRatio : 0f, 2f * dt);

            float t = Time.time * 22f;
            Vector3 shake = new Vector3(
                Mathf.PerlinNoise(t, 0.0f) - 0.5f,
                Mathf.PerlinNoise(0.0f, t) - 0.5f,
                Mathf.PerlinNoise(t, t) - 0.5f) * (2f * shakeDegrees * _shakeAmt);

            Quaternion target = Quaternion.Euler(-_angVel * cameraLag) * Quaternion.Euler(shake);
            _camT.localRotation = Quaternion.Slerp(_camT.localRotation, target, 1f - Mathf.Exp(-10f * dt));
        }
    }

    // -------------------------------------------------------------------- HUD
    GUIStyle _style;

    void OnGUI()
    {
        if (!showHUD) return;
        if (_style == null)
        {
            _style = new GUIStyle(GUI.skin.label) { fontSize = 16 };
            _style.normal.textColor = new Color(0.85f, 0.95f, 1f);
        }

        float mps = _velocity.magnitude * _metersPerUnit;
        float limMps = _currentLimit * _metersPerUnit;
        float altKm = _surfaceDist * _metersPerUnit / 1000f;

        string text =
            $"Speed:     {Format(mps)}   (limit {Format(limMps)})\n" +
            $"Nearest:   {_nearestName}   altitude {altKm:N0} km\n" +
            $"Assist:    {(flightAssist ? "ON" : "OFF")}   Throttle cap {(_throttleCap * 100f):N0}%";

        GUI.Label(new Rect(16, 16, 700, 90), text, _style);
    }

    static string Format(float metersPerSecond)
    {
        return metersPerSecond >= 1000f
            ? $"{metersPerSecond / 1000f:N1} km/s"
            : $"{metersPerSecond:N0} m/s";
    }
}
