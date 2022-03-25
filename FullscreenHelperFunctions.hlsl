#ifndef CYAN_FULLSCREEN_HELPER_FUNCTIONS_INCLUDED
#define CYAN_FULLSCREEN_HELPER_FUNCTIONS_INCLUDED

#ifdef USING_STEREO_MATRICES
// Single Pass Instanced VR
float4x4 _InverseViewStereo[2];
#else
// Non-VR & Multi Pass VR
float4x4 _InverseView;
#endif

void GetInverseViewMatrix_float(out float4x4 Out){
	#ifdef USING_STEREO_MATRICES
		Out = _InverseViewStereo[unity_StereoEyeIndex];
	#else
		Out = _InverseView;
	#endif
}

#if defined(SHADERGRAPH_PREVIEW) && !defined(TEXTURE2D_X)
#define TEXTURE2D_X(textureName)							TEXTURE2D(textureName)
#define SAMPLE_TEXTURE2D_X(textureName, samplerName, uv)	SAMPLE_TEXTURE2D(textureName, samplerName, uv)
#endif

TEXTURE2D_X(_MainTex);
SAMPLER(sampler_MainTex);

void SampleMainTexture_float(float2 uv, out float4 Out){
	Out = SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, uv);
}

#endif