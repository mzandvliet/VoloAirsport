// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/Grass" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_NormalTex ("Normal (RGB)", 2D) = "bump" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_Cutoff ("Cutoff", Range(0,1)) = 0.5
	}
	SubShader {
		Tags { "Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout" }
		LOD 200
		AlphaToMask On

		CGINCLUDE

		float4 tex2Dlod_bilinear(sampler2D tex, half4 uv) {
			const half g_resolution = 128.0;
			const half g_resolutionInv = 1.0/g_resolution;

			half2 pixelFrac = frac(uv.xy * g_resolution);

			half4 baseUV = uv - half4(pixelFrac * g_resolutionInv,0,0);
			half4 heightBL = tex2Dlod(tex, baseUV);
			half4 heightBR = tex2Dlod(tex, baseUV + half4(g_resolutionInv,0,0,0));
			half4 heightTL = tex2Dlod(tex, baseUV + half4(0,g_resolutionInv,0,0));
			half4 heightTR = tex2Dlod(tex, baseUV + half4(g_resolutionInv,g_resolutionInv,0,0));

			half4 tA = lerp(heightBL, heightBR, pixelFrac.x);
			half4 tB = lerp(heightTL, heightTR, pixelFrac.x);

			return lerp(tA, tB, pixelFrac.y);
		}

		void animateVertex(inout float4 v, half2 uv) {
			float3 vWorld = mul(unity_ObjectToWorld, v).rgb;

			v.x += sin(_Time[2] * 2 + vWorld.x * 0.33) * 0.2 * uv.y;
			v.z += sin(_Time[2] * 1 + vWorld.z * 0.33) * 0.2 * uv.y;
			v.x += sin(_Time[3] * 6 + vWorld.x * 1) * 0.1 * uv.y;
			v.z += sin(_Time[3] * 5 + vWorld.z * 1) * 0.1 * uv.y;
		}

		ENDCG

		Pass {
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }
			Fog {Mode Off}
			ZWrite On ZTest Less Cull Off
			ColorMask RGB

			Offset 1, 1

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile_shadowcaster
			#include "UnityCG.cginc"

			sampler2D _MainTex;

			struct v2f {
				V2F_SHADOW_CASTER;
				half2 uv : TEXCOORD1;
			};

			v2f vert(appdata_full v) {
				v2f o;

				animateVertex(v.vertex, v.texcoord);

				TRANSFER_SHADOW_CASTER(o);

				o.uv = v.texcoord;
				return o;
			}

			half4 frag(v2f i) : COLOR {
				fixed4 col = tex2D(_MainTex, i.uv);
				clip(col.a - 0.1);
				SHADOW_CASTER_FRAGMENT(i);
			}

			ENDCG
		}

		ZWrite On ZTest LEqual Cull Off

		CGPROGRAM

		// Todo: Clip shadows based on alpha, otherwise terrain is still in shade even though grass geom is fully clipped away

		/*#pragma surface surf Lambert vertex:vert*/
		#pragma surface surf Standard vertex:vert fullforwardshadows addshadow
		#pragma target 3.0
		//#pragma multi_compile TERRAIN_DEVMODE

		struct Input {
			half2 uv_MainTex;
			half viewDistance;
			half nDotL;
			half vDotN;
		};

		sampler2D _MainTex;
		sampler2D _NormalTex;
		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		half _Cutoff;
		half _GrassDrawRange;
		half3 _SunDir;
		half _SunIntensity;

		void vert(inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);

			animateVertex(v.vertex, v.texcoord);

			float3 worldVertex = mul(unity_ObjectToWorld, v.vertex);
			half3 viewRay = (_WorldSpaceCameraPos - worldVertex).rgb;
			o.viewDistance = length(viewRay);

			half3 viewDir = viewRay / o.viewDistance;
			o.vDotN = dot(viewDir, v.normal);
			o.nDotL = dot(v.normal, _SunDir);
		}

		void surf(Input IN, inout SurfaceOutputStandard o) {
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;

			half proximity = 1 - saturate(IN.viewDistance / _GrassDrawRange);
			proximity = min(1, proximity * 1.5);

			// Clip alpha based on proximity, preserving top pixels longer than low pixels (because of highlights)
			o.Alpha = c.a * proximity * (1 - proximity * pow(1 - IN.uv_MainTex.y, 2));
			clip(o.Alpha - _Cutoff);

			// Add some fake shading based on height
			o.Albedo = c.rgb;			
			o.Albedo *= lerp(1, 0.6 + 0.4 * IN.uv_MainTex.y, proximity);// Todo: put in custom lighting func

			// Add smoothness towards the top of the blades for some cool specular action
			o.Smoothness = (0.2 + 0.8 * pow(proximity, 2) * pow(IN.uv_MainTex.y, 6)) * _Glossiness; // Todo: bake into texture
			o.Metallic = _Metallic;

			half rim = 1 - IN.vDotN;
			half3 fresnel = half3(0.8, 0.8, 0.6) * (1.0 * pow(rim, 3.0));
			half ndotl = 0;// 0.5 + 0.5 * IN.nDotL;
			fresnel *= _SunIntensity / 8.0 * (0.5 + o.Smoothness * 0.5) * ndotl;
			o.Emission = fresnel;
		}

		ENDCG
	}

	Fallback "Legacy Shaders/Transparent/Cutout/VertexLit"
}