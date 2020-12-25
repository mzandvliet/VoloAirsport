Shader "Custom/Screenfader Post Effect" {
Properties {
    _MainTex ("Base (RGB)", 2D) = "white" {}
    _Opacity("Opacity", Range(0, 1)) = 0.0
    _FadeColor("FadeColor", Color) = (0,0,0,1)
}

SubShader {
    Pass {
        ZTest Always Cull Off ZWrite Off

        CGPROGRAM
        #pragma vertex vert_img
        #pragma fragment frag
        #include "UnityCG.cginc"

        uniform sampler2D _MainTex;
        float _Opacity;
        float4 _FadeColor;

        fixed4 frag (v2f_img i) : SV_Target {
          fixed4 original = tex2D(_MainTex, i.uv);
          return lerp(original, _FadeColor, _Opacity);
        }
        ENDCG
    }
}

Fallback off

}
