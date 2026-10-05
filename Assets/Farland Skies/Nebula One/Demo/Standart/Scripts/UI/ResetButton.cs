using UnityEngine;
using UnityEngine.UI;

namespace Borodar.FarlandSkies.NebulaOne
{
    public class ResetButton : MonoBehaviour
    {
        [Header("Starfield")]
        public Image BackgroundColorImage;
        public Image StarsTintImage;
        public Slider BrightnessMinSlider;
        public Slider BrightnessMaxSlider;

        [Header("Nebula Colors")]
        public Image BackgroundTintImage;
        public Slider BackgroundAlphaSlider;
        public Image BasementTintImage;
        public Slider BasementAlphaSlider;
        public Image RipplesTint1Image;
        public Slider RipplesAlpha1Slider;
        public Image RipplesTint2Image;
        public Slider RipplesAlpha2Slider;

        [Header("Nebula Density")]
        public Slider DensityRotationX;
        public Slider DensityRotationY;
        public Slider DensityRotationZ;
        public Slider ThresholdLow;
        public Slider ThresholdHigh;

        [Header("Nebula Diffusion")]
        public Slider RipplesDistortionX;
        public Slider RipplesDistortionY;
        public Slider RipplesDistortionZ;

        [Header("General")]
        public Slider ExposureSlider;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        public void Start()
        {
            // Starfield
            DefaultValue.BackgroundColor = SkyboxController.Instance.BackgroundColor;
            DefaultValue.StarsTint = SkyboxController.Instance.StarsTint;
            DefaultValue.BrightnessMin = SkyboxController.Instance.StarsBrightnessMin;
            DefaultValue.BrightnessMax = SkyboxController.Instance.StarsBrightnessMax;
            // Nebula Colors
            DefaultValue.BackgroundTint = SkyboxController.Instance.AmbientTint;
            DefaultValue.BasementTint = SkyboxController.Instance.BasementTint;
            DefaultValue.RipplesTint1 = SkyboxController.Instance.RipplesTint1;
            DefaultValue.RipplesTint2 = SkyboxController.Instance.RipplesTint2;
            // Nebula Density
            DefaultValue.DensityRotation = SkyboxController.Instance.DensityRotation;
            DefaultValue.ThresholdLow = SkyboxController.Instance.DensityThresholdLow;
            DefaultValue.ThresholdHigh = SkyboxController.Instance.DensityThresholdHigh;
            // Nebula Diffusion
            DefaultValue.RipplesDistortion = SkyboxController.Instance.RipplesDistortion;
            // General
            DefaultValue.Exposure = SkyboxController.Instance.Exposure;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void OnClick()
        {
            // Starfield
            SkyboxController.Instance.BackgroundColor = DefaultValue.BackgroundColor;
            BackgroundColorImage.color = DefaultValue.BackgroundColor;

            SkyboxController.Instance.StarsTint = DefaultValue.StarsTint;
            StarsTintImage.color = DefaultValue.StarsTint;

            SkyboxController.Instance.StarsBrightnessMin = DefaultValue.BrightnessMin;
            BrightnessMinSlider.value = DefaultValue.BrightnessMin;

            SkyboxController.Instance.StarsBrightnessMax = DefaultValue.BrightnessMax;
            BrightnessMaxSlider.value = DefaultValue.BrightnessMax;

            // Nebula Colors
            SkyboxController.Instance.AmbientTint = DefaultValue.BackgroundTint;
            var color = DefaultValue.BackgroundTint;
            BackgroundAlphaSlider.value = color.a;
            color.a = 1f;
            BackgroundTintImage.color = color;

            SkyboxController.Instance.BasementTint = DefaultValue.BasementTint;
            color = DefaultValue.BasementTint;
            BasementAlphaSlider.value = color.a;
            color.a = 1f;
            BasementTintImage.color = color;

            SkyboxController.Instance.RipplesTint1 = DefaultValue.RipplesTint1;
            color = DefaultValue.RipplesTint1;
            RipplesAlpha1Slider.value = color.a;
            color.a = 1f;
            RipplesTint1Image.color = color;

            SkyboxController.Instance.RipplesTint2 = DefaultValue.RipplesTint2;
            color = DefaultValue.RipplesTint2;
            RipplesAlpha2Slider.value = color.a;
            color.a = 1f;
            RipplesTint2Image.color = color;

            // Nebula Density
            SkyboxController.Instance.DensityRotation = DefaultValue.DensityRotation;
            DensityRotationX.value = DefaultValue.DensityRotation.x;
            DensityRotationY.value = DefaultValue.DensityRotation.y;
            DensityRotationZ.value = DefaultValue.DensityRotation.z;

            SkyboxController.Instance.DensityThresholdLow = DefaultValue.ThresholdLow;
            ThresholdLow.value = DefaultValue.ThresholdLow;

            SkyboxController.Instance.DensityThresholdHigh = DefaultValue.ThresholdHigh;
            ThresholdHigh.value = DefaultValue.ThresholdHigh;

            // Nebula Diffusion
            SkyboxController.Instance.RipplesDistortion = DefaultValue.RipplesDistortion;
            RipplesDistortionX.value = DefaultValue.RipplesDistortion.x;
            RipplesDistortionY.value = DefaultValue.RipplesDistortion.y;
            RipplesDistortionZ.value = DefaultValue.RipplesDistortion.z;

            // General
            SkyboxController.Instance.Exposure = DefaultValue.Exposure;
            ExposureSlider.value = DefaultValue.Exposure;
        }

        //---------------------------------------------------------------------
        // Nested
        //---------------------------------------------------------------------

        private static class DefaultValue
        {
            // Starfield
            public static Color BackgroundColor;
            public static Color StarsTint;
            public static float BrightnessMin;
            public static float BrightnessMax;
            // Nebula Colors
            public static Color BackgroundTint;
            public static Color BasementTint;
            public static Color RipplesTint1;
            public static Color RipplesTint2;
            // Nebula Density
            public static Vector3 DensityRotation;
            public static float ThresholdLow;
            public static float ThresholdHigh;
            // Nebula Diffusion
            public static Vector3 RipplesDistortion;
            // General
            public static float Exposure;
        }
    }
}