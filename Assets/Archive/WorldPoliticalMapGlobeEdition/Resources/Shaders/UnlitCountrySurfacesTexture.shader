Shader "World Political Map/Unlit Country Surface Texture" {
 
Properties {
    _Color ("Color", Color) = (1,1,1)
    _MainTex ("Texture", 2D) = "white"
}
 
SubShader {
    Tags {
        "Queue"="Geometry-17"
        "RenderType"="Opaque"
    }

	Blend SrcAlpha OneMinusSrcAlpha
	ZWrite Off
	Pass {

    	CGPROGRAM
		#pragma vertex vert	
		#pragma fragment frag				
		
		#include "UnityCG.cginc"
		
		fixed4 _Color;
		sampler2D _MainTex;

		struct AppData {
			float4 vertex : POSITION;
			float4 uv : TEXCOORD0;
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
            o.uv = v.uv;
            return o;
		}
		
		fixed4 frag(v2f i) : SV_Target {
            UNITY_SETUP_INSTANCE_ID(i);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); 
			return tex2D(_MainTex, i.uv) * _Color;
		}
			
		ENDCG
    }

}
 
}
