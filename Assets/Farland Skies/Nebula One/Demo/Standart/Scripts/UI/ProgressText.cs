using System;
using UnityEngine;
using UnityEngine.UI;

namespace Borodar.FarlandSkies.NebulaOne
{
    public class ProgressText : MonoBehaviour
    {
        private Text _text;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected void Awake()
        {
            _text = GetComponent<Text>();
        }

        protected void Update()
        {
            _text.text = SkyboxCycleManager.Instance.CycleProgress.ToString("F") + " %";
        }
    }
}