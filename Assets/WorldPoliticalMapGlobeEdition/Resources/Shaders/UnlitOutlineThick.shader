Shader "World Political Map/Unlit Outline Thick" {
 
Properties {
    _Color ("Color", Color) = (1,1,1,1)
    _Thickness ("Thickness", Float) = 0.05
}
 
SubShader {
    Tags {
       "Queue"="Geometry-6"
       "RenderType"="Opaque"
  	}
  	ZWrite [_ZWrite]
    Cull Off
    Pass {
    	CGPROGRAM
		#pragma vertex vert	
		#pragma fragment frag				
		#include "UnityCG.cginc"

		fixed4 _Color;
		float _Thickness;
        float4x4 _CustomObjectToWorld;

		struct AppData {
			float4 vertex : POSITION;
            float4 uv     : TEXCOORD0;
            UNITY_VERTEX_INPUT_INSTANCE_ID
		};

        struct v2f {
            float4 pos : SV_POSITION;
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
        };

        float4 ComputePos(float4 v) {
            float4 vertex = UnityObjectToClipPos(v);
            return vertex;
        }

		v2f vert(AppData v) {
            v2f o;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_OUTPUT(v2f, o);
            UNITY_TRANSFER_INSTANCE_ID(v, o);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

		    float4 p0 = ComputePos(v.vertex);
            // support for dynamic batching
            if (dot(v.vertex.xyz, v.vertex.xyz) > 0.7 * 0.7) {
                v.uv.xyz = mul(_CustomObjectToWorld, float4(v.uv.xyz, 1.0));
            }
            float4 p1 = ComputePos(v.uv);

            float4 ab = p1 - p0;

            float4 normal = float4(-ab.y, ab.x, 0, 0);
            normal.xy = normalize(normal.xy) * _Thickness;

            float aspect = _ScreenParams.x / _ScreenParams.y;
            normal.y *= aspect;

            o.pos = p0 + normal * v.uv.w - normal.yxww * float4(0.5,-0.5,0,0);
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
