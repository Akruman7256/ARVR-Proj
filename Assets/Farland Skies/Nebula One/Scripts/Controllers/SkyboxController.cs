using Borodar.FarlandSkies.Core.Helpers;
using UnityEngine;

namespace Borodar.FarlandSkies.NebulaOne
{
    [ExecuteInEditMode]
    [HelpURL("http://www.borodar.com/stuff/farlandskies/nebulaone/docs/QuickStart.v1.3.2.pdf")]
    public class SkyboxController : Singleton<SkyboxController>
    {
        public Material SkyboxMaterial;

        // Starfield

        [SerializeField]
        private Cubemap _starfieldCubemap;

        [SerializeField]
        private Vector3 _starfieldRotation = Vector3.zero;
        private Matrix4x4 _starfieldRotationMatrix = Matrix4x4.identity;

        [SerializeField]
        private Color _backgroundColor = Color.black;
        [SerializeField]
        private Color _starsTint = Color.gray;
        [SerializeField]
        [Range(0,1)]
        [Tooltip("Lower threshold of the brightness of stars")]
        private float _starsBrightnessMin = 0f;
        [SerializeField]
        [Range(0, 1)]
        [Tooltip("Upper threshold of the brightness of stars")]
        private float _starsBrightnessMax = 1f;

        // Nebula Colors

        [SerializeField]
        [Tooltip("Applies the color filter to mild ripples across the whole skybox, regardless of nebula density")]
        private Color _ambientTint = new Color(0f, .63f, 1f, .03f);
        [SerializeField]
        [Tooltip("Refers to nebula basement haze, which is not affected by ripple distortion")]
        private Color _basementTint = new Color(0f, .63f, 1f, .5f);
        [SerializeField]
        [Tooltip("Applies to first layer of cloudy ripples. Usually, one of most dominant nebula colors")]
        private Color _ripplesTint1 = new Color(0f, .63f, 1f, .26f);
        [SerializeField]
        [Tooltip("Applies to second layer of cloudy ripples. Usually, one of most dominant nebula colors")]
        private Color _ripplesTint2 = new Color(.32f, .15f, .34f, .32f);

        // Nebula Density

        [SerializeField]
        [Tooltip("Defines the shape of the nebula. The brighter the area on the cubemap, the denser and opaque nebula is in this area")]
        private Cubemap _densityCubemap;

        [SerializeField]
        private Vector3 _densityRotation = Vector3.zero;
        private Matrix4x4 _densityRotationMatrix = Matrix4x4.identity;

        [SerializeField]
        [Range(0, 1)]
        [Tooltip("Lower threshold of the nebula's density. Allows adjusting overall nebula thickness without modifying the cubemap")]
        private float _densityThresholdLow = 0.4f;
        [SerializeField]
        [Range(0, 1)]
        [Tooltip("Upper threshold of the nebula's density. Allows adjusting overall nebula thickness without modifying the cubemap")]
        private float _densityThresholdHigh = 0.8f;

        // Nebula Diffusion

        [SerializeField]
        [Tooltip("Determines the appearance of the nebula in the details, as well as final ripple distortion values.")]
        private Cubemap _diffusionCubemap;

        [SerializeField]
        [Tooltip("Affects two layers of nebula cloudy ripples and allows to diversify nebula appearance")]
        private Vector3 _ripplesDistortion = new Vector3(0.2f, 0.1f, 0f);

        // General

        [SerializeField]
        [Range(0, 10f)]
        [Tooltip("Adjusts the brightness of the skybox")]
        private float _exposure = 1f;

        //---------------------------------------------------------------------
        // Shader Properties
        //---------------------------------------------------------------------

        private static readonly int BACKGROUND_COLOR = Shader.PropertyToID("_BackgroundColor");
        // Stars
        private static readonly int STARFIELD_CUBE = Shader.PropertyToID("_StarfieldCube");
        private static readonly int STARFIELD_ROTATION = Shader.PropertyToID("_StarfieldRotation");
        private static readonly int STARS_TINT = Shader.PropertyToID("_StarsTint");
        private static readonly int STARS_BRIGHTNESSL_MIN = Shader.PropertyToID("_StarsBrightnesslMin");
        private static readonly int STARS_BRIGHTNESSL_MAX = Shader.PropertyToID("_StarsBrightnesslMax");
        // Nebula Colors
        private static readonly int AMBIENT_TINT = Shader.PropertyToID("_AmbientTint");
        private static readonly int BASEMENT_TINT = Shader.PropertyToID("_BasementTint");
        private static readonly int RIPPLES_TINT1 = Shader.PropertyToID("_RipplesTint1");
        private static readonly int RIPPLES_TINT2 = Shader.PropertyToID("_RipplesTint2");
        // Nebula Density
        private static readonly int DENSITY_CUBE = Shader.PropertyToID("_DensityCube");
        private static readonly int DENSITY_ROTATION = Shader.PropertyToID("_DensityRotation");
        private static readonly int DENSITY_THRESHOLD_LOW = Shader.PropertyToID("_DensityThresholdLow");
        private static readonly int DENSITY_THRESHOLD_HIGH = Shader.PropertyToID("_DensityThresholdHigh");
        // Nebula Diffusion
        private static readonly int DIFFUSION_CUBE = Shader.PropertyToID("_DiffusionCube");
        private static readonly int RIPPLES_DISTORTION = Shader.PropertyToID("_RipplesDistortion");
        // General
        private static readonly int EXPOSURE = Shader.PropertyToID("_Exposure");

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        // Background

        public Color BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;
                SkyboxMaterial.SetColor(BACKGROUND_COLOR, _backgroundColor);
            }
        }

        // Stars

        public Cubemap StarfieldCubemap
        {
            get => _starfieldCubemap;
            set
            {
                _starfieldCubemap = value;
                SkyboxMaterial.SetTexture(STARFIELD_CUBE, _starfieldCubemap);
            }
        }

        public Vector3 StarfieldRotation
        {
            get => _starfieldRotation;
            set
            {
                _starfieldRotation = value;
                _starfieldRotationMatrix.SetTRS(Vector3.zero, Quaternion.Euler(_starfieldRotation), Vector3.one);
                SkyboxMaterial.SetMatrix(STARFIELD_ROTATION, _starfieldRotationMatrix);
            }
        }

        public Color StarsTint
        {
            get => _starsTint;
            set
            {
                _starsTint = value;
                SkyboxMaterial.SetColor(STARS_TINT, _starsTint);
            }
        }

        public float StarsBrightnessMin
        {
            get => _starsBrightnessMin;
            set
            {
                _starsBrightnessMin = value;
                SkyboxMaterial.SetFloat(STARS_BRIGHTNESSL_MIN, _starsBrightnessMin);
            }
        }

        public float StarsBrightnessMax
        {
            get => _starsBrightnessMax;
            set
            {
                _starsBrightnessMax = value;
                SkyboxMaterial.SetFloat(STARS_BRIGHTNESSL_MAX, _starsBrightnessMax);
            }
        }

        // Nebula Colors

        public Color AmbientTint
        {
            get => _ambientTint;
            set
            {
                _ambientTint = value;
                SkyboxMaterial.SetColor(AMBIENT_TINT, _ambientTint);
            }
        }

        public Color BasementTint
        {
            get => _basementTint;
            set
            {
                _basementTint = value;
                SkyboxMaterial.SetColor(BASEMENT_TINT, _basementTint);
            }
        }

        public Color RipplesTint1
        {
            get => _ripplesTint1;
            set
            {
                _ripplesTint1 = value;
                SkyboxMaterial.SetColor(RIPPLES_TINT1, _ripplesTint1);
            }
        }

        public Color RipplesTint2
        {
            get => _ripplesTint2;
            set
            {
                _ripplesTint2 = value;
                SkyboxMaterial.SetColor(RIPPLES_TINT2, _ripplesTint2);
            }
        }

        // Nebula Density

        public Cubemap DensityCubemap
        {
            get => _densityCubemap;
            set
            {
                _densityCubemap = value;
                SkyboxMaterial.SetTexture(DENSITY_CUBE, _densityCubemap);
            }
        }

        public Vector3 DensityRotation
        {
            get => _densityRotation;
            set
            {
                _densityRotation = value;
                _densityRotationMatrix.SetTRS(Vector3.zero, Quaternion.Euler(_densityRotation), Vector3.one);
                SkyboxMaterial.SetMatrix(DENSITY_ROTATION, _densityRotationMatrix);
            }
        }

        public float DensityThresholdLow
        {
            get => _densityThresholdLow;
            set
            {
                _densityThresholdLow = value;
                SkyboxMaterial.SetFloat(DENSITY_THRESHOLD_LOW, _densityThresholdLow);
            }
        }

        public float DensityThresholdHigh
        {
            get => _densityThresholdHigh;
            set
            {
                _densityThresholdHigh = value;
                SkyboxMaterial.SetFloat(DENSITY_THRESHOLD_HIGH, _densityThresholdHigh);
            }
        }

        // Nebula Diffusion

        public Cubemap DiffusionCubemap
        {
            get => _diffusionCubemap;
            set
            {
                _diffusionCubemap = value;
                SkyboxMaterial.SetTexture(DIFFUSION_CUBE, _diffusionCubemap);
            }
        }

        public Vector3 RipplesDistortion
        {
            get => _ripplesDistortion;
            set
            {
                _ripplesDistortion = value;
                SkyboxMaterial.SetVector(RIPPLES_DISTORTION, _ripplesDistortion);
            }
        }

        // General

        public float Exposure
        {
            get => _exposure;
            set
            {
                _exposure = value;
                SkyboxMaterial.SetFloat(EXPOSURE, _exposure);
            }
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected void Awake()
        {
            if (SkyboxMaterial != null)
            {
                RenderSettings.skybox = SkyboxMaterial;
                UpdateSkyboxProperties();
            }
            else
            {
                Debug.LogWarning("SkyboxController: Skybox material is not assigned.");
            }
        }

        protected void OnValidate()
        {
            UpdateSkyboxProperties();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void UpdateSkyboxProperties()
        {
            if (SkyboxMaterial == null) return;

            // Background
            SkyboxMaterial.SetColor(BACKGROUND_COLOR, _backgroundColor);
            // Stars
            SkyboxMaterial.SetTexture(STARFIELD_CUBE, _starfieldCubemap);
            _starfieldRotationMatrix.SetTRS(Vector3.zero, Quaternion.Euler(_starfieldRotation), Vector3.one);
            SkyboxMaterial.SetMatrix(STARFIELD_ROTATION, _starfieldRotationMatrix);
            SkyboxMaterial.SetColor(STARS_TINT, _starsTint);
            SkyboxMaterial.SetFloat(STARS_BRIGHTNESSL_MIN, _starsBrightnessMin);
            SkyboxMaterial.SetFloat(STARS_BRIGHTNESSL_MAX, _starsBrightnessMax);
            // Nebula Colors
            SkyboxMaterial.SetColor(AMBIENT_TINT, _ambientTint);
            SkyboxMaterial.SetColor(BASEMENT_TINT, _basementTint);
            SkyboxMaterial.SetColor(RIPPLES_TINT1, _ripplesTint1);
            SkyboxMaterial.SetColor(RIPPLES_TINT2, _ripplesTint2);
            // Nebula Density
            SkyboxMaterial.SetTexture(DENSITY_CUBE, _densityCubemap);
            _densityRotationMatrix.SetTRS(Vector3.zero, Quaternion.Euler(_densityRotation), Vector3.one);
            SkyboxMaterial.SetMatrix(DENSITY_ROTATION, _densityRotationMatrix);
            SkyboxMaterial.SetFloat(DENSITY_THRESHOLD_LOW, _densityThresholdLow);
            SkyboxMaterial.SetFloat(DENSITY_THRESHOLD_HIGH, _densityThresholdHigh);
            // Nebula Diffusion
            SkyboxMaterial.SetTexture(DIFFUSION_CUBE, _diffusionCubemap);
            SkyboxMaterial.SetVector(RIPPLES_DISTORTION, _ripplesDistortion);
            // General
            SkyboxMaterial.SetFloat(EXPOSURE, _exposure);
        }
    }
}