Shader "Custom/AtmosphereRim"
{
    Properties
    {
        _RimColor ("Atmosphere Color", Color) = (0.35, 0.6, 1.0, 1.0)
        _RimPower ("Rim Falloff (higher = thinner glow)", Range(0.5, 8.0)) = 3.0
        _RimIntensity ("Intensity", Range(0.0, 5.0)) = 1.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha One   // Additive-ish blend so it glows rather than occludes
        ZWrite Off
        // Default culling (front faces only) - do NOT use Cull Front here.
        // Cull Front renders the shell's far/inner faces, where dot(normal,viewDir)
        // is negative everywhere and gets clamped to 0 by saturate(), which makes
        // the fresnel term hit its maximum across the WHOLE visible hemisphere
        // instead of just the silhouette edge - that's what caused the flat,
        // solid-color look instead of a thin glowing rim.

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS    : TEXCOORD0;
                float3 viewDirWS   : TEXCOORD1;
            };

            float4 _RimColor;
            float _RimPower;
            float _RimIntensity;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs positions = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionHCS = positions.positionCS;
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.viewDirWS = GetWorldSpaceViewDir(positions.positionWS);
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float3 normal = normalize(IN.normalWS);
                float3 viewDir = normalize(IN.viewDirWS);

                // Fresnel term: strong at grazing angles (the planet's silhouette edge),
                // near-zero facing the camera directly - this is what creates the
                // "thin glowing rim around the edge" atmospheric-scattering look.
                float fresnel = 1.0 - saturate(dot(normal, viewDir));
                fresnel = pow(fresnel, _RimPower);

                float3 color = _RimColor.rgb * fresnel * _RimIntensity;
                return float4(color, fresnel * _RimColor.a);
            }
            ENDHLSL
        }
    }
}
