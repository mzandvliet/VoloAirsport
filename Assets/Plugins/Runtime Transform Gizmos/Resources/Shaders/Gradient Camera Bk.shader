// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Gradient Camera Bk"
{
	Properties
	{
		_TopColor ("Top Color", Color) = (1, 1, 1, 1)
		_BottomColor ("Bottom Color", Color) = (1, 1, 1, 1)
		_Height ("Height", float) = 1
		_GradientOffset ("Gradient Offset", float) = 0
	}

	Subshader
	{
		Tags {"Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True"}
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			ZTest On

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			float4 _TopColor;
			float4 _BottomColor;
			float _Height;
			float _GradientOffset;

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
				float pixelYPos = _Height * 0.5f - o.viewPos.y;
				float weight = saturate(pixelYPos / _Height + _GradientOffset);
				float4 pixelColor = lerp(_TopColor, _BottomColor, weight);
				return float4(pixelColor.rgb, 1.0f);
			}
			ENDCG
		}
	}
}