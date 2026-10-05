using UnityEngine;

/// <summary>
/// Spawns a ring of purely visual asteroids - NOT physics bodies, NOT part
/// of GravityBody's N-body list. Each one moves via a direct Kepler
/// circular-orbit formula computed every frame (cheap: no Rigidbody, no
/// collider, no per-pair force checks), so hundreds of them cost almost
/// nothing compared to even a handful of extra GravityBody objects.
///
/// Speed uses the same G/Sun-mass math as your real orbits, so visually
/// it matches the "physics feel" of the rest of the sim even though
/// nothing here is actually being simulated with forces.
/// </summary>
public class CosmeticAsteroidBelt : MonoBehaviour
{
    public GravityBody primary;      // the Sun - used for position + mass, NOT modified
    public GameObject asteroidPrefab; // small rock mesh, no Rigidbody needed
    public int count = 300;

    [Header("Belt shape")]
    public float innerRadius = 21f;
    public float outerRadius = 26f;
    [Tooltip("Max random vertical (Y) offset - real belts aren't perfectly flat.")]
    public float beltThickness = 1.5f;
    public float minScale = 0.05f;
    public float maxScale = 0.25f;

    private struct Asteroid
    {
        public Transform t;
        public float radius;
        public float angle;
        public float angularSpeed; // radians/sec, derived from Kepler formula
        public float yOffset;
    }

    private Asteroid[] asteroids;

    void Start()
    {
        asteroids = new Asteroid[count];
        for (int i = 0; i < count; i++)
        {
            float radius = Random.Range(innerRadius, outerRadius);
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float yOffset = Random.Range(-beltThickness, beltThickness);

            // Same circular-orbit speed formula used elsewhere - keeps
            // this visually consistent with your real orbiting bodies.
            float orbitalSpeed = Mathf.Sqrt(GravityBody.G * primary.mass / radius);
            float angularSpeed = orbitalSpeed / radius;

            GameObject go = Instantiate(asteroidPrefab, transform);
            go.name = $"Asteroid_{i}";
            float scale = Random.Range(minScale, maxScale);
            go.transform.localScale = Vector3.one * scale;
            go.transform.rotation = Random.rotation;

            asteroids[i] = new Asteroid
            {
                t = go.transform,
                radius = radius,
                angle = angle,
                angularSpeed = angularSpeed,
                yOffset = yOffset
            };

            UpdatePosition(i);
        }
    }

    void Update()
    {
        for (int i = 0; i < asteroids.Length; i++)
        {
            asteroids[i].angle += asteroids[i].angularSpeed * Time.deltaTime;
            UpdatePosition(i);
        }
    }

    private void UpdatePosition(int i)
    {
        var a = asteroids[i];
        Vector3 pos = primary.transform.position + new Vector3(
            Mathf.Cos(a.angle) * a.radius,
            a.yOffset,
            Mathf.Sin(a.angle) * a.radius
        );
        a.t.position = pos;
    }
}