Shader "World Political Map/Unlit Texture 16K" {
 
Properties {
    _Color ("Color", Color) = (1,1,1)
	_TexTL ("Tex TL", 2D) = "white" {}
	_TexTR ("Tex TR", 2D) = "white" {}
	_TexBL ("Tex BL", 2D) = "white" {}
	_TexBR ("Tex BR", 2D) = "white" {}
}
 
SubShader {
  	Tags { "Queue"="Geometry-20" "RenderType"="Opaque" }
		Lighting Off
		ZWrite Off
		Pass {
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				
				#include "UnityCG.cginc"
				
				sampler2D _TexTL;
				sampler2D _TexTR;
				sampler2D _TexBL;
				sampler2D _TexBR;

				struct v2f {
					float4 pos : SV_POSITION;
					float2 uv : TEXCOORD0;
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
				};
        
				v2f vert (appdata_base v) {
					v2f o;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_OUTPUT(v2f, o);
            UNITY_TRANSFER_INSTANCE_ID(v, o);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					o.pos 				= UnityObjectToClipPos (v.vertex);
					o.uv				= v.texcoord.xy;
					return o;
				 }
				
				half4 frag (v2f i) : SV_Target {
            UNITY_SETUP_INSTANCE_ID(i);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); 
					half4 color;
					// compute Earth pixel color
					if (i.uv.x<0.5) {
						if (i.uv.y>0.5) {
							color = tex2Dlod(_TexTL, float4(i.uv.x * 2.0, (i.uv.y - 0.5) * 2.0, 0, 0));
						} else {
							color = tex2Dlod(_TexBL, float4(i.uv.x * 2.0, i.uv.y * 2.0, 0, 0));
						}
					} else {
						if (i.uv.y>0.5) {
							color = tex2Dlod(_TexTR, float4((i.uv.x - 0.5) * 2.0f, (i.uv.y - 0.5) * 2.0, 0, 0));
						} else {
							color = tex2Dlod(_TexBR, float4((i.uv.x - 0.5) * 2.0f, i.uv.y * 2.0, 0, 0));
						}
					}

		  		   return color;
				}
			
			ENDCG
		}
	}
}