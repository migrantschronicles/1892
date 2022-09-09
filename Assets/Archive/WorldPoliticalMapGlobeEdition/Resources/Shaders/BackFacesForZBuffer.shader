Shader "World Political Map/BackFacesForZBuffer" {

Properties {
	_Color ("Default Color", Color) = (1,1,1,1)
	_Inverted ("Inverted Mode", Float) = 0
}
SubShader {
	Tags { "Queue"="Geometry-21" "RenderType"="Opaque" }
    Cull Front // Required for inverted mode
	ZWrite On
	Pass {
	
	    	CGPROGRAM
		#pragma vertex vert	
		#pragma fragment frag				
		
		#include "UnityCG.cginc"
		
		fixed4 _Color;
		fixed _Inverted;

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
			if (_Inverted) {
                o.vertex = UnityObjectToClipPos(v.vertex);
                #if UNITY_REVERSED_Z
    				o.vertex.z -= 0.001;
                #else
    				o.vertex.z += 0.001;
                #endif	
			} else {
                float3 u = v.vertex.xyz;
                float3 viewDir = ObjSpaceViewDir(v.vertex);
                float3 n = normalize(viewDir);
                float3 projVertex = u - n * dot(u,n) / dot(n,n);
                o.vertex.xyz = projVertex; // (v.vertex.xyz + projVertex) * 0.5;
                o.vertex = UnityObjectToClipPos(o.vertex);
            }
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
