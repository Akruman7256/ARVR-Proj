using Borodar.FarlandSkies.Core.DotParams;
using UnityEditor;
using UnityEngine;

namespace Borodar.FarlandSkies.NebulaOne
{
    [CustomPropertyDrawer(typeof(BackgroundParam))]
    public class BackgroundParamDrawer : BaseParamDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label);
            position.x += TIME_FIELD_WIDHT;
            position.y += V_PAD;
            // Background Tint
            position.width -= TIME_FIELD_WIDHT;
            position.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("BackgroundColor"), GUIContent.none);
        }
    }
}