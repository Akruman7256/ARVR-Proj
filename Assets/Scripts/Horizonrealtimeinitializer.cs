using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Fetches REAL current position + velocity data for each planet from NASA
/// JPL's Horizons system (the actual database used for real mission
/// planning) and seeds your GravityBody/Rigidbody setup with it.
///
/// IMPORTANT: this replaces MultiBodyOrbitInitializer's *approximated*
/// circular-orbit velocities with the planets' TRUE current state vectors.
/// It does NOT continuously poll - it fetches once at Start(), because
/// planetary motion is far too slow to notice between repeated polls.
/// From that point on, your own GravityBody N-body loop evolves the
/// simulation forward exactly as before - this just gives it a
/// historically-accurate starting point instead of a guessed one.
///
/// Requires internet access at Play time (works fine in the Unity Editor;
/// if you ever build a standalone player, confirm network permissions).
/// </summary>
public class HorizonsRealTimeInitializer : MonoBehaviour
{
    [Serializable]
    public struct HorizonsTarget
    {
        public string bodyName;      // must match a GravityBody's GameObject for logging clarity
        public string naifId;        // Horizons target ID, e.g. "499" for Mars
        public GravityBody target;   // the scene object to apply this data to
    }

    [Header("NAIF IDs: Mercury=199 Venus=299 Earth=399 Mars=499 Jupiter=599 Saturn=699 Uranus=799 Neptune=899")]
    public HorizonsTarget[] bodiesToFetch;

    [Header("Scale conversion (MUST match how you scaled distances/masses elsewhere)")]
    [Tooltip("Unity units per real kilometer. Tiny by necessity - tune so results land near your existing scene scale (e.g. Mars around 15-25 units from the Sun).")]
    public double distanceScaleUnityPerKm = 0.00000007; // ~ places Mars (~2.28e8 km) around ~16 units
    [Tooltip("Unity units-per-second per real km/s. Should generally match distanceScaleUnityPerKm if your simulation runs in real seconds.")]
    public double velocityScaleUnityPerKmS = 0.00000007;

    [Tooltip("Center body for the returned vectors - '500@10' = Sun's center. Leave as default unless you know you want something else.")]
    public string centerId = "500@10";

    void Start()
    {
        foreach (var b in bodiesToFetch)
        {
            StartCoroutine(FetchAndApply(b));
        }
    }

    private IEnumerator FetchAndApply(HorizonsTarget b)
    {
        DateTime today = DateTime.UtcNow.Date;
        DateTime tomorrow = today.AddDays(1);
        string start = today.ToString("yyyy-MM-dd");
        string stop = tomorrow.ToString("yyyy-MM-dd");

        string url = "https://ssd.jpl.nasa.gov/api/horizons.api" +
            "?format=text" +
            $"&COMMAND='{b.naifId}'" +
            "&OBJ_DATA='NO'" +
            "&MAKE_EPHEM='YES'" +
            "&EPHEM_TYPE='VECTORS'" +
            $"&CENTER='{centerId}'" +
            $"&START_TIME='{start}'" +
            $"&STOP_TIME='{stop}'" +
            "&STEP_SIZE='1%20d'" +
            "&VEC_TABLE='2'" +
            "&OUT_UNITS='KM-S'" +
            "&REF_PLANE='ECLIPTIC'" +
            "&CSV_FORMAT='YES'";

        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[Horizons] Failed to fetch {b.bodyName}: {req.error}");
                yield break;
            }

            if (!TryParseStateVector(req.downloadHandler.text, out Vector3 posKm, out Vector3 velKmS))
            {
                Debug.LogError($"[Horizons] Could not parse response for {b.bodyName}. Raw response logged below.");
                Debug.Log(req.downloadHandler.text);
                yield break;
            }

            ApplyToBody(b, posKm, velKmS);
        }
    }

    /// <summary>
    /// Horizons CSV vector output has data rows between "$$SOE" and "$$EOE"
    /// markers, format: JDTDB, Calendar Date, X, Y, Z, VX, VY, VZ, (km, km/s)
    /// </summary>
    private bool TryParseStateVector(string raw, out Vector3 posKm, out Vector3 velKmS)
    {
        posKm = Vector3.zero;
        velKmS = Vector3.zero;

        int startIdx = raw.IndexOf("$$SOE");
        int endIdx = raw.IndexOf("$$EOE");
        if (startIdx < 0 || endIdx < 0 || endIdx <= startIdx) return false;

        string block = raw.Substring(startIdx + 5, endIdx - (startIdx + 5)).Trim();
        string firstLine = block.Split('\n')[0].Trim();
        string[] fields = firstLine.Split(',');
        // fields: [0]=JDTDB [1]=Calendar Date [2]=X [3]=Y [4]=Z [5]=VX [6]=VY [7]=VZ
        if (fields.Length < 8) return false;

        float x = float.Parse(fields[2], CultureInfo.InvariantCulture);
        float y = float.Parse(fields[3], CultureInfo.InvariantCulture);
        float z = float.Parse(fields[4], CultureInfo.InvariantCulture);
        float vx = float.Parse(fields[5], CultureInfo.InvariantCulture);
        float vy = float.Parse(fields[6], CultureInfo.InvariantCulture);
        float vz = float.Parse(fields[7], CultureInfo.InvariantCulture);

        // Horizons ecliptic frame: X,Y = orbital plane, Z = perpendicular.
        // Mapped here to Unity's X,Z = orbital plane (Y up) to match how
        // you've been placing bodies (orbits flat on XZ).
        posKm = new Vector3(x, z, y);
        velKmS = new Vector3(vx, vz, vy);
        return true;
    }

    private void ApplyToBody(HorizonsTarget b, Vector3 posKm, Vector3 velKmS)
    {
        if (b.target == null)
        {
            Debug.LogWarning($"[Horizons] No target assigned for {b.bodyName}, skipping.");
            return;
        }

        Rigidbody rb = b.target.GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogWarning($"[Horizons] {b.bodyName} has no Rigidbody, skipping.");
            return;
        }

        Vector3 unityPos = posKm * (float)distanceScaleUnityPerKm;
        Vector3 unityVel = velKmS * (float)velocityScaleUnityPerKmS;

        rb.position = unityPos;
        rb.linearVelocity = unityVel;

        Debug.Log($"[Horizons] {b.bodyName}: realPos={posKm} km, realVel={velKmS} km/s " +
                  $"-> scenePos={unityPos}, sceneVel={unityVel}");
    }
}