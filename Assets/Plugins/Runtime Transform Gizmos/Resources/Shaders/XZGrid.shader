// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "XZGrid"
{
	Properties
	{
		_Color ("Color", Color) = (1, 1, 1, 1)
		_CamFarPlane ("Cam Far Plane", float) = 1000
		_CamLook ("Cam Look", Vector) = (0, 0, 1, 0)
		_FadeScale ("Fade Scale", float) = 1
	}

	Subshader
	{
		Tags {"Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True"}
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			Cull Off

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma fragment frag

			float4 _Color;
			float _CamFarPlane;
			float4 _CamLook;
			float _FadeScale;

			struct vInput
			{
				float4 vertexPos : POSITION;
			};

			struct vOutput
			{
				float4 clipPos : SV_POSITION;
				float3 viewPos : TEXCOORD0;
			};

			vOutput vert(vInput input)
			{
				vOutput o;
				o.clipPos = UnityObjectToClipPos(input.vertexPos);
				o.viewPos = mul(UNITY_MATRIX_MV, input.vertexPos);

				return o;
			}

			float4 frag(vOutput o) : COLOR
			{
				float distFromFarPlane = abs(o.viewPos.z);
				float farPlaneAlphaScale = saturate(1.0f - distFromFarPlane / (_CamFarPlane * max(1.0f, _FadeScale) * 0.1f * (1000.0f / _CamFarPlane)));
				return float4(_Color.rgb, _Color.a * farPlaneAlphaScale);
			}
			ENDCG
		}
	}
}