Shader "Custom/RingInner" {
Properties {
	_Color ("Main Color", Color) = (0.2,0.2,1,0.4)
	_FlashColor ("Flash Color", Color) = (1,1,1,0.7)
	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
	_Flash ("Flash", Range (0, 1)) = 0
}

SubShader {
	Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
	LOD 200
	Cull off

CGPROGRAM
#pragma surface surf Lambert alpha

sampler2D _MainTex;
fixed4 _Color;
fixed4 _FlashColor;
half _Flash;

struct Input {
	float2 uv_MainTex;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 tint = lerp(_Color, _FlashColor, _Flash);
	//fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * tint;
	o.Emission = tint.rgb;
	o.Alpha = tint.a;
}
ENDCG
}

Fallback "Transparent/VertexLit"
}
