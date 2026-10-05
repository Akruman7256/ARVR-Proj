// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Skybox/Farland Skies/Nebula One" {
	
	Properties{

		// Starfield
		[NoScaleOffset]
        _StarfieldCube("Starfield Cubemap", Cube) = "grey" {}
		_BackgroundColor("Background Color", Color) = (0, 0, 0, 1.0)
		_StarsTint("Stars Tint", Color) = (.5, .5, .5, 1.0)
		[Space]
		_StarsBrightnesslMin("Brightness Min", Range(0, 1)) = 0.0
		_StarsBrightnesslMax("Brightness Max", Range(0, 1)) = 1.0

		// Nebula Density
		[NoScaleOffset]
		_DensityCube("Density Cubemap", Cube) = "grey" {}
		_DensityThresholdLow("Density Threshold Low", Range(0, 1)) = 0.4
		_DensityThresholdHigh("Density Threshold High", Range(0, 1)) = 0.8

		// Nebula Diffusion
		[NoScaleOffset]
		_DiffusionCube("Diffusion Cubemap", Cube) = "grey" {}
		_RipplesDistortion("Ripples Distortion", Vector) = (0.2, 0.1, 0, 0)
		
		// Nebula Colors
		_AmbientTint("Ambient Tint", Color) = (0, .63, 1, .03)
		_BasementTint("Basement Tint", Color) = (0, .63, 1, .5)
		_RipplesTint1("Ripples 1 Tint", Color) = (0, .63, 1, .26)
		_RipplesTint2("Ripples 2 Tint", Color) = (.32, .15, .34, .32)

		// General
		[Gamma] _Exposure("Exposure", Range(0, 10)) = 1.0
	}

	CustomEditor "NebulaOneShaderGUI"

	SubShader{
		Tags{ "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" }
		Cull Off ZWrite Off

		Pass{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			// Exposed
			half3 _BackgroundColor;
			
			samplerCUBE _StarfieldCube;
			half4 _StarsTint;
			fixed _StarsBrightnesslMin;
			fixed _StarsBrightnesslMax;

			samplerCUBE _DensityCube;
			fixed _DensityThresholdLow;
			fixed _DensityThresholdHigh;

			samplerCUBE _DiffusionCube;
			fixed3 _RipplesDistortion;

			half4 _AmbientTint;
			half4 _BasementTint;
			half4 _RipplesTint1;
			half4 _RipplesTint2;

			half _Exposure;

			// Scripted
			float4x4 _DensityRotation;
			float4x4 _StarfieldRotation;

			// -----------------------------------------
			// Structs
			// -----------------------------------------

			struct appdata
            {
                float4 vertex   : POSITION;

                // Single pass instanced rendering
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

			struct v2f {
				float4 position : SV_POSITION;
				float3 vertex : TEXCOORD0;
				float3 densityRotation : TEXCOORD1;
				float3 starfieldRotation : TEXCOORD2;

				// Single pass instanced rendering
				UNITY_VERTEX_OUTPUT_STEREO
			};

			// -----------------------------------------
			// Functions
			// -----------------------------------------

			half LinearStep(half a, half b, half x) {
				return saturate((x - a) / (b - a));
			}

			// -----------------------------------------
			// Vertex
			// -----------------------------------------

			v2f vert(appdata v)
			{
				v2f OUT;

				// Single pass instanced rendering
				UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, OUT);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

				// General
				OUT.position = UnityObjectToClipPos(v.vertex);
				OUT.vertex = v.vertex;				
				// Nebula
				OUT.densityRotation = mul(_DensityRotation, v.vertex);
				OUT.starfieldRotation = mul(_StarfieldRotation, v.vertex);

				return OUT;
			}

			// -----------------------------------------
			// Fragment
			// -----------------------------------------

			half4 frag(v2f IN) : SV_Target
			{				
				// Stars
				half3 starsTex = texCUBE(_StarfieldCube, IN.starfieldRotation);
				half starsCoef = LinearStep(_StarsBrightnesslMin, _StarsBrightnesslMax, starsTex.r) * _StarsTint.a;
				half3 starsColor = _StarsTint.rgb * unity_ColorSpaceDouble.rgb;
				half3 color = lerp(_BackgroundColor, starsColor, starsCoef);

				// Nebula

				half4 diffusion = texCUBE(_DiffusionCube, IN.vertex);
				half density = texCUBE(_DensityCube, IN.densityRotation).r;

				// Nebula Ambient
				half4 nColor = diffusion.g * (unity_ColorSpaceDouble * _AmbientTint);
				nColor.a *= smoothstep(_DensityThresholdHigh, _DensityThresholdLow, density);
				color = lerp(color, nColor.rgb, nColor.a);
			
				// Nebula Basement
				nColor = 0.5 * (diffusion.g + diffusion.b) * (unity_ColorSpaceDouble * _BasementTint);				
				nColor.a *= smoothstep(_DensityThresholdLow, _DensityThresholdHigh, density);
				color = lerp(color, nColor.rgb, nColor.a);

				// Nebula Ripples 1
				half3 offset = _RipplesDistortion.xyz * diffusion.b * half3(.75, -.75, .75);
				nColor = texCUBE(_DiffusionCube, IN.vertex + offset).r * (unity_ColorSpaceDouble * _RipplesTint1);
				nColor.a *= smoothstep(_DensityThresholdLow, _DensityThresholdHigh, texCUBE(_DensityCube, IN.densityRotation + offset).r);
				color = lerp(color, nColor.rgb, nColor.a);

				// Nebula Ripples 2
				offset = _RipplesDistortion.zyx * diffusion.g;
				nColor = texCUBE(_DiffusionCube, IN.vertex + offset).r * (unity_ColorSpaceDouble * _RipplesTint2);
				nColor.a *= smoothstep(_DensityThresholdLow, _DensityThresholdHigh, texCUBE(_DensityCube, IN.densityRotation + offset).r);
				color = lerp(color, nColor.rgb, nColor.a);
				
				// General
				color *= _Exposure;

				return half4(color, 1);
			}
			ENDCG
		}
	}
}