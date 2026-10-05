using System;
using UnityEngine;
using UnityEngine.UI;

namespace Borodar.FarlandSkies.NebulaOne
{
    public class PropertySlider : MonoBehaviour
    {
        public Type SliderType;
        private Slider _slider;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected void Awake()
        {
            _slider = GetComponent<Slider>();
        }

        protected void Start()
        {
            switch (SliderType)
            {
                case Type.StarsBrightnessMin:
                    _slider.value = SkyboxController.Instance.StarsBrightnessMin;
                    break;
                case Type.StarsBrightnessMax:
                    _slider.value = SkyboxController.Instance.StarsBrightnessMax;
                    break;
                case Type.NebulaBackgroundAlpha:
                    _slider.value = SkyboxController.Instance.AmbientTint.a;
                    break;
                case Type.NebulaBasementAlpha:
                    _slider.value = SkyboxController.Instance.BasementTint.a;
                    break;
                case Type.NebulaRipples1Alpha:
                    _slider.value = SkyboxController.Instance.RipplesTint1.a;
                    break;
                case Type.NebulaRipples2Alpha:
                    _slider.value = SkyboxController.Instance.RipplesTint2.a;
                    break;
                case Type.NebulaRotationX:
                    _slider.value = SkyboxController.Instance.DensityRotation.x;
                    break;
                case Type.NebulaRotationY:
                    _slider.value = SkyboxController.Instance.DensityRotation.y;
                    break;
                case Type.NebulaRotationZ:
                    _slider.value = SkyboxController.Instance.DensityRotation.z;
                    break;
                case Type.NebulaThresholdLow:
                    _slider.value = SkyboxController.Instance.DensityThresholdLow;
                    break;
                case Type.NebulaThresholdHigh:
                    _slider.value = SkyboxController.Instance.DensityThresholdHigh;
                    break;
                case Type.NebulaRipplesDistortionX:
                    _slider.value = SkyboxController.Instance.RipplesDistortion.x;
                    break;
                case Type.NebulaRipplesDistortionY:
                    _slider.value = SkyboxController.Instance.RipplesDistortion.y;
                    break;
                case Type.NebulaRipplesDistortionZ:
                    _slider.value = SkyboxController.Instance.RipplesDistortion.z;
                    break;
                case Type.Exposure:
                    _slider.value = SkyboxController.Instance.Exposure;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void OnValueChanged(float value)
        {
            var skyboxController = SkyboxController.Instance;
            switch (SliderType)
            {
                case Type.StarsBrightnessMin:
                {
                    _slider.value = Mathf.Min(value, skyboxController.StarsBrightnessMax);
                    skyboxController.StarsBrightnessMin = _slider.value;
                    break;
                }
                case Type.StarsBrightnessMax:
                {
                    _slider.value = Mathf.Max(value, skyboxController.StarsBrightnessMin);
                    skyboxController.StarsBrightnessMax = _slider.value;
                    break;
                }
                case Type.NebulaBackgroundAlpha:
                {
                    var color = skyboxController.AmbientTint;
                    color.a = value;
                    skyboxController.AmbientTint = color;
                    break;
                }
                case Type.NebulaBasementAlpha:
                {
                    var color = skyboxController.BasementTint;
                    color.a = value;
                    skyboxController.BasementTint = color;
                    break;
                }
                case Type.NebulaRipples1Alpha:
                {
                    var color = skyboxController.RipplesTint1;
                    color.a = value;
                    skyboxController.RipplesTint1 = color;
                    break;
                }
                case Type.NebulaRipples2Alpha:
                {
                    var color = skyboxController.RipplesTint2;
                    color.a = value;
                    skyboxController.RipplesTint2 = color;
                    break;
                }
                case Type.NebulaRotationX:
                {
                    var vector = skyboxController.DensityRotation;
                    vector.x = value;
                    skyboxController.DensityRotation = vector;
                    break;
                }
                case Type.NebulaRotationY:
                {
                    var vector = skyboxController.DensityRotation;
                    vector.y = value;
                    skyboxController.DensityRotation = vector;
                    break;
                }
                case Type.NebulaRotationZ:
                {
                    var vector = skyboxController.DensityRotation;
                    vector.z = value;
                    skyboxController.DensityRotation = vector;
                    break;
                }
                case Type.NebulaThresholdLow:
                {
                    skyboxController.DensityThresholdLow = value;
                    break;
                }
                case Type.NebulaThresholdHigh:
                {
                    skyboxController.DensityThresholdHigh = value;
                    break;
                }
                case Type.NebulaRipplesDistortionX:
                {
                    var vector = skyboxController.RipplesDistortion;
                    vector.x = value;
                    skyboxController.RipplesDistortion = vector;
                    break;
                }
                case Type.NebulaRipplesDistortionY:
                {
                    var vector = skyboxController.RipplesDistortion;
                    vector.y = value;
                    skyboxController.RipplesDistortion = vector;
                    break;
                }
                case Type.NebulaRipplesDistortionZ:
                {
                    var vector = skyboxController.RipplesDistortion;
                    vector.z = value;
                    skyboxController.RipplesDistortion = vector;
                    break;
                }
                case Type.Exposure:
                {
                    skyboxController.Exposure = value;
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        //---------------------------------------------------------------------
        // Nested
        //---------------------------------------------------------------------

        public enum Type
        {
            StarsBrightnessMin,
            StarsBrightnessMax,
            NebulaBackgroundAlpha,
            NebulaBasementAlpha,
            NebulaRipples1Alpha,
            NebulaRipples2Alpha,
            NebulaRotationX,
            NebulaRotationY,
            NebulaRotationZ,
            NebulaThresholdLow,
            NebulaThresholdHigh,
            NebulaRipplesDistortionX,
            NebulaRipplesDistortionY,
            NebulaRipplesDistortionZ,
            Exposure
        }
    }
}