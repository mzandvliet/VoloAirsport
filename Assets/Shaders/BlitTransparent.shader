// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/BlitTransparent" {
	Properties {
    _MainTex ("Texture", 2D) = "white" {}
   }

	SubShader {
    Cull Off
    ZWrite Off
    Lighting Off
    Ztest Always
    Blend SrcAlpha OneMinusSrcAlpha
    //Blend One OneMinusSrcAlpha // Premultiplied Alpha Blending
    //Blend OneMinusDstColor One
    //Blend DstColor SrcColor // 2x Multiplicative
    //Blend DstColor Zero // Multiplicative
    //Blend DstColor Zero
    //Blend One One
    //Blend Off

    Pass {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag

      #include "UnityCG.cginc"

      sampler2D _MainTex;

      struct appdata_t {
        float4 vertex : POSITION;
        float2 texcoord : TEXCOORD0;
      };

      struct v2f {
        float4 vertex : SV_POSITION;
        float2 texcoord : TEXCOORD0;
      };

      //float4 _MainTex_ST;

      v2f vert (appdata_t v) {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        //o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
        o.texcoord = v.texcoord;
        return o;
      }

      fixed4 frag (v2f i) : SV_Target {
        return tex2D(_MainTex, i.texcoord);
      }
      ENDCG

    }
  }
  Fallback Off
}
