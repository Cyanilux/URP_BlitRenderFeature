Shader "Hidden/Cyan/BlitInvert" {
	SubShader {
		Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
		LOD 100
		Cull Off
		ZWrite Off
		ZTest NotEqual // ZTest Always
		Pass {
			Name "BlitPass"

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			struct Attributes {
				float4 positionOS   : POSITION;
				float2 uv           : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct Varyings {
				float4  positionCS  : SV_POSITION;
				float2  uv          : TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			Varyings vert(Attributes input) {
				Varyings output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				//output.positionCS = TransformObjectToHClip(input.positionOS.xyz);

				/*
				- The feature is setup with a mesh already in clip space so we output directly
				- (Though it can also optionally override the view/projection matrices so other 
				shaders/materials that don't do this (e.g. shader graphs) can still be used)
				*/
				// output.positionCS = float4(input.positionOS.xyz, 1.0);
				output.positionCS = float4(input.positionOS.xy, UNITY_NEAR_CLIP_VALUE, 1.0);
				// Note : Have switched to using UNITY_NEAR_CLIP_VALUE and ZTest NotEqual to stop blit occurring on occlusion mesh for VR
				#if UNITY_UV_STARTS_AT_TOP
					output.positionCS.y *= -1;
				#endif

				output.uv = input.uv;
				return output;
			}

			TEXTURE2D_X(_MainTex);
			SAMPLER(sampler_MainTex);

			half4 frag (Varyings input) : SV_Target {
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
				return 1 - SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, input.uv);
			}
			ENDHLSL
		}
	}
}