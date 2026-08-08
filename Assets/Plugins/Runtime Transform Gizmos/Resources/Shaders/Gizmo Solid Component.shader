// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Gizmo Solid Component"
{
	Properties
	{
		_Color ("Color", Color) = (1, 1, 1, 1)
		_IsLit ("Is Lit", int) = 1
		_LightDir ("Light Dir", Vector) = (1, 1, 1, 0)
		_CullMode ("Cull Mode", int) = 2			
		_StencilRefValue ("Stencil Ref Value", int) = 1
	}

	Subshader
	{
		Tags { "Queue" = "Transparent" }
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			ZTest Off
			Cull [_CullMode]

			Stencil
			{
				Ref [_StencilRefValue]		
				Comp Always					
				Pass Replace					
			}

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma fragment frag
			#pragma vertex vert

			float4 _Color;
			int _IsLit;
			float4 _LightDir;

			struct vInput
			{
				float4 vertexPos : POSITION;
				float3 vertexNormal : NORMAL;
			};

			struct vOutput
			{
				float4 clipPos : SV_POSITION;
				float3 worldNormal : TEXCOORD1;
			};

			vOutput vert(vInput input)
			{
				vOutput o;
				o.clipPos = UnityObjectToClipPos(input.vertexPos);
				o.worldNormal = mul(input.vertexNormal, unity_WorldToObject);

				return o;
			}

			float4 frag(vOutput o) : COLOR
			{
				if(_IsLit == 0) return _Color;
				else
				{
					o.worldNormal = normalize(o.worldNormal);

					float lightInensity = 1.5f;

					float minInfluence = 0.35f;
					float lightInfluence = saturate(dot(-_LightDir.xyz, o.worldNormal));
					lightInfluence = saturate(lightInfluence + minInfluence * (1.0f - lightInfluence));

					return _Color * float4(lightInfluence * lightInensity, lightInfluence * lightInensity, lightInfluence * lightInensity, 1.0f);
				}
			}
			ENDCG
		}
	}
}