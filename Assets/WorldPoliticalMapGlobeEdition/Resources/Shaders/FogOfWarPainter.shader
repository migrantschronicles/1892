Shader "World Political Map/FogOfWarPainter" {
	Properties {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_PaintData ("Location", Vector) = (0,0,0,1)
		_PaintStrength ("Paint Strength", Float) = 0.2
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		Pass {
		
		CGPROGRAM
		#pragma vertex vert	
		#pragma fragment frag
        #pragma fragmentoption ARB_precision_hint_fastest
        #include "UnityCG.cginc"

		sampler2D _MainTex;
		float4 _PaintData;
		float _PaintStrength;
		
		struct appdata {
			float4 vertex : POSITION;
			float2 texcoord: TEXCOORD0;
		};

		struct v2f {
			float4 pos : SV_POSITION;
			float2 uv: TEXCOORD0;
		};
		
		v2f vert(appdata v) {
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
			o.uv = v.texcoord;
			return o;
		}
		
		
		fixed4 frag(v2f i) : SV_Target {
			fixed c = tex2D (_MainTex, i.uv).r;
			float2 dd = abs(i.uv - _PaintData.xy);
			dd.x = min(dd.x, 1.0 - dd.x);
			dd.x *= 2.0;

			float dy = abs(i.uv.y - 0.5);
			float cx = cos (3.1415927 * dy);
//			if (dy > 0.5) {
//				cx *= 1.0 + (dy - 0.5f) * 0.33f;
//			}
			dd.x *= cx;
			
			float dist = dot(dd, dd);
			if (dist<_PaintData.z) c = lerp(c, _PaintData.w, _PaintStrength);
			return c.xxxx;
		}
		ENDCG
		}
	} 
}
