using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using UnityEditor;

[UsedImplicitly]
[SuppressMessage("ReSharper", "CheckNamespace")]
public class NebulaOneShaderGUI : ShaderGUI
{
    // Starfield
    private MaterialProperty _starfieldCube;
    private MaterialProperty _backgroundColor;
    private MaterialProperty _starsTint;
    private MaterialProperty _starsBrightnesslMin;
    private MaterialProperty _starsBrightnesslMax;
    // Nebula Density
    private MaterialProperty _densityCube;
    private MaterialProperty _densityThresholdLow;
    private MaterialProperty _densityThresholdHigh;
    // Nebula Diffusion
    private MaterialProperty _diffusionCube;
    private MaterialProperty _ripplesDistortion;
    // Nebula Colors
    private MaterialProperty _ambientTint;
    private MaterialProperty _basementTint;
    private MaterialProperty _ripplesTint1;
    private MaterialProperty _ripplesTint2;
    // General
    private MaterialProperty _exposure;

    //---------------------------------------------------------------------
    // Public
    //---------------------------------------------------------------------

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        FindProperties(properties);
        ShaderPropertiesGUI(materialEditor);
    }

    //---------------------------------------------------------------------
    // Helpers
    //---------------------------------------------------------------------

    private void FindProperties(MaterialProperty[] properties)
    {
        // Starfield
        _starfieldCube = FindProperty("_StarfieldCube", properties);
        _backgroundColor = FindProperty("_BackgroundColor", properties);
        _starsTint = FindProperty("_StarsTint", properties);
        _starsBrightnesslMin = FindProperty("_StarsBrightnesslMin", properties);
        _starsBrightnesslMax = FindProperty("_StarsBrightnesslMax", properties);
        // Nebula Density
        _densityCube = FindProperty("_DensityCube", properties);
        _densityThresholdLow = FindProperty("_DensityThresholdLow", properties);
        _densityThresholdHigh = FindProperty("_DensityThresholdHigh", properties);
        // Nebula Diffusion
        _diffusionCube = FindProperty("_DiffusionCube", properties);
        _ripplesDistortion = FindProperty("_RipplesDistortion", properties);
        // Nebula Colors
        _ambientTint = FindProperty("_AmbientTint", properties);
        _basementTint = FindProperty("_BasementTint", properties);
        _ripplesTint1 = FindProperty("_RipplesTint1", properties);
        _ripplesTint2 = FindProperty("_RipplesTint2", properties);
        // General
        _exposure = FindProperty("_Exposure", properties);
    }

    private void ShaderPropertiesGUI(MaterialEditor materialEditor)
    {
        materialEditor.SetDefaultGUIWidths();

        // Starfield
        EditorGUILayout.BeginVertical("Box");
        EditorGUILayout.LabelField("Starfield", EditorStyles.boldLabel);
        EditorGUILayout.EndVertical();

        materialEditor.ShaderProperty(_starfieldCube, "Starfield Cubemap");
        materialEditor.ShaderProperty(_backgroundColor, "Background Color");
        materialEditor.ShaderProperty(_starsTint, "Stars Tint");
        materialEditor.ShaderProperty(_starsBrightnesslMin, "Brightness Min");
        materialEditor.ShaderProperty(_starsBrightnesslMax, "Brightness Max");
        EditorGUILayout.Space();

        // Nebula Density
        EditorGUILayout.BeginVertical("Box");
        EditorGUILayout.LabelField("Nebula Density", EditorStyles.boldLabel);
        EditorGUILayout.EndVertical();

        materialEditor.ShaderProperty(_densityCube, "Density Cubemap");
        materialEditor.ShaderProperty(_densityThresholdLow, "Density Threshold Low");
        materialEditor.ShaderProperty(_densityThresholdHigh, "Density Threshold High");
        EditorGUILayout.Space();

        // Nebula Diffusion
        EditorGUILayout.BeginVertical("Box");
        EditorGUILayout.LabelField("Nebula Diffusion", EditorStyles.boldLabel);
        EditorGUILayout.EndVertical();

        materialEditor.ShaderProperty(_diffusionCube, "Diffusion Cubemap");
        materialEditor.ShaderProperty(_ripplesDistortion, "Ripples Distortion");

        // Nebula Colors
        EditorGUILayout.BeginVertical("Box");
        EditorGUILayout.LabelField("Nebula Colors", EditorStyles.boldLabel);
        EditorGUILayout.EndVertical();

        materialEditor.ShaderProperty(_ambientTint, "Ambient Tint");
        materialEditor.ShaderProperty(_basementTint, "Basement Tint");
        materialEditor.ShaderProperty(_ripplesTint1, "Ripples 1 Tint");
        materialEditor.ShaderProperty(_ripplesTint2, "Ripples 2 Tint");
        EditorGUILayout.Space();

        // General
        EditorGUILayout.BeginVertical("Box");
        EditorGUILayout.LabelField("General", EditorStyles.boldLabel);
        EditorGUILayout.EndVertical();

        materialEditor.ShaderProperty(_exposure, "Exposure");
    }
}