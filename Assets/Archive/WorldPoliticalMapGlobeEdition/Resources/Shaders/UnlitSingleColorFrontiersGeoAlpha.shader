Shader "World Political Map/Unlit Single Color Frontiers Alpha Geo" {
 
Properties {
    _Color ("Color", Color) = (1,1,1,1)
    _Thickness ("Thickness", Float) = 0.01
}

SubShader {
    Tags {
      "Queue"="Geometry-8"
      "RenderType"="Opaque"
    }
    ZWrite [_ZWrite]
    Cull Off

    GrabPass { "_BackgroundTexture" }

      Pass {

	   	CGPROGRAM
		#pragma vertex vert	
	   	#pragma exclude_renderers metal
	   	#pragma geometry geom
		#pragma fragment frag				
		#pragma target 4.0
		#include "UnityCG.cginc"
		
		fixed4 _Color;
		float _Thickness;
		sampler2D _BackgroundTexture;

		struct AppData {
			float4 vertex : POSITION;
            UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		struct v2g {
			float4 vertex : SV_POSITION;
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
		};

		struct g2f {
			float4 pos    : SV_POSITION;
			float4 grabPos : TEXCOORD0;
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
		};

		
		v2g vert(AppData v) {
            v2g o;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_OUTPUT(v2g, o);
            UNITY_TRANSFER_INSTANCE_ID(v, o);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
			o.vertex = UnityObjectToClipPos(v.vertex);
            return o;
		}


		[maxvertexcount(6)]
        void geom(line v2g p[2], inout TriangleStream<g2f> outputStream) {
           float4 p0 = p[0].vertex;
           float4 p1 = p[1].vertex;

           float4 ab = p1 - p0;
           float4 normal = float4(-ab.y, ab.x, 0, 0);
           normal.xy = normalize(normal.xy) * _Thickness;
           float4 tl = p0 - normal;
           float4 bl = p0 + normal;
           float4 tr = p1 - normal;
           float4 br = p1 + normal;
  		   float4 dd = float4(normalize(p1.xy-p0.xy), 0, 0) * _Thickness;

           g2f pIn;
           UNITY_INITIALIZE_OUTPUT(g2f, pIn);
           UNITY_TRANSFER_INSTANCE_ID(p[0], pIn);
           UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(p[0], pIn);
           pIn.pos = p0 - dd;
   		   pIn.grabPos = ComputeGrabScreenPos(pIn.pos);
           outputStream.Append(pIn);
           pIn.pos = bl;
		   pIn.grabPos = ComputeGrabScreenPos(pIn.pos);
           outputStream.Append(pIn);
           pIn.pos = tl;
           pIn.grabPos = ComputeGrabScreenPos(pIn.pos);
           outputStream.Append(pIn);
           pIn.pos = br;
           pIn.grabPos = ComputeGrabScreenPos(pIn.pos);
           outputStream.Append(pIn);
           pIn.pos = tr;
           pIn.grabPos = ComputeGrabScreenPos(pIn.pos);
           outputStream.Append(pIn);
           pIn.pos = p1 + dd;
           pIn.grabPos = ComputeGrabScreenPos(pIn.pos);
           outputStream.Append(pIn);
 		}
		
		fixed4 frag(g2f i) : SV_Target {
            UNITY_SETUP_INSTANCE_ID(i);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); 
			fixed4 bgcolor = tex2Dproj(_BackgroundTexture, i.grabPos);
			return fixed4(_Color.rgb * _Color.a + bgcolor.rgb * (1.0 - _Color.a), _Color.a);
		}
		ENDCG
    }
}

SubShader {
    Cull Off
    ZWrite [_ZWrite]
    Tags {
		"Queue"="Geometry-8"
        "RenderType"="Opaque"
    }
    Blend SrcAlpha OneMinusSrcAlpha
    Pass {
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
