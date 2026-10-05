using System.Diagnostics.CodeAnalysis;
using Borodar.FarlandSkies.Core.DotParams;
using UnityEditor;
using UnityEngine;

namespace Borodar.FarlandSkies.NebulaOne
{
    [CustomPropertyDrawer(typeof(StarsParam))]
    public class StarsParamDrawer : BaseParamDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 3f * EditorGUIUtility.singleLineHeight + 7f * V_PAD;
        }

        [SuppressMessage("ReSharper", "InvertIf")]
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label);
            position.x += TIME_FIELD_WIDHT;
            position.y += 1.5f * V_PAD;
            var baseX = position.x;
            var baseWidth = position.width;

            // Stars Tint
            position.width -= TIME_FIELD_WIDHT;
            position.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("Tint"), GUIContent.none);

            // Brightness
            var brightnessMin = property.FindPropertyRelative("BrightnessMin");
            var brightnessMax = property.FindPropertyRelative("BrightnessMax");

            // Brightness Min
            position.x = baseX;
            position.y += position.height + 2f * V_PAD;
            position.width = baseWidth - TIME_FIELD_WIDHT;

            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(position, brightnessMin, GUIContent.none);
            if (EditorGUI.EndChangeCheck())
            {
                var minValue = brightnessMin.floatValue;
                var maxValue = brightnessMax.floatValue;
                if (minValue > maxValue) brightnessMin.floatValue = maxValue;
            }

            // Brightness Max
            position.x = baseX;
            position.y += position.height + 2f * V_PAD;

            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(position, brightnessMax, GUIContent.none);
            if (EditorGUI.EndChangeCheck())
            {
                var minValue = brightnessMin.floatValue;
                var maxValue = brightnessMax.floatValue;
                if (maxValue < minValue) brightnessMax.floatValue = minValue;
            }
        }
    }
}