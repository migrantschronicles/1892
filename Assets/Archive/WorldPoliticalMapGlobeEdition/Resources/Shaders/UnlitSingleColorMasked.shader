Shader "World Political Map/Unlit Single Color Masked" {
 
Properties {
    _Color ("Color", Color) = (1,1,1)
    _MainTex ("Mask", 2D) = "black" {}
}
 
SubShader {
    Tags {
        "Queue"="Geometry-7"
        "RenderType"="Opaque"
    }
    ZWrite [_ZWrite]
    Pass {
    	CGPROGRAM
    	#pragma vertex vert
		#pragma fragment frag
    
    	#include "UnityCG.cginc"
    	
    	uniform sampler2D _MainTex;
    	uniform float4 _MainTex_TexelSize;
    	fixed4 _Color;
    	
    	struct v2f {
			float4 pos : SV_POSITION;
			float2 uv  : TEXCOORD0;
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
		};
    	
    	
    	
		v2f vert( appdata_img v ) {
			v2f o;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_OUTPUT(v2f, o);
            UNITY_TRANSFER_INSTANCE_ID(v, o);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
			o.pos = UnityObjectToClipPos (v.vertex);

			const float PI = 3.141529;
    		const float invPI = 1.0 / PI;
    		const float PI2 = PI * 2.0;

		   	float phi   = asin ( v.vertex.y * 2);
			float theta = atan2( v.vertex.x,  v.vertex.z);
			float lonDec = -theta;
			float latDec = phi;
			float x = (lonDec+PI)/PI2;
			float y = latDec * invPI + 0.5;
			
			o.uv = float2(x, y);

			return o;
		}
    	
    	fixed4 frag(v2f i) : SV_Target {
            UNITY_SETUP_INSTANCE_ID(i);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); 
			fixed m = tex2D(_MainTex, i.uv).b;
			clip(m - 0.1);
			return _Color;
    	}
    	
    	ENDCG
    }
}
 
}
