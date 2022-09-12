Shader "World Political Map/Unlit Country Surface Single Color Alpha" {
 
Properties {
    _Color ("Color", Color) = (1,1,1)
    _MainTex ("Texture", 2D) = "white" {}
}
 
SubShader {
    Tags {
        "Queue"="Geometry-17"
        "RenderType"="Transparent"
    	}
    Blend SrcAlpha OneMinusSrcAlpha
    ZWrite Off
	Pass {
			CGPROGRAM	
			#pragma fragment frag
			#pragma vertex vert	
			#include "UnityCG.cginc"

			fixed4 _Color;	
			fixed _Intensity;

			struct AppData {
				float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f {
				float4 pos : SV_POSITION;
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
