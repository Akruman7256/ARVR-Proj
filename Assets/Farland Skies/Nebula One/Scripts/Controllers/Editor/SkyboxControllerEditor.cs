using Borodar.FarlandSkies.Core.Editor;
using UnityEditor;
using UnityEngine;

namespace Borodar.FarlandSkies.NebulaOne
{
    [CustomEditor(typeof(SkyboxController))]
    public class SkyboxControllerEditor : Editor
    {
        // Skybox
        private SerializedProperty _skyboxMaterial;
        // Starfield
        private SerializedProperty _starfieldCubemap;
        private SerializedProperty _starfieldRotation;
        private SerializedProperty _backgroundColor;
        private SerializedProperty _starsTint;
        private SerializedProperty _starsBrightnessMin;
        private SerializedProperty _starsBrightnessMax;
        // Nebula Colors
        private SerializedProperty _ambientTint;
        private SerializedProperty _basementTint;
        private SerializedProperty _ripplesTint1;
        private SerializedProperty _ripplesTint2;
        // Nebula Density
        private SerializedProperty _densityCubemap;
        private SerializedProperty _densityRotation;
        private SerializedProperty _densityThresholdLow;
        private SerializedProperty _densityThresholdHigh;
        // Nebula Diffusion
        private SerializedProperty _diffusionCubemap;
        private SerializedProperty _ripplesDistortion;
        // General
        private SerializedProperty _exposure;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            CustomGUILayout();
            serializedObject.ApplyModifiedProperties();
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected void OnEnable()
        {
            // Skybox
            _skyboxMaterial = serializedObject.FindProperty("SkyboxMaterial");
            // Starfield
            _starfieldCubemap = serializedObject.FindProperty("_starfieldCubemap");
            _starfieldRotation = serializedObject.FindProperty("_starfieldRotation");
            _backgroundColor = serializedObject.FindProperty("_backgroundColor");
            _starsTint = serializedObject.FindProperty("_starsTint");
            _starsBrightnessMin = serializedObject.FindProperty("_starsBrightnessMin");
            _starsBrightnessMax = serializedObject.FindProperty("_starsBrightnessMax");
            // Nebula Colors
            _ambientTint = serializedObject.FindProperty("_ambientTint");
            _basementTint = serializedObject.FindProperty("_basementTint");
            _ripplesTint1 = serializedObject.FindProperty("_ripplesTint1");
            _ripplesTint2 = serializedObject.FindProperty("_ripplesTint2");
            // Nebula Density
            _densityCubemap = serializedObject.FindProperty("_densityCubemap");
            _densityRotation = serializedObject.FindProperty("_densityRotation");
            _densityThresholdLow = serializedObject.FindProperty("_densityThresholdLow");
            _densityThresholdHigh = serializedObject.FindProperty("_densityThresholdHigh");
            // Nebula Diffusion
            _diffusionCubemap = serializedObject.FindProperty("_diffusionCubemap");
            _ripplesDistortion = serializedObject.FindProperty("_ripplesDistortion");
            // General
            _exposure = serializedObject.FindProperty("_exposure");
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void CustomGUILayout()
        {
            // Rate dialog          
            RateMeDialog.DrawRateDialog(AssetInfo.ASSET_NAME, AssetInfo.ASSET_STORE_ID);

            // Skybox
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_skyboxMaterial);
            EditorGUILayout.Space();

            // Starfield
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Starfield", EditorStyles.boldLabel);
            EditorGUILayout.EndVertical();

            EditorGUILayout.PropertyField(_starfieldCubemap, new GUIContent("Cubemap"));
            EditorGUILayout.PropertyField(_starfieldRotation, new GUIContent("Rotation"));
            EditorGUILayout.PropertyField(_backgroundColor, new GUIContent("Background Color"));
            EditorGUILayout.PropertyField(_starsTint, new GUIContent("Stars Tint"));

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_starsBrightnessMin, new GUIContent("Brightness Min"));
            if (EditorGUI.EndChangeCheck())
            {
                var minValue = _starsBrightnessMin.floatValue;
                var maxValue = _starsBrightnessMax.floatValue;
                if (minValue > maxValue) _starsBrightnessMin.floatValue = maxValue;
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_starsBrightnessMax, new GUIContent("Brightness Max"));
            if (EditorGUI.EndChangeCheck())
            {
                var minValue = _starsBrightnessMin.floatValue;
                var maxValue = _starsBrightnessMax.floatValue;
                if (maxValue < minValue) _starsBrightnessMax.floatValue = minValue;
            }

            EditorGUILayout.Space();

            // Nebula Colors
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Nebula", EditorStyles.boldLabel);
            EditorGUILayout.EndVertical();

            EditorGUILayout.PropertyField(_ambientTint, new GUIContent("Ambient Tint"));
            EditorGUILayout.PropertyField(_basementTint, new GUIContent("Basement Tint"));
            EditorGUILayout.PropertyField(_ripplesTint1, new GUIContent("Ripples Tint 1"));
            EditorGUILayout.PropertyField(_ripplesTint2, new GUIContent("Ripples Tint 2"));
            EditorGUILayout.Space();

            // Nebula Density
            EditorGUILayout.LabelField("Nebula Density", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_densityCubemap, new GUIContent("Cubemap"));
            EditorGUILayout.PropertyField(_densityRotation, new GUIContent("Rotation"));
            EditorGUILayout.PropertyField(_densityThresholdLow, new GUIContent("Threshold Low"));
            EditorGUILayout.PropertyField(_densityThresholdHigh, new GUIContent("Threshold High"));
            EditorGUILayout.Space();

            // Nebula Diffusion
            EditorGUILayout.LabelField("Nebula Diffusion", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_diffusionCubemap, new GUIContent("Cubemap"));
            EditorGUILayout.PropertyField(_ripplesDistortion);
            EditorGUILayout.Space();

            // General
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("General", EditorStyles.boldLabel);
            EditorGUILayout.EndVertical();

            EditorGUILayout.PropertyField(_exposure);
            EditorGUILayout.Space();
        }
    }
}