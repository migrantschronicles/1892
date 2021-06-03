Shader "World Political Map/Unlit Earth 16K Scenic" {

	Properties {
		_TexTL ("Tex TL", 2D) = "white" {}
		_TexTR ("Tex TR", 2D) = "white" {}
		_TexBL ("Tex BL", 2D) = "white" {}
		_TexBR ("Tex BR", 2D) = "white" {}
		_NormalMap ("Normal Map", 2D) = "bump" {}
		_BumpAmount ("Bump Amount", Range(0, 1)) = 0.5
		_SpecularPower("Specular Power", Float) = 32.0
		_SpecularIntensity("Specular Intensity", Float) = 2.0
		_CloudMap ("Cloud Map", 2D) = "black" {}
		_CloudSpeed ("Cloud Speed", Range(-1, 1)) = -0.04
		_CloudAlpha ("Cloud Alpha", Range(0, 1)) = 1
		_CloudShadowStrength ("Cloud Shadow Strength", Range(0, 1)) = 0.2
		_CloudElevation ("Cloud Elevation", Range(0.001, 0.1)) = 0.003
		_SunLightDirection("Sun Light Direction", Vector) = (0,0,1)
		_AtmosphereColor("Atmosphere Color", Color) = (0.4, 0.3, 0.9, 1)
		_AtmosphereAlpha("Atmosphere Alpha", Range(0,1)) = 1
		_AtmosphereFallOff("Atmosphere Falloff", Range(0,5)) = 1.35
		_ScenicIntensity("Intensity", Range(0,1)) = 1
		_Brightness("Brightness", Range(1,3)) = 1.5
		_Contrast("Contrast", Range(0,2)) = 1.1
		_AmbientLight("Ambient Light", Range(0,1)) = 0.1		
	}
	
	Subshader {
		Tags { "Queue"="Geometry-20" "RenderType"="Opaque" }
		Lighting Off
		ZWrite Off
		Pass {
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma multi_compile __ WPM_SPECULAR_ENABLED
				#pragma multi_compile __ WPM_BUMPMAP_ENABLED
				#pragma multi_compile __ WPM_CLOUDSHADOWS_ENABLED

				#include "UnityCG.cginc"
				
				sampler2D _TexTL;
				sampler2D _TexTR;
				sampler2D _TexBL;
				sampler2D _TexBR;
				sampler2D _NormalMap;
				sampler2D _CloudMap;
				half _BumpAmount;
				half _CloudSpeed;
				half _CloudAlpha;
				half _CloudShadowStrength;
				half _CloudElevation;
				half3 _SunLightDirection;
				half4 _AtmosphereColor;
				half _AtmosphereAlpha;
				half _AtmosphereFallOff;
				half _ScenicIntensity;
				half _Brightness;
				half _Contrast;
				half _AmbientLight;	
				float _SpecularPower;
				float _SpecularIntensity;

				struct v2f {
					float4 pos     : SV_POSITION;
					float2 uv      : TEXCOORD0;
					half3 viewDir: TEXCOORD1;
					half3 normal: TEXCOORD2;
					half2 scatter: TEXCOORD3;
					#if WPM_BUMPMAP_ENABLED
					half3 tspace0 : TEXCOORD4; // tangent.x, bitangent.x, normal.x
                	half3 tspace1 : TEXCOORD5; // tangent.y, bitangent.y, normal.y
                	half3 tspace2 : TEXCOORD6; // tangent.z, bitangent.z, normal.z
                	#endif
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO

				};
        
				v2f vert (appdata_tan v) {
					v2f o;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_OUTPUT(v2f, o);
            UNITY_TRANSFER_INSTANCE_ID(v, o);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					o.pos 				= UnityObjectToClipPos (v.vertex);
					o.uv 				= v.texcoord;
					half3 wNormal		= UnityObjectToWorldNormal(v.normal);
					o.normal			= wNormal;
					o.viewDir 			= normalize(WorldSpaceViewDir(v.vertex));
					// compute scatter vectors
					half d 				= dot(-wNormal, _SunLightDirection);
					o.scatter 			= half2(1.0 - saturate(d * _AtmosphereFallOff),  0);

					// normal stuff
					#if WPM_BUMPMAP_ENABLED
	                half3 wTangent = UnityObjectToWorldDir(v.tangent.xyz);
        	        half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
            	    half3 wBitangent = cross(wNormal, wTangent) * tangentSign;
                	// output the tangent space matrix
	                o.tspace0 = half3(wTangent.x, wBitangent.x, wNormal.x);
    	            o.tspace1 = half3(wTangent.y, wBitangent.y, wNormal.y);
        	        o.tspace2 = half3(wTangent.z, wBitangent.z, wNormal.z);
        	        #endif

					return o;
				 }

				 half3 projectOnPlane(half3 v, half3 n) {
				 	return v - dot(v, n) * n;
				 }

				half4 frag (v2f i) : SV_Target {
            UNITY_SETUP_INSTANCE_ID(i);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); 

					// compute Earth pixel color
					half4 color;
					
					// compute Earth pixel color
					if (i.uv.x<0.5) {
						if (i.uv.y>0.5) {
							color = tex2Dlod(_TexTL, float4(i.uv.x * 2.0, (i.uv.y - 0.5) * 2.0, 0, 0));
						} else {
							color = tex2Dlod(_TexBL, float4(i.uv.x * 2.0, i.uv.y * 2.0, 0, 0));
						}
					} else {
						if (i.uv.y>0.5) {
							color = tex2Dlod(_TexTR, float4((i.uv.x - 0.5) * 2.0f, (i.uv.y - 0.5) * 2.0, 0, 0));
						} else {
							color = tex2Dlod(_TexBR, float4((i.uv.x - 0.5) * 2.0f, i.uv.y * 2.0, 0, 0));
						}
					}
					
					// sphere normal (without bump-map)
					half3 snormal = normalize(i.normal);

				// specular reflection
				#if WPM_SPECULAR_ENABLED
				float3 worldRefl = reflect(_SunLightDirection, snormal);
        	    float spec = pow(max(0.0, dot(-i.viewDir, worldRefl)), _SpecularPower);
				color.rgb += (spec * color.a * _SpecularIntensity);
				#endif


					// transform normal from tangent to world space
					#if WPM_BUMPMAP_ENABLED
					half3 tnormal = UnpackNormal(tex2D(_NormalMap, i.uv)); 
                	half3 worldNormal;
                	worldNormal.x = dot(i.tspace0, tnormal);
                	worldNormal.y = dot(i.tspace1, tnormal);
                	worldNormal.z = dot(i.tspace2, tnormal);
                	half3 normal = normalize(lerp(snormal, worldNormal, _BumpAmount));
                	#else
                	half3 normal = snormal;
                	#endif

                	// Clouds
                	half  LdotS = saturate(dot(_SunLightDirection, snormal));
					half2 t = half2(_Time[0] * _CloudSpeed, 0);
					half2 disp = -i.viewDir * _CloudElevation;
					half4 cloud = tex2D (_CloudMap, i.uv + t - disp);
					cloud.rgb *= (LdotS + _AmbientLight);

					// Cloud shadows
					#if WPM_CLOUDSHADOWS_ENABLED
					const half2 c = half2(0.998,0);
					half3 proj  = projectOnPlane(_SunLightDirection, snormal);
					half3 up    = projectOnPlane(half3(0,1,0), snormal);
					half3 right = projectOnPlane(half3(1,0,0), snormal);
					half  x     = dot(proj, right);
					half  y     = dot(proj, up);
					half2 persp = half2(x,y) * 0.01;
					half4 shadows = tex2D (_CloudMap, i.uv + c + t + persp) * (LdotS * _CloudShadowStrength);
					#endif

                	// Earth component
					half LdotN = saturate(dot(_SunLightDirection, normal));
					half lighting = LdotN + _AmbientLight;
					half4 earth = color * lighting;
					#if WPM_CLOUDSHADOWS_ENABLED
					earth *= 1.0 - shadows;
					#endif

					// Compose
//					half4 rgb = lerp(earth, cloud, _CloudAlpha * cloud.a);
					half4 rgb = earth * (1.0 - (_CloudAlpha * cloud.a)) + cloud * _CloudAlpha;

					// Atmosphere
					rgb = lerp(rgb, _AtmosphereColor * i.scatter.x, _AtmosphereAlpha);

					// Color correction
			  		color.rgb = (color.rgb - 0.5.xxx) * _Contrast + 0.5.xxx;
					color.rgb *= _Brightness;
		  			
					return lerp(color, rgb, _ScenicIntensity);
				}
			
			ENDCG
		}
	}
}