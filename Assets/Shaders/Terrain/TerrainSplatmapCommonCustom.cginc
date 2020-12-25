#ifndef TERRAIN_SPLATMAP_COMMON_CUSTOM_CGINC_INCLUDED
#define TERRAIN_SPLATMAP_COMMON_CUSTOM_CGINC_INCLUDED

sampler2D _Control;
float4 _Control_ST;
sampler2D _Splat0, _Splat1, _Splat2, _Splat3;
sampler2D _GlobalNormalTex;
sampler2D _GlobalOcclusionTex;

sampler2D _Normal0, _Normal1, _Normal2, _Normal3;

float4 _GlobalTexUVCoords;
float4 _UVScale;

half2 _DetailDistance;

float4 _HeightGain;
float4 _HeightPow;
float2 _SnowAltitude;

half _GlobalOcclusionIntensity;
half _GlobalOcculusionPow;

half4 _FresnelGain;
half4 _FresnelPower;
half4 _FresnelColor0;
half4 _FresnelColor1;
half4 _FresnelColor2;
half4 _FresnelColor3;

float3 _SunDir;
half _SunIntensity;
half _Fogginess;

struct v2f_surf {
	float4 pos : SV_POSITION;
	float4 tSpace0 : TEXCOORD0;
	float4 tSpace1 : TEXCOORD1;
	float4 tSpace2 : TEXCOORD2;
	half4 uv : TEXCOORD3; // xy = tileUV, zw = globalUV
	half camDist : TEXCOORD4;

#ifndef DIRLIGHTMAP_OFF
	half3 viewDir : TEXCOORD5;
#endif
	float4 lmap : TEXCOORD6;
#ifdef LIGHTMAP_OFF
#if UNITY_SHOULD_SAMPLE_SH
	half3 sh : TEXCOORD7; // SH
#endif
#else
#ifdef DIRLIGHTMAP_OFF
	float4 lmapFadePos : TEXCOORD8;
#endif
#endif
	SHADOW_COORDS(9) // Todo: not needed in deferred pass
	//UNITY_INSTANCE_ID // Todo: set this up for deferred as well?
};

half sigmoid(half x) {
	if (x >= 1.0) return 1.0;
	else if (x <= 0.0) return 0.0;

	x = x * 2 - 1;
	return 0.5 + x * (1.0 - abs(x) * 0.5);
}

half3 ColorToNormal(half3 c) {
	return half3(
		-1 + c.r * 2,
		-1 + c.g * 2,
		c.b);
}

half3 NormalToColor(half3 c) {
	return half3(
		0.5 + c.r * 0.5,
		0.5 + c.g * 0.5,
		c.b);
}

struct TpBasis {
	half3 blend;
	float2 coord0;
	float2 coord1;
	float2 coord2;
	half3x3 tbn0;
	half3x3 tbn1;
};

TpBasis GetTpBasis(float3 worldPos, float3 worldNormal) {
	// From: https://www.gamedev.net/topic/621962-using-normal-mapping-with-triplanar-texture-projection/

	TpBasis tpb;

	tpb.blend = abs(worldNormal);
	tpb.blend.y = pow(tpb.blend.y, 4);
	tpb.blend /= (tpb.blend.x + tpb.blend.y + tpb.blend.z);

	// Triplanar sample coords
	tpb.coord0 = float2(worldPos.z, -worldPos.y); // ZY: Left and Right
	tpb.coord1 = float2(worldPos.x, -worldPos.z); // XZ: Top and Bottom (magic x2 to avoid stretch)
	tpb.coord2 = float2(worldPos.x, -worldPos.y); // XY: Front and Back
	half3 flip = sign(worldNormal);
	tpb.coord0.x *= flip.x;
	tpb.coord1.x *= flip.y;
	tpb.coord2.x *= -flip.z;

	half3 tangent0 = normalize(half3(-worldNormal.z, 0, worldNormal.x));
	half3 tangent1 = normalize(half3(worldNormal.y, -worldNormal.x, 0));

	half3 binormal0 = cross(tangent0, worldNormal);
	half3 binormal1 = cross(tangent1, worldNormal);

	// Build TBN matrices for both tangent spaces
	tpb.tbn0 = half3x3(
		tangent0.x, tangent0.y, tangent0.z,
		binormal0.x, binormal0.y, binormal0.z,
		worldNormal.x, worldNormal.y, worldNormal.z
		);

	tpb.tbn1 = half3x3(
		tangent1.x, tangent1.y, tangent1.z,
		binormal1.x, binormal1.y, binormal1.z,
		worldNormal.x, worldNormal.y, worldNormal.z
		);

	return tpb;
}

half4 Tex2DTriplanar(sampler2D tex, TpBasis b, half uvScale) {
	half4 c0 = tex2D(tex, b.coord0 * uvScale);
	half4 c1 = tex2D(tex, b.coord1 * uvScale);
	half4 c2 = tex2D(tex, b.coord2 * uvScale);
	return
		c0 * b.blend.x +
		c1 * b.blend.y +
		c2 * b.blend.z;
}

half4 Tex2DNormal(sampler2D tex, TpBasis b, half2 uv) {
	half4 c = tex2D(tex, uv);
	half3 n = c.rgb * 2.0 - 1.0;
	n = mul(n, b.tbn1);
	return half4(n, c.a);
}

half4 Tex2DTriplanarNormal(sampler2D tex, TpBasis b, half uvScale) {
	half4 c0 = tex2D(tex, b.coord0 * uvScale);
	half4 c1 = tex2D(tex, b.coord1 * uvScale);
	half4 c2 = tex2D(tex, b.coord2 * uvScale);

	half3 normal0 = c0.rgb * 2.0 - 1.0;
	half3 normal1 = c1.rgb * 2.0 - 1.0;
	half3 normal2 = c2.rgb * 2.0 - 1.0;

	// Transform normals into world space using the two tangent bases
	normal0 = mul(normal0, b.tbn0);
	normal1 = mul(normal1, b.tbn1);
	normal2 = mul(normal2, b.tbn0);

	// Blend together
	half3 normal =
		normal0 * b.blend.x +
		normal1 * b.blend.y +
		normal2 * b.blend.z;

	return half4(
		normalize(normal),
		c0.a * b.blend.x + c1.a * b.blend.y + c2.a * b.blend.z
		);
}

half ToGrayscale(half4 c) {
	return c.r * 0.3086 + c.g * 0.6094 + c.b * 0.0820;
}

float BlendAdditive(half a, half b, half blend) {
	blend = blend * blend;
	return (a + b * blend) / (1.0 + blend);
}

half4 BlendAdditive(half4 a, half4 b, half blend) {
	return lerp(a, b, blend);
	//return float4(lerp(a.rgb, ToGrayscale(a) * b.rgb, blend), a.a * b.a);
	//return float4(lerp(a.rgb, b.rgb, blend), min(a.a, b.a));
}

half3 BlendDetailNormal(half3 n1, half3 n2, half mix)
{
	// Todo: If you want to do any non-slerp normal blending you need to keep
	// that in mind during the triplanar texture sampling. Transform base normal to
	// world space, but sample detail normal like regular and then blend it as per:
	//http://blog.selfshadow.com/publications/blending-in-detail/ ?
	return normalize(lerp(n1, n2, mix));
}

half BlendHeightLods(half h0, half h1, half closeness) {
	return lerp(h0, h0 * h1, closeness);
	//return h0 * h1;
	/*return h0 * weight + (1.0 - weight) * pow(h1, 1);*/
}

half4 HeightBlend(half height0, half height1, half height2, half height3) {
	half depth = 0.05;

	half ma = max(height0, height1) - depth;
	half b0 = max(height0 - ma, 0);
	half b1 = max(height1 - ma, 0);

	half4 res = half4(0, 0, 0, 0);
	res.r = b0 / (b0 + b1);
	res.g = b1 / (b0 + b1);
	res.b = 0;
	res.a = 0;

	return res;
}

void SplatmapMix(v2f_surf IN, inout SurfaceOutputStandard o)
{
	float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
	float3 worldNormal = float3(IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z);

	float3 originalWorldNormal = worldNormal;

	/* Worldspace uvs */
	float uvScale = _UVScale.x;
	float uvScaleLod = _UVScale.y;
	float uvScaleUnit = _UVScale.z;
	float3 worldUV = worldPos * uvScale;
	float3 worldUVLod = worldPos * uvScaleLod;
	float3 worldUVUnit = worldPos * uvScaleUnit;

	/* Triplanar tangent space bases */
	TpBasis tpb = GetTpBasis(worldPos, worldNormal);

	half distLerp = saturate((IN.camDist - _DetailDistance.x) / _DetailDistance.y);
	half closeness = (1.0 - distLerp);

	/* Sample Normals (triplanar, to world space) */

	// Sample detail normals
	half4 norm0 = Tex2DNormal(_Normal0, tpb, worldUV.xz);
	half4 norm1 = Tex2DTriplanarNormal(_Normal1, tpb, uvScale);
	half4 norm2 = Tex2DNormal(_Normal2, tpb, worldUV.xz);
	half4 norm3 = Tex2DNormal(_Normal3, tpb, worldUV.xz);

	// Sample LOD normals
	half4 norm0Lod = Tex2DNormal(_Normal0, tpb, worldUVLod.xz);
	half4 norm1Lod = Tex2DTriplanarNormal(_Normal1, tpb, uvScaleLod);
	half4 norm2Lod = Tex2DNormal(_Normal2, tpb, worldUVLod.xz);
	half4 norm3Lod = Tex2DNormal(_Normal3, tpb, worldUVLod.xz);

	/* Splat Layer Distribution */

	// Blend heightmap lods
	norm0.a = BlendHeightLods(norm0Lod.a, norm0.a, closeness);
	norm1.a = BlendHeightLods(norm1Lod.a, norm1.a, closeness);
	norm3.a = BlendHeightLods(norm3Lod.a, norm3.a, closeness);

	half4 splat_control = tex2D(_Control, IN.uv.xy);

	// Add extra snow to splatmap data

	half snowFalloffLow = saturate((worldPos.y - _SnowAltitude.x) / (_SnowAltitude.y - _SnowAltitude.x));
	half snowFalloffHigh = saturate((worldPos.y - (_SnowAltitude.x + 250)) / (_SnowAltitude.y + 250 - (_SnowAltitude.x + 250)));

	snowFalloffLow *= snowFalloffLow;
	snowFalloffHigh *= snowFalloffHigh;

	splat_control.a *= snowFalloffLow;

	const half3 SnowDir = normalize(float3(0.5, 1.0, 0.1));
	half extraSnow = pow(saturate(dot(worldNormal, SnowDir)), 5);
	extraSnow *= snowFalloffHigh;

	splat_control.a = max(splat_control.a, extraSnow);
	splat_control.r = max(0, splat_control.r - snowFalloffHigh * 0.5);

	splat_control.a = min(1, splat_control.a + splat_control.b * snowFalloffLow);
	//splat_control.b = max(0, splat_control.b - splat_control.a * splat_control.a);
	splat_control.b = max(0, splat_control.b - splat_control.r * 0.5);

	// Blend splatmaps with height maps
	norm0.a *= splat_control.r;
	norm1.a *= splat_control.g;
	norm2.a *= splat_control.b;
	norm3.a *= splat_control.a;

	norm0.a = pow(norm0.a, _HeightPow.r);
	norm1.a = pow(norm1.a, _HeightPow.g);
	norm2.a = pow(norm2.a, _HeightPow.b);
	norm3.a = pow(norm3.a, _HeightPow.a);

	half heightMax = norm0.a + norm1.a + norm2.a + norm3.a;
	half heightBlend0 = norm0.a / heightMax;
	half heightBlend1 = norm1.a / heightMax;
	half heightBlend2 = norm2.a / heightMax;
	half heightBlend3 = norm3.a / heightMax;

	/* Normals */

	// Additively blend in detail normals at close range
	norm0.rgb = BlendDetailNormal(norm0Lod.rgb, norm0.rgb, closeness);
	norm1.rgb = BlendDetailNormal(norm1Lod.rgb, norm1.rgb, closeness);
	//norm2.rgb = BlendDetailNormal(norm2Lod.rgb, norm2.rgb, closeness);
	norm3.rgb = BlendDetailNormal(norm3Lod.rgb, norm3.rgb, closeness);

	// Mix normals based on height blend values
	worldNormal = norm0.rgb * heightBlend0;
	worldNormal += norm1.rgb * heightBlend1;
	worldNormal += norm2.rgb * heightBlend2;
	worldNormal += norm3.rgb * heightBlend3;
	worldNormal = normalize(worldNormal);

	/* Diffuse */

	half4 splat0 = tex2D(_Splat0, worldUV.xz);
	half4 splat1 = Tex2DTriplanar(_Splat1, tpb, uvScale);
	half4 splat2 = tex2D(_Splat2, worldUV.xz);
	half4 splat3 = tex2D(_Splat3, worldUV.xz);

	half4 splat0Lod = tex2D(_Splat0, worldUVLod.xz);
	half4 splat1Lod = Tex2DTriplanar(_Splat1, tpb, uvScaleLod);
	half4 splat2Lod = tex2D(_Splat2, worldUVLod.xz);
	half4 splat3Lod = tex2D(_Splat3, worldUVLod.xz);

	splat0 = BlendAdditive(splat0Lod, splat0, closeness);
	splat1 = BlendAdditive(splat1Lod, splat1, closeness);
	//splat2 = BlendAdditive(splat2Lod, splat2, closeness);
	splat3 = BlendAdditive(splat3Lod, splat3, closeness);

	splat1.a = min(1.0, splat1.a + extraSnow * 0.2); // Snow-covered rock gets wet

	half4 diffuse = half4(0, 0, 0, 0);

//#ifdef TERRAIN_DEVMODE
//	diffuse += float4(0.1, 0.3, 0.6, 0.2) * heightBlend0;
//	diffuse += float4(0.8, 0.25, 0.0, 0.2) * heightBlend1;
//	diffuse += float4(1.0, 0.5, 0.0, 0.2) * heightBlend2;
//	diffuse += float4(0.95, 0.95, 0.95, 0.2) * heightBlend3;
//	half2 grid = frac(worldPos.xz * 0.25);
//	grid.x = grid.x > 0.95 ? 1.0 : 0.0;
//	grid.y = grid.y > 0.95 ? 1.0 : 0.0;
//	half proximity = 1.0-saturate((IN.camDist - 128) / 256);
//	diffuse = lerp(diffuse, float4(0, 0, 0, 0.2), max(grid.x, grid.y) * proximity);
//#else
	diffuse += splat0 * heightBlend0;
	diffuse += splat1 * heightBlend1;
	diffuse += splat2 * heightBlend2;
	diffuse += splat3 * heightBlend3;
//#endif

	// Todo: Occlusion in global normal alpha channel
	half occlusion = tex2D(_GlobalOcclusionTex, IN.uv.zw).x;

	/* Per-layer additional rim lighting */

	half3 viewDir = normalize(_WorldSpaceCameraPos.xyz - worldPos);
	half rim = 1 - dot(viewDir, worldNormal);
	half fresnel = half3(0, 0, 0);
	fresnel += _FresnelColor0 * (_FresnelGain.r * pow(rim, _FresnelPower.r) * heightBlend0) * (0.5 + splat0.a);
	fresnel += _FresnelColor1 * (_FresnelGain.g * pow(rim, _FresnelPower.g) * heightBlend1) * (0.5 + splat1.a);
	fresnel += _FresnelColor2 * (_FresnelGain.b * pow(rim, _FresnelPower.b) * heightBlend2) * (0.5 + splat2.a);
	fresnel += _FresnelColor3 * (_FresnelGain.a * pow(rim, _FresnelPower.a) * heightBlend3) * (0.5 + splat3.a);

	half ndotl = 0.5 + 0.5 * dot(worldNormal, _SunDir);
	fresnel *= 0;// ndotl * _SunIntensity / 8.0;

	/* Output to lighting stage */

	o.Albedo = diffuse.rgb;
	o.Emission = fresnel;
	o.Smoothness = diffuse.a;
	o.Occlusion = occlusion;
	o.Normal = worldNormal;
	o.Metallic = 0;
}

void SplatmapFinalGBuffer(SurfaceOutputStandard o, inout half4 diffuse, inout half4 specSmoothness, inout half4 normal, inout half4 emission)
{
	diffuse.rgb *= o.Alpha;
	specSmoothness *= o.Alpha;
	normal.rgb *= o.Alpha;
	emission *= o.Alpha;
}

#endif
