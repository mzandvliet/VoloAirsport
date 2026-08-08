// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "GLLine"
{
	SubShader
	{
		Tags { "Queue" = "Transparent" }
		Pass
		{
			ZTest On				
			Lighting Off						
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma fragment frag

			struct vInput
			{
				float4 vertexPos : POSITION;
				float4 vertexColor : COLOR;
			};

			struct vOutput
			{
				float4 clipPos : SV_POSITION;
				float4 color : COLOR;
			};

			vOutput vert(vInput input)
			{
				vOutput o;
				o.clipPos = UnityObjectToClipPos(input.vertexPos);
				o.color = input.vertexColor;

				return o;
			}

			float4 frag(vOutput input) : COLOR
			{
				return input.color;
			}
			ENDCG
		}
	}
}
