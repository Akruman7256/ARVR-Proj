using Borodar.FarlandSkies.Core.Helpers;
using UnityEngine;

namespace Borodar.FarlandSkies.NebulaOne
{
    [ExecuteInEditMode]
    [HelpURL("http://www.borodar.com/stuff/farlandskies/nebulaone/docs/QuickStart.v1.3.2.pdf")]
    public class SkyboxAnimator : Singleton<SkyboxAnimator>
    {
        [SerializeField]
        private Vector3 _rotationSpeed = Vector3.one;
        [SerializeField]
        private float _distortionSpeed = 1f;
        [SerializeField]
        private float _maxDistortionValue = 0.25f;

        [SerializeField]
        private BackgroundParamsList _backgroundParamsList = new BackgroundParamsList();
        [SerializeField]
        private StarsParamsList _starsParamsList = new StarsParamsList();
        [SerializeField]
        private NebulaParamsList _nebulaParamsList = new NebulaParamsList();
        
        [SerializeField]
        private int _framesInterval = 2;

        // Non Serialized
        private SkyboxController _skyboxController;
        private float _cycleProgress;

        private int _framesToSkip;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        /// <summary>
        /// Progress of current animation cycle, in percents (0-100).</summary>
        public float CycleProgress
        {
            get => _cycleProgress;
            set => _cycleProgress = value % 100;
        }

        public Vector3 RotationSpeed
        {
            get => _rotationSpeed;
            set => _rotationSpeed = value;
        }

        public float DistortionSpeed
        {
            get => _distortionSpeed;
            set => _distortionSpeed = value;
        }

        public float MaxDistortionValue
        {
            get => _maxDistortionValue;
            set => _maxDistortionValue = value;
        }

        public BackgroundParam CurrentBackgroundParam { get; private set; }
        public StarsParam CurrentStarsParam { get; private set; }
        public NebulaParam CurrentNebulaParam { get; private set; }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected void Awake()
        {
            _backgroundParamsList.Init();
            _starsParamsList.Init();
            _nebulaParamsList.Init();
        }

        protected void Start()
        {
            _skyboxController = SkyboxController.Instance;
            CurrentBackgroundParam = _backgroundParamsList.GetParamPerTime(CycleProgress);
            CurrentStarsParam = _starsParamsList.GetParamPerTime(CycleProgress);
            CurrentNebulaParam = _nebulaParamsList.GetParamPerTime(CycleProgress);
        }

        protected void Update()
        {
            if (--_framesToSkip > 0) return;            
            _framesToSkip = _framesInterval;
                
            // Background Color
            CurrentBackgroundParam = _backgroundParamsList.GetParamPerTime(CycleProgress);
            _skyboxController.BackgroundColor = CurrentBackgroundParam.BackgroundColor;

            // Stars Params
            CurrentStarsParam = _starsParamsList.GetParamPerTime(CycleProgress);
            _skyboxController.StarsTint = CurrentStarsParam.Tint;
            _skyboxController.StarsBrightnessMin = CurrentStarsParam.BrightnessMin;
            _skyboxController.StarsBrightnessMax = CurrentStarsParam.BrightnessMax;

            // Rotation and distortion
            var scaledTime = (Application.isPlaying) ? 0.1f * Time.time : 0f;
            _skyboxController.DensityRotation = Modulo360(scaledTime * 10f * _rotationSpeed);

            scaledTime *= _distortionSpeed;

            var scaledTimeCos = Mathf.Cos(scaledTime);
            var scaledTimeSin = Mathf.Sin(scaledTime);
            var distortionDirection = new Vector3(scaledTimeCos, scaledTimeSin, scaledTimeCos*scaledTimeSin);

            _skyboxController.RipplesDistortion = _maxDistortionValue * distortionDirection;

            // Nebula Colors
            CurrentNebulaParam = _nebulaParamsList.GetParamPerTime(CycleProgress);
            _skyboxController.AmbientTint = CurrentNebulaParam.BackgroundTint;
            _skyboxController.BasementTint = CurrentNebulaParam.BasementTint;
            _skyboxController.RipplesTint1 = CurrentNebulaParam.RipplesTint1;
            _skyboxController.RipplesTint2 = CurrentNebulaParam.RipplesTint2;
        }

        protected void OnValidate()
        {
            _backgroundParamsList.Update();
            _starsParamsList.Update();
            _nebulaParamsList.Update();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private static Vector3 Modulo360(Vector3 input)
        {
            return new Vector3(input.x % 360f, input.y % 360f, input.z % 360f);
        }
    }
}