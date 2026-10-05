using UnityEngine;
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PlanetRingMeshGenerator : MonoBehaviour
{
    [Header("Ring Dimensions")]
    [Tooltip("Inner radius of the ring, in local units. Should be >= the planet's own radius.")]
    public float innerRadius = 0.46f;
    [Tooltip("Outer radius of the ring, in local units.")]
    public float outerRadius = 0.97f;
    [Header("Detail")]
    [Tooltip("Number of segments around the ring. Higher = smoother circle.")]
    [Range(8, 256)]
    public int segments = 96;
    [Tooltip("Regenerate automatically if values change in the Inspector while selected.")]
    public bool autoRebuildInEditor = true;
    private Mesh mesh;
    void Awake()
    {
        Build();
    }
    void OnValidate()
    {
        if (autoRebuildInEditor)
        {
            // Defer to avoid calling SendMessage during OnValidate warnings.
            UnityEditor_SafeBuild();
        }
    }
    void UnityEditor_SafeBuild()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null) Build();
            };
        }
#endif
    }
    [ContextMenu("Rebuild Ring Mesh")]
    public void Build()
    {
        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.name = "ProceduralRing";
        }
        else
        {
            mesh.Clear();
        }
        int vertCount = (segments + 1) * 2;
        Vector3[] vertices = new Vector3[vertCount];
        Vector2[] uvs = new Vector2[vertCount];
        Vector3[] normals = new Vector3[vertCount];
        int[] triangles = new int[segments * 6];
        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            float angle = t * Mathf.PI * 2f;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);
            int innerIndex = i * 2;
            int outerIndex = i * 2 + 1;
            vertices[innerIndex] = new Vector3(cos * innerRadius, 0f, sin * innerRadius);
            vertices[outerIndex] = new Vector3(cos * outerRadius, 0f, sin * outerRadius);
            // U = 0 at inner edge, 1 at outer edge - lets a ring texture's
            // radial gradient (color bands, gaps, transparency) map correctly.
            uvs[innerIndex] = new Vector2(0f, t);
            uvs[outerIndex] = new Vector2(1f, t);
            normals[innerIndex] = Vector3.up;
            normals[outerIndex] = Vector3.up;
        }
        int triIndex = 0;
        for (int i = 0; i < segments; i++)
        {
            int innerA = i * 2;
            int outerA = i * 2 + 1;
            int innerB = (i + 1) * 2;
            int outerB = (i + 1) * 2 + 1;
            triangles[triIndex++] = innerA;
            triangles[triIndex++] = outerA;
            triangles[triIndex++] = innerB;
            triangles[triIndex++] = outerA;
            triangles[triIndex++] = outerB;
            triangles[triIndex++] = innerB;
            // Backface copy so the ring is visible from below too (no backface culling issues).
        }
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.normals = normals;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        GetComponent<MeshFilter>().mesh = mesh;
    }
}