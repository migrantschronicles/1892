Shader "World Political Map/Unlit Outline Geo" {
 
Properties {
    _Color ("Color", Color) = (1,1,1,1)
    _Thickness ("Thickness", Float) = 0.01
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
	   	#pragma exclude_renderers metal
		#pragma vertex vert	
		#pragma geometry geom
		#pragma fragment frag				
		#pragma target 4.0
		#include "UnityCG.cginc"
		
		fixed4 _Color;
		float _Thickness;

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
           outputStream.Append(pIn);
           pIn.pos = bl;
           outputStream.Append(pIn);
           pIn.pos = tl;
           outputStream.Append(pIn);
           pIn.pos = br;
           outputStream.Append(pIn);
           pIn.pos = tr;
           outputStream.Append(pIn);
           pIn.pos = p1 + dd;
           outputStream.Append(pIn);
 		}
		
		fixed4 frag(g2f i) : SV_Target {
            UNITY_SETUP_INSTANCE_ID(i);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
			return _Color;
		}

		ENDCG
    }
            
}

// For non-geometry shader compatible platforms
SubShader {
    Tags {
       "Queue"="Geometry-6"
       "RenderType"="Opaque"
  	}
  	ZWrite [_ZWrite]
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
    
   // SECOND STROKE (RIGHT) ***********
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
			o.vertex.x += 2.0 * (o.vertex.w/_ScreenParams.x);
            return o;
		}
		
        fixed4 frag(v2f i) : SV_Target {
            UNITY_SETUP_INSTANCE_ID(i);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); 
			return _Color;					
		}
			
		ENDCG
    }
    
      // THIRD STROKE (UP) ***********
 
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
			o.vertex.y += 2.0 * (o.vertex.w/_ScreenParams.y);
            return o;
		}
		
        fixed4 frag(v2f i) : SV_Target {
            UNITY_SETUP_INSTANCE_ID(i);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); 
			return _Color;					
		}
			
		ENDCG
    }
    
       
      // FOURTH STROKE (LEFT) ***********
 
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
			o.vertex.x -= 2.0 * (o.vertex.w/_ScreenParams.x);	
            return o;
		}
		
        fixed4 frag(v2f i) : SV_Target {
            UNITY_SETUP_INSTANCE_ID(i);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); 
			return _Color;					
		}
			
		ENDCG
    }
    
    // FIFTH STROKE (DOWN) ***********
 
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
			o.vertex.y -= 2.0 * (o.vertex.w/_ScreenParams.y);	
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

 
