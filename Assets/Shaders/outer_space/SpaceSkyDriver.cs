using UnityEngine;

/// <summary>
/// Feeds the direction to the Sun into the Space Skybox shader every frame so
/// comet tails always point away from the Sun, even as the camera moves
/// through the solar system. Attach to any GameObject (e.g. the Camera).
/// </summary>
[ExecuteAlways]
public class SpaceSkyDriver : MonoBehaviour
{
    [Tooltip("The Sun object in your scene.")]
    public Transform sun;

    [Tooltip("Optional. Defaults to Camera.main.")]
    public Camera viewCamera;

    static readonly int SunDirId = Shader.PropertyToID("_SpaceSunDirection");

    void LateUpdate()
    {
        if (sun == null) return;

        Camera cam = viewCamera != null ? viewCamera : Camera.main;
        Vector3 from = cam != null ? cam.transform.position : Vector3.zero;

        Vector3 dir = sun.position - from;
        if (dir.sqrMagnitude < 1e-6f) return;
        dir.Normalize();

        Shader.SetGlobalVector(SunDirId, new Vector4(dir.x, dir.y, dir.z, 1f));
    }

    void OnDisable()
    {
        // w = 0 tells the shader to use its Fallback Sun Direction
        Shader.SetGlobalVector(SunDirId, Vector4.zero);
    }
}
