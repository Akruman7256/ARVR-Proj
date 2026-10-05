using Borodar.FarlandSkies.Core.DotParams;
using UnityEditor;
using UnityEngine;

namespace Borodar.FarlandSkies.NebulaOne
{
    [CustomPropertyDrawer(typeof(NebulaParam))]
    public class NebulaParamDrawer : BaseParamDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label);
            position.x += TIME_FIELD_WIDHT;
            position.y += V_PAD;
            // Background Tint
            position.width = (position.width - TIME_FIELD_WIDHT - 3f * H_PAD) / 4f;
            position.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("BackgroundTint"), GUIContent.none);
            // Basement Tint
            position.x += position.width + H_PAD;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("BasementTint"), GUIContent.none);
            // Ripples Tint 1
            position.x += position.width + H_PAD;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("RipplesTint1"), GUIContent.none);
            // Ripples Tint 2
            position.x += position.width + H_PAD;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("RipplesTint2"), GUIContent.none);
        }
    }
}