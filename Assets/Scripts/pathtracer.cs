using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(LineRenderer))]
public class OrbitPathTracer : MonoBehaviour
{
    [Tooltip("Seconds of simulated time between recorded points. Smaller = smoother curve, more memory.")]
    public float recordInterval = 0.1f;
    [Tooltip("Max points kept in the trail.")]
    public int maxPoints = 2000;
    [Tooltip("If true, stops adding new points once maxPoints is hit (freezes the full path). If false, oldest points get dropped so the trail keeps following the body (scrolling comet-tail style).")]
    public bool freezeWhenFull = true;
    [Tooltip("Line width in world units.")]
    public float lineWidth = 0.005f;
    private LineRenderer line;
    private List<Vector3> points = new List<Vector3>();
    private float timeSinceLastRecord;
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
        if (freezeWhenFull && points.Count >= maxPoints) return;
        timeSinceLastRecord += Time.fixedDeltaTime;
        if (timeSinceLastRecord < recordInterval) return;
        timeSinceLastRecord = 0f;
        points.Add(transform.position);
        if (!freezeWhenFull && points.Count > maxPoints)
        {
            points.RemoveAt(0);
        }
        line.positionCount = points.Count;
        line.SetPositions(points.ToArray());
    }
    [ContextMenu("Clear Trail")]
    public void ClearTrail()
    {
        points.Clear();
        line.positionCount = 0;
    }
}