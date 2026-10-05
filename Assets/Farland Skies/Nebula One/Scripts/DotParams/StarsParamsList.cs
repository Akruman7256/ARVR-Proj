using System;
using Borodar.FarlandSkies.Core.DotParams;
using UnityEngine;

namespace Borodar.FarlandSkies.NebulaOne
{
    [Serializable]
    public class StarsParamsList : SortedParamsList<StarsParam>
    {
        public StarsParam GetParamPerTime(float currentTime)
        {
            if (SortedParams.Count <= 0)
            {
                Debug.LogWarning("Stars params list is empty");
                SortedParams.Add(0, new StarsParam());
            }

            var index = SortedParams.FindIndexPerTime(currentTime);

            if (index < 1) index = SortedParams.Count;

            var timeKey1 = SortedParams.Keys[index - 1];
            var value = SortedParams.Values[index - 1];
            var tint1 = value.Tint;
            var brightnessMin1 = value.BrightnessMin;
            var brightnessMax1 = value.BrightnessMax;

            if (index >= SortedParams.Count) index = 0;

            var timeKey2 = SortedParams.Keys[index];
            value = SortedParams.Values[index];
            var tint2 = value.Tint;
            var brightnessMin2 = value.BrightnessMin;
            var brightnessMax2 = value.BrightnessMax;

            var t1 = (currentTime > timeKey1) ?  currentTime - timeKey1 : currentTime + (100f - timeKey1);
            var t2 = (timeKey1 < timeKey2) ? timeKey2 - timeKey1 : 100f + timeKey2 - timeKey1;
            var t = t1/t2;

            var currentParam = new StarsParam
            {
                Tint = Color.Lerp(tint1, tint2, t),
                BrightnessMin = Mathf.Lerp(brightnessMin1, brightnessMin2, t),
                BrightnessMax = Mathf.Lerp(brightnessMax1, brightnessMax2, t)
            };

            return currentParam;
        }
    }
}