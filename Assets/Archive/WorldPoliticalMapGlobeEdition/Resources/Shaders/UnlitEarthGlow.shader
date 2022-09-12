Shader "World Political Map/Unlit Earth Glow" {

	Properties {
		_GlowColor("Glow Color", Color) = (0.4, 0.3, 0.9, 1)
		_GlowIntensity("Glow Intensity", Range(0, 5)) = 2
		_GlowGrow("Glow Grow", Range(0.9, 1.2)) = 0.9
		_GlowFallOff("Glow FallOff", Range(0, 5)) = 1
		_SunLightDirection("Sun Light Direction", Vector) = (0,0,1)		
	}
	
	Subshader {
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Front
		Zwrite Off
		Pass {
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				
				#include "UnityCG.cginc"
				
				half3 _GlowColor;
				half _GlowGrow, _GlowIntensity, _GlowFallOff;
				half3 _SunLightDirection;
				
				struct v2f {
					float4 pos  : SV_POSITION;
					half4 color: COLOR0;
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
				};

				v2f vert (appdata_tan v) {
					v2f o;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_OUTPUT(v2f, o);
            UNITY_TRANSFER_INSTANCE_ID(v, o);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

					v.vertex 		   *= _GlowGrow;
					o.pos 				= UnityObjectToClipPos (v.vertex);

					float3 center = float3(unity_ObjectToWorld[0][3], unity_ObjectToWorld[1][3], unity_ObjectToWorld[2][3]);
					float3 v3CameraPos = _WorldSpaceCameraPos - center;
					float fCameraHeight2 = dot(v3CameraPos, v3CameraPos);
			
					float3 v3Pos = mul(unity_ObjectToWorld, v.vertex).xyz - center;
					float3 v3Ray = v3Pos - v3CameraPos;
					float t = length(v3Ray);
					v3Ray /= t;
				
					// Calculate the closest intersection of the ray with the outer atmosphere (which is the near point of the ray passing through the atmosphere)
					float B = 2.0 * dot(v3CameraPos, v3Ray);
					float r = length(v3Pos);
					float C = fCameraHeight2 - r * r;
					float t0 = 0;
					if (C>0) {
						float fDet = B*B - 4.0 * C;
						t0 = 0.5 * (-B - sqrt(fDet));
					}
					t = (t-t0) / r;	// make it independent of scale

					half3 wNormal  = normalize(v3Pos);
					half d 				= dot(-wNormal, _SunLightDirection);
					half atten 			= 1.0 - saturate(d * _GlowFallOff);

					t = saturate(t * atten);
                    
					o.color			    = fixed4(_GlowColor + t * t, t) * _GlowIntensity;

					return o;
				 }
				
				half4 frag (v2f i) : SV_Target {
            UNITY_SETUP_INSTANCE_ID(i);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); 
                    half4 color = i.color;
                     #if !UNITY_COLORSPACE_GAMMA
                    color.rgb = GammaToLinearSpace(color.rgb);
                    #endif
					return saturate(color);
				}
			
			ENDCG
		}
	}
}