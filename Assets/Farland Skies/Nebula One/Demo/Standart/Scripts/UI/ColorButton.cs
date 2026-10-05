using System;
using Borodar.FarlandSkies.Core.Demo;
using UnityEngine;

namespace Borodar.FarlandSkies.NebulaOne
{
    public class ColorButton : BaseColorButton
    {
        public ColorType SkyColorType;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected void Start()
        {
            switch (SkyColorType)
            {
                case ColorType.BackgroundColor:
                    UpdateColorImage(SkyboxController.Instance.BackgroundColor);
                    break;
                case ColorType.StarsTint:
                    UpdateColorImage(SkyboxController.Instance.StarsTint);
                    break;
                case ColorType.NebulaBackgroundTint:
                    UpdateColorImage(SkyboxController.Instance.AmbientTint);
                    break;
                case ColorType.NebulaBasementTint:
                    UpdateColorImage(SkyboxController.Instance.BasementTint);
                    break;
                case ColorType.NebulaRipplesTint1:
                    UpdateColorImage(SkyboxController.Instance.RipplesTint1);
                    break;
                case ColorType.NebulaRipplesTint2:
                    UpdateColorImage(SkyboxController.Instance.RipplesTint2);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public override void ChangeColor(Color color)
        {
            base.ChangeColor(color);

            switch (SkyColorType)
            {
                case ColorType.BackgroundColor:
                    SkyboxController.Instance.BackgroundColor = color;
                    break;
                case ColorType.StarsTint:
                    color.a = SkyboxController.Instance.StarsTint.a;
                    SkyboxController.Instance.StarsTint = color;
                    break;
                case ColorType.NebulaBackgroundTint:
                    color.a = SkyboxController.Instance.AmbientTint.a;
                    SkyboxController.Instance.AmbientTint = color;
                    break;
                case ColorType.NebulaBasementTint:
                    color.a = SkyboxController.Instance.BasementTint.a;
                    SkyboxController.Instance.BasementTint = color;
                    break;
                case ColorType.NebulaRipplesTint1:
                    color.a = SkyboxController.Instance.RipplesTint1.a;
                    SkyboxController.Instance.RipplesTint1 = color;
                    break;
                case ColorType.NebulaRipplesTint2:
                    color.a = SkyboxController.Instance.RipplesTint2.a;
                    SkyboxController.Instance.RipplesTint2 = color;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        //---------------------------------------------------------------------
        // Helpres
        //---------------------------------------------------------------------

        private void UpdateColorImage(Color color)
        {
            color.a = 1f;
            ColorImage.color = color;
        }

        //---------------------------------------------------------------------
        // Nested
        //---------------------------------------------------------------------

        public enum ColorType
        {
            BackgroundColor,
            StarsTint,
            NebulaBackgroundTint,
            NebulaBasementTint,
            NebulaRipplesTint1,
            NebulaRipplesTint2
        }
    }
}