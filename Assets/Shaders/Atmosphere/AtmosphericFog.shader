Shader "Custom/AtmosphericFog" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "black" {}
}

CGINCLUDE

	#include "UnityCG.cginc"
	#include "AtmosphericScattering.cginc"

	#pragma target 3.0

	uniform sampler2D _MainTex;
	uniform sampler2D_float _CameraDepthTexture;

	uniform float4 _MainTex_TexelSize;

	// For world space reconstruction from depth. Rows are the four frustum corner rays,
	// set by AtmosphericFog.cs each frame.
	uniform float4x4 _FrustumCornersWS;
	uniform float4 _CameraWS;

	struct appdata_fog {
		float4 vertex : POSITION;
		half2 texcoord : TEXCOORD0;
	};

	struct v2f {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
		float2 uv_depth : TEXCOORD1;
		float4 interpolatedRay : TEXCOORD2;
	};

	v2f vert (appdata_fog v) {
		v2f o;
		v.vertex.z = 0.1;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = v.texcoord.xy;
		o.uv_depth = v.texcoord.xy;

		#if UNITY_UV_STARTS_AT_TOP
		if (_MainTex_TexelSize.y < 0)
			o.uv.y = 1 - o.uv.y;
		#endif

		// Same corner-indexing trick as Unity's own GlobalFog - lets this work with a
		// plain Graphics.Blit instead of a hand-rolled immediate-mode quad.
		int frustumIndex = v.texcoord.x + (2 * o.uv.y);
		o.interpolatedRay = _FrustumCornersWS[frustumIndex];
		o.interpolatedRay.w = frustumIndex;

		return o;
	}

	half4 frag (v2f i) : SV_Target {
		half4 sceneColor = tex2D(_MainTex, UnityStereoTransformScreenSpaceTex(i.uv));

		float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, UnityStereoTransformScreenSpaceTex(i.uv_depth));
		float linearDepth = Linear01Depth(rawDepth);

		// Skybox pixels already went through the same integrator in AtmosphericSky.shader,
		// with the ray escaping to infinity. Re-integrating them here would double-apply the
		// atmosphere; skipping is both correct and cheaper.
		if (linearDepth > 0.9999) {
			return sceneColor;
		}

		float3 wsDir = linearDepth * i.interpolatedRay.xyz;
		float3 wsPos = _CameraWS.xyz + wsDir;

		return half4(AtmosApplyFog(sceneColor.rgb, _CameraWS.xyz, wsPos), sceneColor.a);
	}

ENDCG

SubShader {
	ZTest Always Cull Off ZWrite Off

	Pass {
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		ENDCG
	}
}

Fallback off

}
