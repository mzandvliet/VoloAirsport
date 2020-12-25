Shader "Geometry2D"
{
	Properties 
	{
		_Color ("Color", Color) = (1,1,1,1)
	}

	SubShader 
	{
		// We want to support transparent geometry
		Tags { "Queue" = "Transparent" }

		// Use the specified color property to color the vertices
		Color[_Color]

		Pass
		{
			ZWrite Off
			ZTest Off							// We will need our 2D geometry to render on top of everything else, so we will disable Z tests
			Cull Off							// We will not concern ourselves with culling when rendering 2D geometry, so we will turn it off
			Fog { Mode Off }
			Blend SrcAlpha OneMinusSrcAlpha		// Enable alpha blending for meshes with transparent vertices
		}
	} 
}
