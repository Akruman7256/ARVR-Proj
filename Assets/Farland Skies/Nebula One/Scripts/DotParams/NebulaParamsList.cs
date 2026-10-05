using System;
using Borodar.FarlandSkies.Core.DotParams;
using UnityEngine;

namespace Borodar.FarlandSkies.NebulaOne
{
    [Serializable]
    public class NebulaParamsList : SortedParamsList<NebulaParam>
    {
        public NebulaParam GetParamPerTime(float currentTime)
        {
            if (SortedParams.Count <= 0)
            {
                Debug.LogWarning("Nebula params list is empty");
                SortedParams.Add(0, new NebulaParam());
            }

            var index = SortedParams.FindIndexPerTime(currentTime);

            if (index < 1) index = SortedParams.Count;

            var timeKey1 = SortedParams.Keys[index - 1];
            var value = SortedParams.Values[index - 1];
            var backgroundTint1 = value.BackgroundTint;
            var basementTint1 = value.BasementTint;
            var ripplesTint11 = value.RipplesTint1;
            var ripplesTint21 = value.RipplesTint2;

            if (index >= SortedParams.Count) index = 0;

            var timeKey2 = SortedParams.Keys[index];
            value = SortedParams.Values[index];
            var topColor2 = value.BackgroundTint;
            var middleColor2 = value.BasementTint;
            var bottomColor2 = value.RipplesTint1;
            var cloudsTint2 = value.RipplesTint2;

            var t1 = (currentTime > timeKey1) ?  currentTime - timeKey1 : currentTime + (100f - timeKey1);
            var t2 = (timeKey1 < timeKey2) ? timeKey2 - timeKey1 : 100f + timeKey2 - timeKey1;
            var t = t1/t2;

            var currentParam = new NebulaParam
            {
                BackgroundTint = Color.Lerp(backgroundTint1, topColor2, t),
                BasementTint = Color.Lerp(basementTint1, middleColor2, t),
                RipplesTint1 = Color.Lerp(ripplesTint11, bottomColor2, t),
                RipplesTint2 = Color.Lerp(ripplesTint21, cloudsTint2, t)
            };

            return currentParam;
        }
    }
}