Shader "World Political Map/Unlit Country Highlight" {
Properties {
    _Color ("Tint Color", Color) = (1,1,1,0.5)
    _Intensity ("Intensity", Range(0.0, 2.0)) = 1.0
    _MainTex ("Texture", 2D) = "white" {}
}
SubShader {
    Tags {
        "Queue"="Geometry-16"
        "IgnoreProjector"="True"
        "RenderType"="Transparent"
    }
			Cull Off
			ZWrite Off
			ZTest Always
			Blend SrcAlpha OneMinusSrcAlpha 
		Pass {
			CGPROGRAM	
			#pragma fragment frag
			#pragma vertex vert	
			#include "UnityCG.cginc"

			fixed4 _Color;	
			fixed _Intensity;
			sampler2D _MainTex;

			struct AppData {
				float4 vertex : POSITION;
				float2 texcoord: TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f {
				float4 pos : SV_POSITION;
				float2 uv: TEXCOORD0;
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f vert(AppData v) {
				v2f o;						
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_OUTPUT(v2f, o);
            UNITY_TRANSFER_INSTANCE_ID(v, o);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.pos = UnityObjectToClipPos(v.vertex);	
				o.uv = v.texcoord;
				return o;								
			}

			fixed4 frag(v2f i) : SV_Target {
            UNITY_SETUP_INSTANCE_ID(i);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); 
				fixed4 pixel = tex2D(_MainTex, i.uv);
				return pixel * _Color * _Intensity;			
			}

			ENDCG

		}
	}	
}
