using System.Diagnostics.CodeAnalysis;
using Borodar.FarlandSkies.Core.DotParams;
using Borodar.FarlandSkies.Core.Editor;
using UnityEditor;
using UnityEngine;

namespace Borodar.FarlandSkies.NebulaOne
{
    [CustomEditor(typeof(SkyboxAnimator))]
    public class SkyboxAnimatorEditor : Editor
    {
        private const float LIST_CONTROLS_PAD = 20f;
        private const float TIME_WIDTH = BaseParamDrawer.TIME_FIELD_WIDHT + LIST_CONTROLS_PAD;

        private SerializedProperty _rotationSpeed;
        private SerializedProperty _distortionSpeed;
        private SerializedProperty _maxDistortionValue;

        private ParamsReorderableList _backgroundDotParamsList;
        private ParamsReorderableList _starsDotParamsList;
        private ParamsReorderableList _nebulaDotParamsList;
                
        private SerializedProperty _framesInterval;

        private static bool _showBackgroundDotParams;
        private static bool _showStarsDotParams;
        private static bool _showNebulaDotParams;

        private GUIContent _guiContent;
        private GUIContent _backgroundParamsLabel;
        private GUIContent _starsParamsLabel;
        private GUIContent _nebulaParamsLabel;
        private GUIContent _rotationSpeedLabel;
        private GUIContent _distortionSpeedLabel;
        private GUIContent _maxDistortionValueLabel;
        private GUIContent _framesIntervalLabel;

        protected void OnEnable()
        {
            _guiContent = new GUIContent();
            _backgroundParamsLabel = new GUIContent("Background Color", "List of starfield background colors for one animation cycle");
            _starsParamsLabel = new GUIContent("Stars Params", "List of stars tint colors and brightness thresholds for one animation cycle");
            _nebulaParamsLabel = new GUIContent("Nebula Colors", "List of nebula colors for one animation cycle");
            _rotationSpeedLabel = new GUIContent("Rotation Speed", "Specifies the angular speed coefficients of the nebula around corresponding axes");
            _distortionSpeedLabel = new GUIContent("Distortion Speed", "Coefficient that determines how fast nebula distortion will be changing over time");
            _maxDistortionValueLabel = new GUIContent("Max Distortion Value", "Maximum nebula ripples distortion during an animation cycle");
            _framesIntervalLabel = new GUIContent("Frames Interval", "Reduce the skybox animation update to run every \"n\" frames");

            _rotationSpeed = serializedObject.FindProperty("_rotationSpeed");
            _distortionSpeed = serializedObject.FindProperty("_distortionSpeed");
            _maxDistortionValue = serializedObject.FindProperty("_maxDistortionValue");

            var backgroundDotParams = serializedObject.FindProperty("_backgroundParamsList").FindPropertyRelative("Params");
            _backgroundDotParamsList = new ParamsReorderableList(backgroundDotParams, new BackgroundParamDrawer());

            var starsDotParams = serializedObject.FindProperty("_starsParamsList").FindPropertyRelative("Params");
            _starsDotParamsList = new ParamsReorderableList(starsDotParams, new StarsParamDrawer());

            var nebulaDotParams = serializedObject.FindProperty("_nebulaParamsList").FindPropertyRelative("Params");
            _nebulaDotParamsList = new ParamsReorderableList(nebulaDotParams, new NebulaParamDrawer());
            
            _framesInterval = serializedObject.FindProperty("_framesInterval");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            CustomGUILayout();
            serializedObject.ApplyModifiedProperties();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        [SuppressMessage("ReSharper", "InvertIf")]
        private void CustomGUILayout()
        {
            // Rate dialog          
            RateMeDialog.DrawRateDialog(AssetInfo.ASSET_NAME, AssetInfo.ASSET_STORE_ID);

            // Starfield

            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Starfield", EditorStyles.boldLabel);
            EditorGUILayout.EndVertical();

            _showBackgroundDotParams = EditorGUILayout.Foldout(_showBackgroundDotParams, _backgroundParamsLabel);
            EditorGUILayout.Space();
            if (_showBackgroundDotParams)
            {
                BackgroundParamsHeader();
                _backgroundDotParamsList.DoLayoutList();
            }

            _showStarsDotParams = EditorGUILayout.Foldout(_showStarsDotParams, _starsParamsLabel);
            EditorGUILayout.Space();
            if (_showStarsDotParams)
            {
                StarsParamsHeader();
                _starsDotParamsList.DoLayoutList();
            }

            // Nebula

            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Nebula", EditorStyles.boldLabel);
            EditorGUILayout.EndVertical();

            _rotationSpeed.vector3Value = EditorGUILayout.Vector3Field(_rotationSpeedLabel, _rotationSpeed.vector3Value);
            _distortionSpeed.floatValue = EditorGUILayout.FloatField(_distortionSpeedLabel, _distortionSpeed.floatValue);
            _maxDistortionValue.floatValue = EditorGUILayout.Slider(_maxDistortionValueLabel, _maxDistortionValue.floatValue, 0f, 0.5f);

            EditorGUILayout.Space();

            _showNebulaDotParams = EditorGUILayout.Foldout(_showNebulaDotParams, _nebulaParamsLabel);
            EditorGUILayout.Space();
            if (_showNebulaDotParams)
            {
                NebulaParamsHeader();
                _nebulaDotParamsList.DoLayoutList();
            }
            
            // General
            
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("General", EditorStyles.boldLabel);
            EditorGUILayout.EndVertical();
            
            _framesInterval.intValue = EditorGUILayout.IntSlider(_framesIntervalLabel, _framesInterval.intValue, 1, 60);
        }

        private void BackgroundParamsHeader()
        {
            var position = GUILayoutUtility.GetRect(_guiContent, ParamsReorderableList.Title, GUILayout.ExpandWidth(true));
            if (Event.current.type == EventType.Repaint)
            {
                var baseWidht = position.width;
                // Time
                position.width = TIME_WIDTH;
                _guiContent.text = "Progress";
                ParamsReorderableList.Title.Draw(position, _guiContent, false, false, false, false);
                // Background Color
                position.x += position.width;
                position.width = baseWidht - position.width;
                _guiContent.text = "Color";
                ParamsReorderableList.Title.Draw(position, _guiContent, false, false, false, false);
            }
        }

        private void StarsParamsHeader()
        {
            var position = GUILayoutUtility.GetRect(_guiContent, ParamsReorderableList.Title, GUILayout.ExpandWidth(true));
            if (Event.current.type == EventType.Repaint)
            {
                var baseWidht = position.width;
                var baseHeight = position.height;
                // Time
                position.width = TIME_WIDTH;
                position.height *= 2f;
                _guiContent.text = "Progress";
                ParamsReorderableList.Title.Draw(position, _guiContent, false, false, false, false);
                // Tint
                position.x += position.width;
                position.width = baseWidht - position.width;
                position.height = baseHeight;
                _guiContent.text = "Tint";
                ParamsReorderableList.Title.Draw(position, _guiContent, false, false, false, false);
            }
            GUILayout.Space(-5f);
            position = GUILayoutUtility.GetRect(_guiContent, ParamsReorderableList.Title, GUILayout.ExpandWidth(true));
            if (Event.current.type == EventType.Repaint)
            {
                // Light Intencity
                position.x += TIME_WIDTH;
                position.width -= TIME_WIDTH;
                _guiContent.text = "Brightness Min/Max";
                ParamsReorderableList.Title.Draw(position, _guiContent, false, false, false, false);
            }
        }

        private void NebulaParamsHeader()
        {
            var position = GUILayoutUtility.GetRect(_guiContent, ParamsReorderableList.Title, GUILayout.ExpandWidth(true));
            if (Event.current.type == EventType.Repaint)
            {
                var baseWidht = position.width;
                // Progress
                position.width = TIME_WIDTH;
                _guiContent.text = "Progress";
                ParamsReorderableList.Title.Draw(position, _guiContent, false, false, false, false);
                // Background Tint
                position.x += position.width;
                position.width = (baseWidht - position.width - LIST_CONTROLS_PAD) / 4f;
                _guiContent.text = "Background";
                ParamsReorderableList.Title.Draw(position, _guiContent, false, false, false, false);
                // Basement Tint
                position.x += position.width;
                _guiContent.text = "Basement";
                ParamsReorderableList.Title.Draw(position, _guiContent, false, false, false, false);
                // Ripples Tint 1
                position.x += position.width;
                _guiContent.text = "Ripples 1";
                ParamsReorderableList.Title.Draw(position, _guiContent, false, false, false, false);
                // Ripples Tint 2
                position.x += position.width;
                position.width += LIST_CONTROLS_PAD;
                _guiContent.text = "Ripples 2";
                ParamsReorderableList.Title.Draw(position, _guiContent, false, false, false, false);
            }
        }
    }
}