// Procedural Sun photosphere for URP (Unity 6). Put on a sphere.
// Features: animated granulation cells, domain-warped plasma turbulence,
// sunspots (umbra + penumbra), latitude-dependent differential rotation,
// physically-inspired limb darkening + limb reddening. HDR output for Bloom.
Shader "Custom/Sun/Surface"
{
    Properties
    {
        [Header(Colors)]
        [HDR] _HotColor  ("Hot (granule centres)",       Color) = (1.00, 0.93, 0.62, 1)
        [HDR] _MidColor  ("Mid",                         Color) = (1.00, 0.55, 0.12, 1)
        [HDR] _ColdColor ("Cool (lanes, limb, sunspots)", Color) = (0.50, 0.10, 0.02, 1)
        _Intensity       ("Emission Intensity", Range(0.5, 8)) = 2.5
        _Contrast        ("Detail Contrast",    Range(0.5, 3)) = 1.5

        [Header(Granulation)]
        _GranulationScale ("Granule Scale",  Range(2, 60))     = 14
        _GranulationSpeed ("Granule Boil Speed", Range(0, 2))  = 0.35
        _LaneWidth        ("Dark Lane Width", Range(0.05, 0.6)) = 0.22

        [Header(Plasma Turbulence)]
        _WarpScale    ("Warp Scale",    Range(0.2, 5))   = 1.5
        _WarpStrength ("Warp Strength", Range(0, 0.5))   = 0.14
        _TurbScale    ("Turbulence Scale", Range(1, 20)) = 6
        _FlowSpeed    ("Flow Speed",    Range(0, 1))     = 0.15

        [Header(Rotation)]
        _RotationSpeed ("Equator Rotation (rad/s)", Range(-0.3, 0.3)) = 0.03
        _DiffRotation  ("Differential Rotation",   Range(0, 0.6))   = 0.35

        [Header(Sunspots)]
        _SpotScale     ("Spot Scale",     Range(0.5, 6)) = 2.2
        _SpotThreshold ("Spot Threshold (higher = fewer)", Range(0.4, 0.9)) = 0.62
        _SpotStrength  ("Spot Darkness",  Range(0, 1))   = 0.8

        [Header(Limb)]
        _LimbDarkening ("Limb Darkening",  Range(0, 1)) = 0.65
        _LimbReddening ("Limb Reddening",  Range(0.1, 3)) = 0.6
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode"="UniversalForward" }
            Cull Back
            ZWrite On

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "SunNoise.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _HotColor;
                float4 _MidColor;
                float4 _ColdColor;
                float  _Intensity;
                float  _Contrast;
                float  _GranulationScale;
                float  _GranulationSpeed;
                float  _LaneWidth;
                float  _WarpScale;
                float  _WarpStrength;
                float  _TurbScale;
                float  _FlowSpeed;
                float  _RotationSpeed;
                float  _DiffRotation;
                float  _SpotScale;
                float  _SpotThreshold;
                float  _SpotStrength;
                float  _LimbDarkening;
                float  _LimbReddening;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 dirOS      : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float3 viewDirWS  : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                float3 posWS = TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(posWS);
                o.dirOS      = v.positionOS.xyz;
                o.normalWS   = TransformObjectToWorldNormal(v.normalOS);
                o.viewDirWS  = GetWorldSpaceViewDir(posWS);
                return o;
            }

            float3 HeatRamp(float heat)
            {
                float3 lo = lerp(_ColdColor.rgb, _MidColor.rgb, saturate(heat * 2.0));
                return lerp(lo, _HotColor.rgb, saturate(heat * 2.0 - 1.0));
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

                float3 N  = normalize(IN.normalWS);
                float3 V  = normalize(IN.viewDirWS);
                float  mu = saturate(dot(N, V)); // 1 at disc centre, 0 at limb

                // Direction on the unit sphere -> noise is independent of mesh scale
                float3 dir = normalize(IN.dirOS);
                float  lat = dir.y;

                // Differential rotation: equator spins faster than the poles
                float ang = _Time.y * _RotationSpeed * (1.0 - _DiffRotation * lat * lat);
                float sn, cs;
                sincos(ang, sn, cs);
                dir.xz = float2(cs * dir.x - sn * dir.z, sn * dir.x + cs * dir.z);

                float t = _Time.y * _FlowSpeed;

                // --- Domain warp: swirling plasma flow ---------------------------------
                float3 q = dir * _WarpScale;
                float3 w = float3(
                    SunFbm(q + float3(0.0, 0.0, t * 0.30), 3),
                    SunFbm(q + float3(5.2, 1.3, 8.1) - t * 0.20, 3),
                    SunFbm(q + float3(9.7, 3.4, 2.2) + t * 0.25, 3));
                float3 pw = dir + w * _WarpStrength;

                // --- Granulation: two cell scales ------------------------------------
                float gt = _Time.y * _GranulationSpeed;
                float2 w1 = SunWorley(pw * _GranulationScale, gt);
                float2 w2 = SunWorley(pw * _GranulationScale * 2.3 + 7.7, gt * 1.3);
                float lane1 = smoothstep(0.0, _LaneWidth, w1.y - w1.x);
                float lane2 = smoothstep(0.0, _LaneWidth * 1.2, w2.y - w2.x);
                float cellGlow = 1.0 - saturate(w1.x * 1.1); // brighter in cell centre
                float gran = lane1 * (0.65 + 0.35 * cellGlow) * (0.75 + 0.25 * lane2);

                // --- Large-scale turbulence / supergranulation -----------------------
                float turb = SunFbm(pw * _TurbScale + float3(0, t * 0.5, 0), 4) * 0.5 + 0.5;

                float heat = gran * 0.62 + turb * 0.38;
                heat = saturate((heat - 0.6) * _Contrast + 0.6);

                // --- Sunspots (active latitude band only) ----------------------------
                float spotNoise = SunFbm(dir * _SpotScale + float3(3.1, 0.0, 7.7), 4) * 0.5 + 0.5;
                float band      = 1.0 - smoothstep(0.35, 0.65, abs(lat));
                float penumbra  = smoothstep(_SpotThreshold,        _SpotThreshold + 0.05, spotNoise) * band;
                float umbra     = smoothstep(_SpotThreshold + 0.05, _SpotThreshold + 0.09, spotNoise) * band;
                float spotDark  = 1.0 - _SpotStrength * (0.45 * penumbra + 0.55 * umbra);
                heat *= lerp(1.0, 0.55, penumbra * _SpotStrength);
                heat *= lerp(1.0, 0.30, umbra * _SpotStrength);

                // --- Colour ----------------------------------------------------------
                float3 col = HeatRamp(heat);
                col *= lerp(0.6, 1.2, heat) * spotDark;

                // Limb darkening (I = 1 - u(1 - mu)) + shift toward cooler red at the edge
                float limb = 1.0 - _LimbDarkening * (1.0 - mu);
                col = lerp(_ColdColor.rgb, col, saturate(pow(mu, _LimbReddening)));
                col *= limb;

                col *= _Intensity;
                return half4(col, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
