using Borodar.FarlandSkies.Core.Helpers;
using UnityEngine;

namespace Borodar.FarlandSkies.NebulaOne
{
    [ExecuteInEditMode]
    [HelpURL("http://www.borodar.com/stuff/farlandskies/nebulaone/docs/QuickStart.v1.3.2.pdf")]
    public class SkyboxCycleManager : Singleton<SkyboxCycleManager>
    {
        public float CycleDuration = 10f;
        public float CycleProgress;
        public bool Paused;

        private SkyboxAnimator _skyboxAnimator;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected void Start()
        {
            _skyboxAnimator = SkyboxAnimator.Instance;
            UpdateCycleProgress();
        }

        protected void Update()
        {
            if (Application.isPlaying && !Paused)
            {
                CycleProgress += (Time.deltaTime / CycleDuration) * 100f;
                CycleProgress %= 100f;
            }

            UpdateCycleProgress();
        }

        protected void OnValidate()
        {
            UpdateCycleProgress();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void UpdateCycleProgress()
        {
            if (_skyboxAnimator != null)
                _skyboxAnimator.CycleProgress = CycleProgress;
        }
    }
}