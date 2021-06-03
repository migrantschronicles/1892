Shader "World Political Map/Unlit Single Color Cursor" {
 
Properties {
    _Color ("Color", Color) = (1,1,1)
}
 
SubShader {
    Tags {
        "Queue"="Transparent"
        "RenderType"="Transparent"
    }
    ZWrite [_ZWrite]
 	Pass {
    	CGPROGRAM
		#pragma vertex vert	
		#pragma fragment frag				
		#pragma multi_compile _ WPM_INVERTED

		#include "UnityCG.cginc"
		
		fixed4 _Color;

		struct AppData {
			float4 vertex : POSITION;
            UNITY_VERTEX_INPUT_INSTANCE_ID
		};

        struct v2f {
            float4 pos    : SV_POSITION;
			float4 scrPos : TEXCOORD0;
			//float hidden  : TEXCOORD1; // no longer used since the horizontal cursor needs to be shown in full
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
        };
		
		v2f vert(AppData v) {
            v2f o;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_OUTPUT(v2f, o);
            UNITY_TRANSFER_INSTANCE_ID(v, o);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

			//float3 viewDir = ObjSpaceViewDir(v.vertex);
			//o.hidden = dot(v.vertex.xyz, viewDir);
		
			o.pos = UnityObjectToClipPos(v.vertex);
			o.scrPos = ComputeScreenPos(o.pos);
	
            return o;

		}
		
		fixed4 frag(v2f i) : SV_Target {
            UNITY_SETUP_INSTANCE_ID(i);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); 
			// Compute view from camera
            #if !WPM_INVERTED
			    //clip(i.hidden);
            #endif

			float2 wcoord = i.scrPos.xy/i.scrPos.w;
			float grad = abs(ddy(wcoord.y) / ddx(wcoord.x));
			float xm = lerp (wcoord.x, wcoord.y, grad > 0.5);
			xm = fmod( floor(xm * 1000.0) / 4,2);

			//clip( xm - 0.5 );
			return _Color;					
		}
			
		ENDCG
    }

}
}
 
