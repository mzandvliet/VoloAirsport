Shader "RenderDepth" 
{
    SubShader {
        Tags { "RenderType"="Opaque" }
        Pass {
            ZWrite On
            ColorMask 0
            Fog { Mode Off }
        }
    }
}