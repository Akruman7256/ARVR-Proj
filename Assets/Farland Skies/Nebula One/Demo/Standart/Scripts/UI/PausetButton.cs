using UnityEngine;

namespace Borodar.FarlandSkies.NebulaOne
{
    public class PausetButton : MonoBehaviour
    {
        public void OnClick()
        {
            var sceneManager = SkyboxCycleManager.Instance;
            sceneManager.Paused = !sceneManager.Paused;
        }
    }
}