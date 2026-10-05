using System;
using Borodar.FarlandSkies.Core.DotParams;
using UnityEngine;

namespace Borodar.FarlandSkies.NebulaOne
{
    [Serializable]
    public class StarsParam : DotParam
    {
        public Color Tint = Color.gray;
        [Range(0, 1f)]
        public float BrightnessMin = 0f;
        [Range(0, 1f)]
        public float BrightnessMax = 1f;
    }
}