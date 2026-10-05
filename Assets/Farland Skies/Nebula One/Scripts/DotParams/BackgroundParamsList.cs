using System;
using Borodar.FarlandSkies.Core.DotParams;
using UnityEngine;

namespace Borodar.FarlandSkies.NebulaOne
{
    [Serializable]
    public class BackgroundParamsList : SortedParamsList<BackgroundParam>
    {
        public BackgroundParam GetParamPerTime(float currentTime)
        {
            if (SortedParams.Count <= 0)
            {
                Debug.LogWarning("Background params list is empty");
                SortedParams.Add(0, new BackgroundParam());
            }

            var index = SortedParams.FindIndexPerTime(currentTime);

            if (index < 1) index = SortedParams.Count;

            var timeKey1 = SortedParams.Keys[index - 1];
            var value = SortedParams.Values[index - 1];
            var backgroundColor = value.BackgroundColor;

            if (index >= SortedParams.Count) index = 0;

            var timeKey2 = SortedParams.Keys[index];
            value = SortedParams.Values[index];
            var backgroundColor2 = value.BackgroundColor;

            var t1 = (currentTime > timeKey1) ?  currentTime - timeKey1 : currentTime + (100f - timeKey1);
            var t2 = (timeKey1 < timeKey2) ? timeKey2 - timeKey1 : 100f + timeKey2 - timeKey1;
            var t = t1/t2;

            var currentParam = new BackgroundParam
            {
                BackgroundColor = Color.Lerp(backgroundColor, backgroundColor2, t)
            };

            return currentParam;
        }
    }
}