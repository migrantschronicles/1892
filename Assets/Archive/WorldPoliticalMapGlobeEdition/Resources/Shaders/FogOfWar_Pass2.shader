Shader "World Political Map/FogOfWar Pass 2" {
	Properties {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Color ("Fog Color 1", Color) = (1,1,0)
		_Color2 ("Fog Color 2", Color) = (0,1,1)
		_MaskTex ("Mask Texture (R)", 2D) = "white" {}
		_Elevation ("_Elevation", Float) = 1.0
		_Alpha ("Alpha", Float) = 1.0
		_Noise ("Noise", Float) = 1.0
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue"="Transparent+52" "DisableBatching"="True" "ForceNoShadowCasting"="True" "IgnoreProjector"="True" }

       	Pass {
       	  	Stencil {
       			Ref 2
       			Comp NotEqual
       			Pass Keep
       		}
			ZWrite On
			ZTest Always
  			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
        	#define WPM_NO_MASK
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
        	#pragma fragmentoption ARB_precision_hint_fastest
			#include "FogOfWar.cginc"
        	ENDCG
       	}

      		      		
	} 
}
