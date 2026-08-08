// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Nature/Terrain/StandardCustom" {
	Properties{
		// set by terrain engine
		_Control("Control (RGBA)", 2D) = "red" {}
		_Splat3("Layer 3 (A)", 2D) = "white" {}
		_Splat2("Layer 2 (B)", 2D) = "white" {}
		_Splat1("Layer 1 (G)", 2D) = "white" {}
		_Splat0("Layer 0 (R)", 2D) = "white" {}
		_Normal3("Normal 3 (A)", 2D) = "bump" {}
		_Normal2("Normal 2 (B)", 2D) = "bump" {}
		_Normal1("Normal 1 (G)", 2D) = "bump" {}
		_Normal0("Normal 0 (R)", 2D) = "bump" {}
		_Splat0SMask("Smoothness Mask", 2D) = "white" {}

		_GlobalOcclusionTex("Global Occlusion", 2D) = "white" {}
		_GlobalNormalTex("Global Normal", 2D) = "bump" {}

		_HeightGain("Height Gain", Vector) = (1,1,1,1)
		_HeightPow("Height Pow", Vector) = (1,1,1,1)

		// used in fallback on old cards & base map
		_MainTex("BaseMap (RGB)", 2D) = "white" {}
		_Color("Main Color", Color) = (1,1,1,1)

		_UVScale("UV Scale", Vector) = (1,0.1,0.001,0)
		_GlobalTexUVCoords("GlobalTex UVCoords", Vector) = (0,0,1000,1000)

		_DetailDistance("Detail Distance", Vector) = (125.0,250.0,0.0,0.0)

		_GlobalOcclusionIntensity("Occlusion Intensity", Range(0.1, 2.0)) = 1.0
		_GlobalOcclusionPow("Occlusion Pow", Range(0.5, 2.0)) = 0.5

		_FresnelGain("Fresnel Gain", Vector) = (0,0,0,0)
		_FresnelPower("Fresnel Gain", Vector) = (2,2,2,2)

		_FresnelColor0("Fresnel Color 0", Color) = (1,1,1,1)
		_FresnelColor1("Fresnel Color 1", Color) = (1,1,1,1)
		_FresnelColor2("Fresnel Color 2", Color) = (1,1,1,1)
		_FresnelColor3("Fresnel Color 3", Color) = (1,1,1,1)
	}

	SubShader {
		Tags {
			//"SplatCount" = "4"
			"Queue" = "Geometry-100"
			"RenderType" = "Opaque"
		}
		LOD 200

		/* ================== Forward Pass ================== */

		Pass{
			Name "FORWARD"
			Tags{ "LightMode" = "ForwardBase" }

			CGPROGRAM
			// compile directives
			#pragma vertex vert_surf
			#pragma fragment frag_surf
			#pragma multi_compile_fog
			#pragma target 3.0
			#pragma exclude_renderers gles
			#pragma multi_compile_fwdbase
			#include "HLSLSupport.cginc"
			#include "UnityShaderVariables.cginc"
			#define UNITY_PASS_FORWARDBASE
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"

			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

			#line 31 ""
			#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
			#endif

			#pragma exclude_renderers gles
			#include "UnityPBSLighting.cginc"
			#include "TerrainSplatmapCommonCustom.cginc"

			// vertex shader
			v2f_surf vert_surf(appdata_full v) {
				//UNITY_SETUP_INSTANCE_ID(v);
				v2f_surf o;
				UNITY_INITIALIZE_OUTPUT(v2f_surf, o);
				//UNITY_TRANSFER_INSTANCE_ID(v, o);

				//o.pos = UnityObjectToClipPos(v.vertex);
				o.pos = UnityObjectToClipPos(v.vertex);

				float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
				fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
				fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
				o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
				o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
				o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
				
				o.uv.xy = TRANSFORM_TEX(v.texcoord, _Control);
				o.uv.zw = float2(
					(worldPos.x - _GlobalTexUVCoords.x) / _GlobalTexUVCoords.z,
					(worldPos.z - _GlobalTexUVCoords.y) / _GlobalTexUVCoords.w
					);
				o.camDist = length(worldPos - _WorldSpaceCameraPos.xyz);
				

#ifndef DYNAMICLIGHTMAP_OFF
				o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#endif
#ifndef LIGHTMAP_OFF
				o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
#endif

				// SH/ambient and vertex lights
#ifdef LIGHTMAP_OFF
#if UNITY_SHOULD_SAMPLE_SH
				o.sh = 0;
				// Approximated illumination from non-important point lights
#ifdef VERTEXLIGHT_ON
				o.sh += Shade4PointLights(
					unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
					unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
					unity_4LightAtten0, worldPos, worldNormal);
#endif
				o.sh = ShadeSHPerVertex(worldNormal, o.sh);
#endif
#endif // LIGHTMAP_OFF

				TRANSFER_SHADOW(o); // pass shadow coordinates to pixel shader
				return o;
		}

		// fragment shader
			fixed4 frag_surf(v2f_surf IN) : SV_Target{
				//UNITY_SETUP_INSTANCE_ID(IN);

				float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
				float3 worldNormal = float3(IN.tSpace0.y, IN.tSpace1.y, IN.tSpace2.y);

#ifndef USING_DIRECTIONAL_LIGHT
				fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
#else
				fixed3 lightDir = _WorldSpaceLightPos0.xyz;
#endif
				fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
#ifdef UNITY_COMPILER_HLSL
				SurfaceOutputStandard o = (SurfaceOutputStandard)0;
#else
				SurfaceOutputStandard o;
#endif
				o.Albedo = 0.0;
				o.Emission = 0.0;
				o.Alpha = 0.0;
				o.Occlusion = 1.0;
				fixed3 normalWorldVertex = fixed3(0,0,1);

				// compute lighting & shadowing factor
				UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
				fixed4 c = 0;
				fixed3 worldN;
				worldN.x = dot(IN.tSpace0.xyz, o.Normal);
				worldN.y = dot(IN.tSpace1.xyz, o.Normal);
				worldN.z = dot(IN.tSpace2.xyz, o.Normal);
				o.Normal = worldN;

				// Read global normal map from atlas and use it instead of mesh normal for tangent-to-world conversion

				float3 globalNormal = UnpackNormal(tex2D(_GlobalNormalTex, IN.uv.zw));
				globalNormal = globalNormal.xzy; // Convert from WorldMachine space
				globalNormal = lerp(float3(o.Normal), globalNormal, 1.0);
				globalNormal = normalize(globalNormal);

				// Re-orthogonalize binormal and tangent based on new global normal

				float3 globalBinormal = normalize(float3(IN.tSpace0.y, IN.tSpace1.y, IN.tSpace2.y));
				float3 globalTangent = normalize(cross(globalNormal, globalBinormal));
				globalBinormal = normalize(cross(globalNormal, globalTangent));

				IN.tSpace0.x = globalTangent.x;
				IN.tSpace1.x = globalTangent.y;
				IN.tSpace2.x = globalTangent.z;
				IN.tSpace0.y = globalBinormal.x;
				IN.tSpace1.y = globalBinormal.y;
				IN.tSpace2.y = globalBinormal.z;
				IN.tSpace0.z = globalNormal.x;
				IN.tSpace1.z = globalNormal.y;
				IN.tSpace2.z = globalNormal.z;

				// call surface function
				SplatmapMix(IN, o);

				// Setup lighting environment
				UnityGI gi;
				UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
				gi.indirect.diffuse = 0;
				gi.indirect.specular = 0;
#if !defined(LIGHTMAP_ON)
				gi.light.color = _LightColor0.rgb;
				gi.light.dir = lightDir;
				gi.light.ndotl = LambertTerm(o.Normal, gi.light.dir);
#endif
				// Call GI (lightmaps/SH/reflections) lighting function
				UnityGIInput giInput;
				UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
				giInput.light = gi.light;
				giInput.worldPos = worldPos;
				giInput.worldViewDir = worldViewDir;
				giInput.atten = atten;
#if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
				giInput.lightmapUV = IN.lmap;
#else
				giInput.lightmapUV = 0.0;
#endif
#if UNITY_SHOULD_SAMPLE_SH
				giInput.ambient = IN.sh;
#else
				giInput.ambient.rgb = 0.0;
#endif
				giInput.probeHDR[0] = unity_SpecCube0_HDR;
				giInput.probeHDR[1] = unity_SpecCube1_HDR;
#if UNITY_SPECCUBE_BLENDING || UNITY_SPECCUBE_BOX_PROJECTION
				giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
#endif
#if UNITY_SPECCUBE_BOX_PROJECTION
				giInput.boxMax[0] = unity_SpecCube0_BoxMax;
				giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
				giInput.boxMax[1] = unity_SpecCube1_BoxMax;
				giInput.boxMin[1] = unity_SpecCube1_BoxMin;
				giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
#endif
				LightingStandard_GI(o, giInput, gi);

				// realtime lighting: call lighting function
				c += LightingStandard(o, worldViewDir, gi);
				//SplatmapFinalColor(surfIN, o, c);
				UNITY_OPAQUE_ALPHA(c.a);
				return c;
			}

			ENDCG
		}
	    

		/* ================== Deferred Pass ================== */

		Pass {
			Name "DEFERRED"
			Tags { "LightMode" = "Deferred" }

				CGPROGRAM
			// compile directives
			#pragma target 3.0
			#pragma vertex vert_surf
			#pragma fragment frag_surf
			#pragma exclude_renderers gles
			#pragma exclude_renderers nomrt
			#pragma multi_compile_prepassfinal
			#pragma multi_compile TERRAIN_DEVMODE

			#include "HLSLSupport.cginc"
			#include "UnityShaderVariables.cginc"

			#define UNITY_PASS_DEFERRED
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#include "Lighting.cginc"

			// Original surface shader snippet:
			#line 15 ""
			#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
			#endif

			//#pragma surface surf Standard fullforwardshadows
			#pragma target 3.0
			// needs more than 8 texcoords
			#pragma exclude_renderers gles
			#include "UnityPBSLighting.cginc"
			#include "TerrainSplatmapCommonCustom.cginc"

			// vertex shader
			v2f_surf vert_surf(appdata_full v) {
				v2f_surf o;
				UNITY_INITIALIZE_OUTPUT(v2f_surf,o);

				v.tangent.xyz = cross(v.normal, float3(0,0,1));
				v.tangent.w = -1;

				o.pos = UnityObjectToClipPos(v.vertex);
				
				float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				float3 worldNormal = UnityObjectToWorldNormal(v.normal);
				float3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
				fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
				o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
				o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
				o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);

				o.uv.xy = TRANSFORM_TEX(v.texcoord, _Control);
				o.uv.zw = float2(
					(worldPos.x - _GlobalTexUVCoords.x) / _GlobalTexUVCoords.z,
					(worldPos.z - _GlobalTexUVCoords.y) / _GlobalTexUVCoords.w
				);
				o.camDist = length(worldPos - _WorldSpaceCameraPos.xyz);

				float3 viewDirForLight = UnityWorldSpaceViewDir(worldPos);
				#ifndef DIRLIGHTMAP_OFF
				o.viewDir.x = dot(viewDirForLight, worldTangent);
				o.viewDir.y = dot(viewDirForLight, worldBinormal);
				o.viewDir.z = dot(viewDirForLight, worldNormal);
				#endif
				#ifndef DYNAMICLIGHTMAP_OFF
					o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
				#else
					o.lmap.zw = 0;
				#endif
				#ifndef LIGHTMAP_OFF
					o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
					#ifdef DIRLIGHTMAP_OFF
					o.lmapFadePos.xyz = (mul(unity_ObjectToWorld, v.vertex).xyz - unity_ShadowFadeCenterAndType.xyz) * unity_ShadowFadeCenterAndType.w;
					o.lmapFadePos.w = (-mul(UNITY_MATRIX_MV, v.vertex).z) * (1.0 - unity_ShadowFadeCenterAndType.w);
					#endif
				#else
					o.lmap.xy = 0;
					#if UNITY_SHOULD_SAMPLE_SH
					#if UNITY_SAMPLE_FULL_SH_PER_PIXEL
						o.sh = 0;
					#elif (SHADER_TARGET < 30)
						o.sh = ShadeSH9(float4(worldNormal,1.0));
					#else
						o.sh = ShadeSH3Order(half4(worldNormal, 1.0));
					#endif
					#endif
				#endif

				return o;
			}

			#ifdef LIGHTMAP_ON
			float4 unity_LightmapFade;
			#endif
			fixed4 unity_Ambient;

			// fragment shader
			void frag_surf(v2f_surf IN,
			out half4 outDiffuse : SV_Target0,
			out half4 outSpecSmoothness : SV_Target1,
			out half4 outNormal : SV_Target2,
			out half4 outEmission : SV_Target3) {

			float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
			float3 worldNormal = float3(IN.tSpace0.y, IN.tSpace1.y, IN.tSpace2.y);

			#ifndef USING_DIRECTIONAL_LIGHT
			fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
			#else
			fixed3 lightDir = _WorldSpaceLightPos0.xyz;
			#endif
			fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
			#ifdef UNITY_COMPILER_HLSL
			SurfaceOutputStandard o = (SurfaceOutputStandard)0;
			#else
			SurfaceOutputStandard o;
			#endif
			o.Albedo = 0.0;
			o.Emission = 0.0;
			o.Alpha = 0.0;
			o.Occlusion = 1.0;
			o.Normal = worldNormal;

			// Read global normal map from atlas and use it instead of mesh normal for tangent-to-world conversion

			float3 globalNormal = UnpackNormal(tex2D(_GlobalNormalTex, IN.uv.zw));
			globalNormal = globalNormal.xzy; // Convert from WorldMachine space
			globalNormal = lerp(float3(o.Normal), globalNormal, 1.0);
			globalNormal = normalize(globalNormal);

			// Re-orthogonalize binormal and tangent based on new global normal

			float3 globalBinormal = normalize(float3(IN.tSpace0.y, IN.tSpace1.y, IN.tSpace2.y));
			float3 globalTangent = normalize(cross(globalNormal, globalBinormal));
			globalBinormal = normalize(cross(globalNormal, globalTangent));

			IN.tSpace0.x = globalTangent.x;
			IN.tSpace1.x = globalTangent.y;
			IN.tSpace2.x = globalTangent.z;
			IN.tSpace0.y = globalBinormal.x;
			IN.tSpace1.y = globalBinormal.y;
			IN.tSpace2.y = globalBinormal.z;
			IN.tSpace0.z = globalNormal.x;
			IN.tSpace1.z = globalNormal.y;
			IN.tSpace2.z = globalNormal.z;

			// call surface function
			SplatmapMix(IN, o);

			half atten = 1;

			// Setup lighting environment
			UnityGI gi;
			UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
			gi.indirect.diffuse = 0;
			gi.indirect.specular = 0;
			gi.light.color = 0;
			gi.light.dir = half3(0,1,0);
			gi.light.ndotl = LambertTerm(o.Normal, gi.light.dir);
			// Call GI (lightmaps/SH/reflections) lighting function
			UnityGIInput giInput;
			UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
			giInput.light = gi.light;
			giInput.worldPos = worldPos;
			giInput.worldViewDir = worldViewDir;
			giInput.atten = atten;
			#if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
				giInput.lightmapUV = IN.lmap;
			#else
				giInput.lightmapUV = 0.0;
			#endif
			#if UNITY_SHOULD_SAMPLE_SH
				giInput.ambient = IN.sh;
			#else
				giInput.ambient.rgb = 0.0;
			#endif
			giInput.probeHDR[0] = unity_SpecCube0_HDR;
			giInput.probeHDR[1] = unity_SpecCube1_HDR;
			#if UNITY_SPECCUBE_BLENDING || UNITY_SPECCUBE_BOX_PROJECTION
				giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
			#endif
			#if UNITY_SPECCUBE_BOX_PROJECTION
				giInput.boxMax[0] = unity_SpecCube0_BoxMax;
				giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
				giInput.boxMax[1] = unity_SpecCube1_BoxMax;
				giInput.boxMin[1] = unity_SpecCube1_BoxMin;
				giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
			#endif
			LightingStandard_GI(o, giInput, gi);

			// call lighting function to output g-buffer
			outEmission = LightingStandard_Deferred(o, worldViewDir, gi, outDiffuse, outSpecSmoothness, outNormal);
			#ifndef UNITY_HDR_ON
			outEmission.rgb = exp2(-outEmission.rgb);
			#endif
			//SplatmapFinalGBuffer (o, outDiffuse, outSpecSmoothness, outNormal, outEmission);
			UNITY_OPAQUE_ALPHA(outDiffuse.a);
			}

			ENDCG

		}

		/* ================== End Of Passes ================== */

	}

	Dependency "AddPassShader" = "TerrainEngine/Splatmap/Standard-AddPass-Custom"
	Dependency "BaseMapShader" = "TerrainEngine/Splatmap/Standard-Base-Custom"

	Fallback "Nature/Terrain/Diffuse"
}
