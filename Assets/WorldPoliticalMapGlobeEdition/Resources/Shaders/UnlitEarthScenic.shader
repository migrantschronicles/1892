Shader "World Political Map/Unlit Earth Scenic" {

	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
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
		_Brightness("Brightness", Range(1,3)) = 1.25
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
				
				sampler2D _MainTex;
				sampler2D _NormalMap;
				sampler2D _CloudMap;
				float _BumpAmount;
				float _CloudSpeed;
				float _CloudAlpha;
				float _CloudShadowStrength;
				float _CloudElevation;
				float3 _SunLightDirection;
				float4 _AtmosphereColor;
				float _AtmosphereAlpha;
				float _AtmosphereFallOff;
				float _ScenicIntensity;
				float _Brightness;
				float _Contrast;
				float _AmbientLight;				
				float _SpecularPower;
				float _SpecularIntensity;
				
				struct v2f {
					float4 pos : SV_POSITION;
					float2 uv : TEXCOORD0;
					float3 viewDir: TEXCOORD1;
					float3 normal: TEXCOORD2;
					float2 scatter: TEXCOORD3;
					#if WPM_BUMPMAP_ENABLED
					float3 tspace0 : TEXCOORD4; // tangent.x, bitangent.x, normal.x
                	float3 tspace1 : TEXCOORD5; // tangent.y, bitangent.y, normal.y
                	float3 tspace2 : TEXCOORD6; // tangent.z, bitangent.z, normal.z
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
					float3 wNormal		= UnityObjectToWorldNormal(v.normal);
					o.normal			= wNormal;
					o.viewDir 			= normalize(WorldSpaceViewDir(v.vertex));
					// compute scatter vectors
					float d 				= dot(-wNormal, _SunLightDirection);
					o.scatter 			= float2(1.0 - saturate(d * _AtmosphereFallOff),  0);

					// normal stuff
					#if WPM_BUMPMAP_ENABLED
	                float3 wTangent = UnityObjectToWorldDir(v.tangent.xyz);
        	        float tangentSign = v.tangent.w * unity_WorldTransformParams.w;
            	    float3 wBitangent = cross(wNormal, wTangent) * tangentSign;
                	// output the tangent space matrix
	                o.tspace0 = float3(wTangent.x, wBitangent.x, wNormal.x);
    	            o.tspace1 = float3(wTangent.y, wBitangent.y, wNormal.y);
        	        o.tspace2 = float3(wTangent.z, wBitangent.z, wNormal.z);
        	        #endif
					return o;
				 }


				 float3 projectOnPlane(float3 v, float3 n) {
				 	return v - dot(v, n) * n;
				 }
				
				float4 frag (v2f i) : SV_Target {
                    UNITY_SETUP_INSTANCE_ID(i);
                    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); 

					// compute Earth pixel color
					float4 color = tex2D (_MainTex, i.uv);

					// sphere normal (without bump-map)
					float3 snormal = normalize(i.normal);


				// specular reflection
				#if WPM_SPECULAR_ENABLED
				float3 worldRefl = reflect(_SunLightDirection, snormal);
        	    float spec = pow(max(0.0, dot(-i.viewDir, worldRefl)), _SpecularPower);
				color.rgb += (spec * color.a * _SpecularIntensity);
				#endif

					// transform normal from tangent to world space
					#if WPM_BUMPMAP_ENABLED
					float3 tnormal = UnpackNormal(tex2D(_NormalMap, i.uv)); 
                	float3 worldNormal;
                	worldNormal.x = dot(i.tspace0, tnormal);
                	worldNormal.y = dot(i.tspace1, tnormal);
                	worldNormal.z = dot(i.tspace2, tnormal);
                	float3 normal = normalize(lerp(snormal, worldNormal, _BumpAmount));
                	#else
                	float3 normal = snormal;
                	#endif

                	// Clouds
                	float  LdotS = saturate(dot(_SunLightDirection, snormal));
					float2 t = float2(_Time[0] * _CloudSpeed, 0);
					float2 disp = -i.viewDir * _CloudElevation;
					float4 cloud = tex2D (_CloudMap, i.uv + t - disp);
					cloud.rgb *= (LdotS + _AmbientLight);

					// Cloud shadows
					#if WPM_CLOUDSHADOWS_ENABLED
					const float2 c = float2(0.998,0);
					float3 proj  = projectOnPlane(_SunLightDirection, snormal);
					float3 up    = projectOnPlane(float3(0,1,0), snormal);
					float3 right = projectOnPlane(float3(1,0,0), snormal);
					float  x     = dot(proj, right);
					float  y     = dot(proj, up);
					float2 persp = float2(x,y) * 0.01;
					float4 shadows = tex2D (_CloudMap, i.uv + c + t + persp) * (LdotS * _CloudShadowStrength);
					#endif

                	// Earth component
					float LdotN = saturate(dot(_SunLightDirection, normal));
					float lighting = LdotN + _AmbientLight;
					float4 earth = color * lighting;
					#if WPM_CLOUDSHADOWS_ENABLED
					earth *= 1.0 - shadows;
					#endif

					// Compose
					float4 rgb = earth * (1.0 - (_CloudAlpha * cloud.a)) + cloud * _CloudAlpha;

					// Atmosphere
					rgb = lerp(rgb, _AtmosphereColor * i.scatter.x, _AtmosphereAlpha);

					// Color correction
		  			rgb = (rgb - 0.5.xxxx) * _Contrast + 0.5.xxxx;
  			   		rgb *= _Brightness;
                    
					return lerp(color, rgb, _ScenicIntensity);
				}
			
			ENDCG
		}
	}
}