// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/AtmosphericFog" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "black" {}
}

CGINCLUDE

	#include "UnityCG.cginc"
	#include "../../Time of Day/Assets/Shaders/TOD_Base.cginc"
	#include "../../Time of Day/Assets/Shaders/TOD_Scattering.cginc"

	#pragma target 3.0

	uniform sampler2D _MainTex;
	uniform sampler2D _CameraDepthTexture;

	uniform float4 _MainTex_TexelSize;

	// for fast world space reconstruction
	uniform float4x4 _FrustumCornersWS;
	uniform float4 _CameraWS;

	struct v2f {
		float4 pos : POSITION;
		float2 uv : TEXCOORD0;
		float2 uv_depth : TEXCOORD1;
		float4 interpolatedRay : TEXCOORD2;
	};

	v2f vert( appdata_img v )
	{
		v2f o;
		float index = v.vertex.z;
		v.vertex.z = 0.1;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = v.texcoord.xy;
		o.uv_depth = v.texcoord.xy;

		#if UNITY_UV_STARTS_AT_TOP
		if (_MainTex_TexelSize.y < 0)
			o.uv.y = 1-o.uv.y;
		#endif

		o.interpolatedRay = _FrustumCornersWS[(int)index];
		o.interpolatedRay.w = index;

		return o;
	}

	half4 frag (v2f i) : COLOR
	{
		float depth = Linear01Depth(UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, i.uv_depth)));
		depth = pow(depth, 0.33);

		float4 raw = tex2D(_MainTex, i.uv);

		if (depth == 1.0) {
			return raw;
		}

		float3 worldPos = (_CameraWS + depth * i.interpolatedRay);
		float3 ray = worldPos - _CameraWS;
		float3 rayDir = normalize(ray);

		float3 inscatter;
		float3 outscatter;

		ScatteringCoefficients(rayDir, depth, inscatter, outscatter);
		float4 c = ScatteringColor(normalize(rayDir), inscatter, outscatter);

		// Fade out as cam points down, as scatter is not accurate there
		//c *= 1.0 - pow(dot(rayDir, float3(0,1,0)), 2.0) * 0.5;
		//return raw + c;
		return lerp(raw, c, depth);
	}

ENDCG

SubShader {
	Pass {
		ZTest Always Cull Off ZWrite Off
		Fog { Mode off }

		CGPROGRAM

		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest
		#pragma exclude_renderers flash

		ENDCG
	}
}

Fallback off

}
