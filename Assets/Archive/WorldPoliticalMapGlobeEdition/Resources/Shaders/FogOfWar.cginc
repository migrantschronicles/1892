		#include "UnityCG.cginc"
		
		sampler2D _MainTex, _MaskTex, _GrabTexture;
		float3 _Color, _Color2;
		float _Elevation;
		float _Alpha;
		float _Noise;
		
		struct appdata {
			float4 vertex : POSITION;
			half2 texcoord: TEXCOORD0;
			float4 normal: NORMAL;
            UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		struct v2f {
			float4 pos : SV_POSITION;
			float2 uv: TEXCOORD0;
			float2 uv1: TEXCOORD1;
			float2 t: TEXCOORD3;
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
		};

		struct v2fMask {
			float4 pos : SV_POSITION;
			float2 uv: TEXCOORD0;
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
		};

		v2f vert(appdata v) {
			v2f o;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_OUTPUT(v2f, o);
            UNITY_TRANSFER_INSTANCE_ID(v, o);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
			v.vertex.xyz *= 1.0 + _Elevation;
			o.pos = UnityObjectToClipPos(v.vertex);
			o.uv = v.texcoord;
			o.t = float2(-_Time.x * 0.3, 0);
			o.uv1 = v.texcoord + o.t; 
			return o;
		}
		
		
		v2fMask vertMask(appdata v) {
			v2fMask o;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_OUTPUT(v2fMask, o);
            UNITY_TRANSFER_INSTANCE_ID(v, o);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
			o.pos = UnityObjectToClipPos(v.vertex);
			o.uv = v.texcoord;
			return o;
		}
		
		
		
		float4 fragMask(v2fMask i) : SV_Target {
            UNITY_SETUP_INSTANCE_ID(i);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); 
			// mask fog of war
			float mask = tex2D (_MaskTex, i.uv).r;
			clip(0.99 - mask);
			return float4(1,0,0,1);
		}
		
		
		float4 frag(v2f i) : SV_Target {
            UNITY_SETUP_INSTANCE_ID(i);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); 
			// Compute noise operators
			float c0 = tex2D (_MainTex, i.uv1).r;
			float2 t2 = i.t * 1.1 + c0 * 0.1;
			float c1 = tex2D (_MainTex, -i.uv * 2.0 + t2).r;
			float c = c0 * (1 - c1) + c1;
			c *= c;	// increase contrast
			float3 rgb = lerp(_Color, _Color2, c) * c;	// blend 2 colors
			rgb = lerp(_Color, rgb, _Noise);
			
			// mask fog of war
			#if defined(WPM_NO_MASK)
			float mask = 1;
			#else
			float mask = tex2D (_MaskTex, i.uv).r;
			#endif
			
			// add sneak factor
//			float sneak = saturate(0.95 - pow( abs(dot(i.viewDir, i.norm) ) , _Sneak ));
//			mask += sneak;

			// avoid showing seams on poles
			float oy = abs(i.uv.y - 0.5);
			float seam = 1.0 - saturate((oy - 0.46) / 0.04);
			rgb *= seam;
			
			// final fog color
			float4 color = fixed4(rgb, mask * _Alpha);
			return color;
		}