// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Gizmo Line"
{
	Properties
	{
		_StencilRefValue ("Stencil Ref Value", int) = 1			// We will use the stencil buffer to mark areas on the screen where the line
																// can not be drawn. This value can be set from the client code to allow for
																// more flexibility.
	}

	SubShader
	{
		Tags { "Queue" = "Transparent" }
		Pass
		{
			// Setup the stencil operations accordingly
			Stencil
			{
				Ref [_StencilRefValue]							// Specify the reference value
				Comp NotEqual									// The line pixel is only accepted if the reference value is not equal the one which
			}

			Cull Off
			Lighting Off						
			ZTest Off							
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
