Shader "Custom/DiffuseScreenDoor" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		_Alpha("Alpha", Range(0,1)) = 1
	}
	SubShader {
		Tags{ "Queue" = "AlphaTest" "IgnoreProjector" = "True" "RenderType" = "TransparentCutout" }
		LOD 200
		Blend Off
		
		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float4 screenPos;
		};

		half _Glossiness;
		half _Metallic;
		half _Alpha;
		fixed4 _Color;

		// Bug: Fix this shader failing the following test:
		// return !mat.IsKeywordEnabled( "_ALPHABLEND_ON" ) && !mat.IsKeywordEnabled( "_ALPHAPREMULTIPLY_ON" );

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// From: https://www.digitalrune.com/Blog/Post/1743/Screen-Door-Transparency

			const float4x4 thresholdMatrix = {
				1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
				13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
				4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
				16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
			};

			uint x = max(0, IN.screenPos.x / IN.screenPos.w * _ScreenParams.x);
			uint y = max(0, IN.screenPos.y / IN.screenPos.w * _ScreenParams.y);
			clip(_Alpha - thresholdMatrix[x % 4, y % 4]);

			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
