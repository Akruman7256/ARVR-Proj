// Billboard corona for the Sun (URP, Unity 6). Put on a Quad centred on the Sun.
// Always faces the camera. Additive glow with animated streamers.
//
// Sizing: _InnerRadius = SunDiameter / QuadWidth.
//   e.g. Sun scale 10 and Quad scale 40  ->  _InnerRadius = 0.25
Shader "Custom/Sun/Corona"
{
    Properties
    {
        [HDR] _InnerColor ("Inner Colour (near surface)", Color) = (1.00, 0.80, 0.40, 1)
        [HDR] _OuterColor ("Outer Colour (far glow)",     Color) = (1.00, 0.45, 0.12, 1)
        _Intensity        ("Intensity", Range(0, 6)) = 1.5

        _InnerRadius  ("Sun Radius / Quad Half-Size", Range(0.05, 0.9)) = 0.25
        _InnerFalloff ("Inner Falloff", Range(1, 30)) = 9
        _OuterFalloff ("Outer Falloff", Range(0.5, 12)) = 3
        _InnerStrength("Inner Strength", Range(0, 3)) = 1.2
        _OuterStrength("Outer Strength", Range(0, 3)) = 0.7

        [Header(Streamers)]
        _StreamerAmount    ("Streamer Amount",    Range(0, 1))  = 0.75
        _StreamerFreq      ("Streamer Frequency", Range(0.5, 8)) = 2.5
        _StreamerSharpness ("Streamer Sharpness", Range(0.5, 8)) = 2.5
        _StreamerSpeed     ("Streamer Speed",     Range(0, 1))  = 0.08
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" }

        Pass
        {
            Name "Corona"
            Tags { "LightMode"="UniversalForward" }
            Blend One One
            ZWrite Off
            ZTest LEqual
            Cull Off

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "SunNoise.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _InnerColor;
                float4 _OuterColor;
                float  _Intensity;
                float  _InnerRadius;
                float  _InnerFalloff;
                float  _OuterFalloff;
                float  _InnerStrength;
                float  _OuterStrength;
                float  _StreamerAmount;
                float  _StreamerFreq;
                float  _StreamerSharpness;
                float  _StreamerSpeed;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0; // -1..1 across the quad
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                float4x4 M = GetObjectToWorldMatrix();
                float3 centerWS = float3(M._m03, M._m13, M._m23);
                float2 scale = float2(length(float3(M._m00, M._m10, M._m20)),
                                      length(float3(M._m01, M._m11, M._m21)));

                // Camera-facing billboard
                float3 right = UNITY_MATRIX_V[0].xyz;
                float3 up    = UNITY_MATRIX_V[1].xyz;
                float3 posWS = centerWS + right * (v.positionOS.x * scale.x)
                                        + up    * (v.positionOS.y * scale.y);

                o.positionCS = TransformWorldToHClip(posWS);
                o.uv = v.positionOS.xy * 2.0; // Unity Quad spans -0.5..0.5
                return o;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

                float2 uv = IN.uv;
                float  r  = length(uv);

                // 0 at the Sun's edge, 1 at the quad edge
                float d = saturate((r - _InnerRadius) / max(1.0 - _InnerRadius, 1e-4));

                // Angular streamers (seamless: sample noise on a circle)
                float  a  = atan2(uv.y, uv.x);
                float3 np = float3(cos(a), sin(a), 0.0) * _StreamerFreq;
                np.z = _Time.y * _StreamerSpeed - d * 1.5; // streamers drift outward
                np.xy *= 1.0 + d * 0.6;
                float sn = SunFbm(np, 3) * 0.5 + 0.5;
                float streamers = pow(saturate(sn * 1.4), _StreamerSharpness);

                float inner = exp(-d * _InnerFalloff) * _InnerStrength;
                float outer = exp(-d * _OuterFalloff) * _OuterStrength;
                outer *= lerp(1.0, streamers * 2.2, _StreamerAmount);

                float3 col = lerp(_InnerColor.rgb, _OuterColor.rgb, saturate(d * 2.0)) * (inner + outer);

                // Fade to zero before the quad edge so no square is ever visible
                float mask = 1.0 - smoothstep(0.7, 1.0, r);
                col *= mask * _Intensity;

                return half4(col, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
