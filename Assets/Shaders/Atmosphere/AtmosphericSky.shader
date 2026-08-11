// Sky counterpart to AtmosphericFog - same scattering model, view ray integrated until it
// escapes the atmosphere instead of stopping at scene geometry. Assign a material using this
// to Lighting > Environment > Skybox Material, replacing the stock Skybox/Procedural one.
//
// Because the fog uses the identical integrator, distant terrain converges on exactly this
// colour rather than merely approximating it, so there is no horizon seam.

Shader "Skybox/AtmosphericScattering" {
Properties {
	_SunDiskSize ("Sun Disk Size", Range(0.0, 0.2)) = 0.02
	_SunDiskIntensity ("Sun Disk Intensity", Range(0.0, 50.0)) = 12.0
}

SubShader {
	Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
	Cull Off
	ZWrite Off

	Pass {
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma target 3.0

		#include "UnityCG.cginc"
		#include "AtmosphericScattering.cginc"

		float _SunDiskSize;
		float _SunDiskIntensity;

		struct appdata_sky {
			float4 vertex : POSITION;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		struct v2f {
			float4 pos : SV_POSITION;
			float3 dir : TEXCOORD0;
			UNITY_VERTEX_OUTPUT_STEREO
		};

		v2f vert(appdata_sky v) {
			v2f o;
			UNITY_SETUP_INSTANCE_ID(v);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
			o.pos = UnityObjectToClipPos(v.vertex);
			// Skybox geometry is centred on the camera, so the object-space position is the
			// view direction.
			o.dir = v.vertex.xyz;
			return o;
		}

		half4 frag(v2f i) : SV_Target {
			float3 rayDir = normalize(i.dir);

			float3 col = AtmosSkyColor(_WorldSpaceCameraPos, rayDir);

			// Sun disk, drawn on top of the scattering. Faded out below the horizon so it
			// doesn't punch through the ground at night.
			float cosTheta = dot(rayDir, _AtmosSunDir);
			float disk = smoothstep(1.0 - _SunDiskSize, 1.0 - _SunDiskSize * 0.5, cosTheta);
			float aboveHorizon = saturate(_AtmosSunDir.y * 8.0 + 0.1);
			col += _AtmosSunColor.rgb * disk * _SunDiskIntensity * aboveHorizon;

			return half4(col, 1.0);
		}
		ENDCG
	}
}

Fallback Off
}
