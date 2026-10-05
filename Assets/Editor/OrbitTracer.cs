using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(LineRenderer))]
public class OrbitPathTracer : MonoBehaviour
{
    [Header("Recording")]
    [Tooltip("Seconds of simulated time between recorded points. Smaller = smoother curve, more memory.")]
    public float recordInterval = 0.1f;
    [Tooltip("Safety cap on raw point count, regardless of loop logic below. Prevents runaway memory if orbitCenter is never assigned.")]
    public int maxPoints = 4000;
    [Tooltip("Line width in world units.")]
    public float lineWidth = 0.05f;
    [Header("Loop-Based Sliding Window")]
    [Tooltip("The body this planet orbits (usually the Sun's Transform). Required for loop counting. If left empty, falls back to simple point-count behavior using maxPoints/freezeWhenFull.")]
    public Transform orbitCenter;
    [Tooltip("How many full loops of trail to keep visible once the sliding window kicks in.")]
    public float loopsToKeep = 2f;
    [Tooltip("If true (and orbitCenter is set), once 'loopsToKeep' loops have completed, older points get removed one-by-one as new points are added, in the same order they were recorded - so the trail always shows roughly the last N loops.")]
    public bool useLoopSlidingWindow = true;
    [Tooltip("Fallback used only if orbitCenter is NOT assigned: stops adding new points once maxPoints is hit (freezes the full path) instead of sliding.")]
    public bool freezeWhenFull = true;
    private LineRenderer line;
    private List<Vector3> points = new List<Vector3>();
    private float timeSinceLastRecord;
    // Loop tracking
    private float cumulativeAngleDeg;
    private Vector3 lastDirFromCenter;
    private bool hasLastDir;
    private bool slidingWindowActive;
    void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.widthMultiplier = lineWidth;
        line.positionCount = 0;
        // Plain white unlit material so it reads as a clean white line
        // regardless of scene lighting.
        if (line.sharedMaterial == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Color"); // fallback for Built-in RP
            Material mat = new Material(shader);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", Color.white);
            line.material = mat;
        }
        line.startColor = Color.white;
        line.endColor = Color.white;
    }
    void FixedUpdate()
    {
        UpdateLoopTracking();
        bool slideNow = useLoopSlidingWindow && orbitCenter != null && slidingWindowActive;
        // Hard safety cap regardless of mode.
        bool atHardCap = points.Count >= maxPoints;
        if (!slideNow && atHardCap && freezeWhenFull)
        {
            return; // simple fallback mode: frozen once full
        }
        timeSinceLastRecord += Time.fixedDeltaTime;
        if (timeSinceLastRecord < recordInterval) return;
        timeSinceLastRecord = 0f;
        points.Add(transform.position);
        if (slideNow)
        {
            // Remove oldest point(s) in the same order they were added, one-for-one,
            // once we're past the loop threshold - keeps a rolling window.
            int targetCount = Mathf.Max(2, EstimatedPointsForLoops(loopsToKeep));
            while (points.Count > targetCount)
            {
                points.RemoveAt(0);
            }
        }
        else if (!freezeWhenFull && points.Count > maxPoints)
        {
            points.RemoveAt(0);
        }
        else if (points.Count > maxPoints)
        {
            points.RemoveAt(0); // hard safety cap even in freeze mode, avoids unbounded growth
        }
        line.positionCount = points.Count;
        line.SetPositions(points.ToArray());
    }
    // Tracks cumulative angle swept around orbitCenter to know when a full
    // loop (360 degrees) has completed, and how many points-per-loop that
    // translates to at the current recordInterval.
    private float degPerLoopEstimate = -1f;
    private float angleSinceLoopStart;
    private int pointsSinceLoopStart;
    private void UpdateLoopTracking()
    {
        if (orbitCenter == null) return;
        Vector3 dir = transform.position - orbitCenter.position;
        dir.y = 0f; // planets share the XZ plane per our earlier setup
        if (dir.sqrMagnitude < 0.0001f) return;
        dir.Normalize();
        if (hasLastDir)
        {
            float stepAngle = Vector3.SignedAngle(lastDirFromCenter, dir, Vector3.up);
            cumulativeAngleDeg += Mathf.Abs(stepAngle);
            angleSinceLoopStart += Mathf.Abs(stepAngle);
            pointsSinceLoopStart++;
            if (angleSinceLoopStart >= 360f)
            {
                // A full loop just completed - lock in how many points that took,
                // so we know how big a "loopsToKeep window" is in point-count terms.
                degPerLoopEstimate = pointsSinceLoopStart; // points per loop, reused as estimate
                angleSinceLoopStart = 0f;
                pointsSinceLoopStart = 0;
                float completedLoops = cumulativeAngleDeg / 360f;
                if (completedLoops >= loopsToKeep)
                {
                    slidingWindowActive = true;
                }
            }
        }
        lastDirFromCenter = dir;
        hasLastDir = true;
    }
    private int EstimatedPointsForLoops(float loops)
    {
        if (degPerLoopEstimate > 0f)
        {
            return Mathf.RoundToInt(degPerLoopEstimate * loops);
        }
        // Not enough data yet to know points-per-loop - fall back to maxPoints.
        return maxPoints;
    }
    [ContextMenu("Clear Trail")]
    public void ClearTrail()
    {
        points.Clear();
        line.positionCount = 0;
        cumulativeAngleDeg = 0f;
        angleSinceLoopStart = 0f;
        pointsSinceLoopStart = 0;
        degPerLoopEstimate = -1f;
        hasLastDir = false;
        slidingWindowActive = false;
    }
}