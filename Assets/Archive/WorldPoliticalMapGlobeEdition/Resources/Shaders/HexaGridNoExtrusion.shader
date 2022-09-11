Shader "World Political Map/HexaGridNoExtrusion" {
    Properties {
        _Color ("Color", Color) = (1,0.5,0.5,1)
    }
    SubShader {
	Tags { "RenderType"="Opaque" "Queue"="Geometry-11" }
	Pass {
		ZWrite [_ZWrite]
	
    	CGPROGRAM
		#pragma vertex vert	
		#pragma fragment frag				
		
		#include "UnityCG.cginc"
		
		fixed4 _Color;

		struct AppData {
			float4 vertex : POSITION;
            UNITY_VERTEX_INPUT_INSTANCE_ID
		};

        struct v2f {
            float4 vertex : SV_POSITION;
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
        };

		v2f vert(AppData v) {
            v2f o;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_OUTPUT(v2f, o);
            UNITY_TRANSFER_INSTANCE_ID(v, o);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
			o.vertex = UnityObjectToClipPos(v.vertex);
            return o;
		}
		
		fixed4 frag(v2f i) : SV_Target {
            UNITY_SETUP_INSTANCE_ID(i);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); 
			return _Color;					
		}
			
		ENDCG
    }
    }
}