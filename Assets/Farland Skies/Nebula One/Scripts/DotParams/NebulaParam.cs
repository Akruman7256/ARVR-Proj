using System;
using Borodar.FarlandSkies.Core.DotParams;
using UnityEngine;

namespace Borodar.FarlandSkies.NebulaOne
{
    [Serializable]
    public class NebulaParam : DotParam
    {
        public Color BackgroundTint = Color.gray;
        public Color BasementTint = Color.gray;
        public Color RipplesTint1 = Color.gray;
        public Color RipplesTint2 = Color.gray;
    }
}