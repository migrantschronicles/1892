Shader "World Political Map/Unlit Earth Glow 2" 
{
	SubShader 
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		//Tags { "RenderType"="Opaque" }
    	Pass 
    	{
    		
			Blend SrcAlpha OneMinusSrcAlpha 
			ZWrite Off
			Cull Front
		
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			
			uniform float3 _SunLightDirection;		// The direction vector to the light source
			uniform float3 v3InvWavelength; // 1 / pow(wavelength, 4) for the red, green, and blue channels
			uniform float fOuterRadius;		// The outer (atmosphere) radius
			uniform float fOuterRadius2;	// fOuterRadius^2
			uniform float fInnerRadius;		// The inner (planetary) radius
			uniform float fInnerRadius2;	// fInnerRadius^2
			uniform float fKrESun;			// Kr * ESun
			uniform float fKmESun;			// Km * ESun
			uniform float fKr4PI;			// Kr * 4 * PI
			uniform float fKm4PI;			// Km * 4 * PI
			uniform float fScale;			// 1 / (fOuterRadius - fInnerRadius)
			uniform float fScaleDepth;		// The scale depth (i.e. the altitude at which the atmosphere's average density is found)
			uniform float fScaleOverScaleDepth;	// fScale / fScaleDepth
			uniform float4 g;				// The Mie phase asymmetry factor & The Mie phase asymmetry factor squared
		    uniform float _GlowIntensity;
            uniform float _Brightness;
            
			struct v2f 
			{
    			float4 pos : SV_POSITION;
    			float3 c0 : COLOR0;
    			float3 c1 : COLOR1;
    			float3 t0 : TEXCOORD0;
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
			};
			
			float scale(float fCos)
			{
				float x = 1.0 - fCos;
				return 0.25 * exp(-0.00287 + x*(0.459 + x*(3.83 + x*(-6.80 + x*5.25))));
			}

            float3 v3CameraPos, center;
            float fCameraHeight;

			v2f vertSky(appdata_base v)
			{
				float fCameraHeight2 = fCameraHeight*fCameraHeight;			// fCameraHeight^2
			
				// Get the ray from the camera to the vertex and its length (which is the far point of the ray passing through the atmosphere)
				float3 v3Pos = mul(unity_ObjectToWorld, v.vertex).xyz - center;
				float3 v3Ray = v3Pos - v3CameraPos;
				float fFar = length(v3Ray);
				v3Ray /= fFar;
				
				// Calculate the closest intersection of the ray with the outer atmosphere (which is the near point of the ray passing through the atmosphere)
				float B = 2.0 * (dot(v3CameraPos, v3Ray));
				float C = fCameraHeight2 - fOuterRadius2;
				float fDet = max(0.0, B*B - 4.0 * C);
				float fNear = 0.5 * (-B - sqrt(fDet));
				
				// Calculate the ray's start and end positions in the atmosphere, then calculate its scattering offset
				float3 v3Start = v3CameraPos + v3Ray * fNear;
				fFar -= fNear;
				float fStartAngle = dot(v3Ray, v3Start) / fOuterRadius;
				float fStartDepth = exp(-1.0/fScaleDepth);
				float fStartOffset = fStartDepth*scale(fStartAngle);
				
				const float fSamples = 2.0;
			
				// Initialize the scattering loop variables
				float fSampleLength = fFar / fSamples;
				float fScaledLength = fSampleLength * fScale;
				float3 v3SampleRay = v3Ray * fSampleLength;
				float3 v3SamplePoint = v3Start + v3SampleRay * 0.5;
			
				// Now loop through the sample rays
				float3 v3FrontColor = float3(0.0, 0.0, 0.0);
				for(int i=int(fSamples); i>0; i--)
				{
					float fHeight = length(v3SamplePoint);
					float fDepth = exp(fScaleOverScaleDepth * (fInnerRadius - fHeight));
					float fLightAngle = dot(_SunLightDirection, v3SamplePoint) / fHeight;
					float fCameraAngle = dot(v3Ray, v3SamplePoint) / fHeight;
					float fScatter = (fStartOffset + fDepth*(scale(fLightAngle) - scale(fCameraAngle)));
					float3 v3Attenuate = exp(-fScatter * (v3InvWavelength * fKr4PI + fKm4PI));
					v3FrontColor += max(0, v3Attenuate * (fDepth * fScaledLength));
					v3SamplePoint += v3SampleRay;
				}
			
    			v2f o;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_OUTPUT(v2f, o);
            UNITY_TRANSFER_INSTANCE_ID(v, o);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    			o.pos = UnityObjectToClipPos(v.vertex);
				o.c0 = v3FrontColor * (v3InvWavelength * fKrESun);
				o.c1 = v3FrontColor * fKmESun;
				o.t0 = v3CameraPos - v3Pos;
    			return o;
			}

		v2f vertAtmos(appdata_base v)
			{
				// Get the ray from the camera to the vertex and its length (which is the far point of the ray passing through the atmosphere)
				float3 v3Pos = mul(unity_ObjectToWorld, v.vertex).xyz - center;
				float3 v3Ray = v3Pos - v3CameraPos;
				float fFar = length(v3Ray);
				v3Ray /= fFar;

				// Calculate the ray's start and end positions in the atmosphere, then calculate its scattering offset
				float3 v3Start = v3CameraPos; // + v3Ray * fNear;
		        float fStartDepth = exp(fScaleOverScaleDepth * (fInnerRadius - fCameraHeight));
				float fStartAngle = dot(v3Ray, v3Start) / fCameraHeight;
				float fStartOffset = fStartDepth * scale(fStartAngle);
				
				const float fSamples = 2.0;
			
				// Initialize the scattering loop variables
				float fSampleLength = fFar / fSamples;
				float fScaledLength = fSampleLength * fScale;
				float3 v3SampleRay = v3Ray * fSampleLength;
				float3 v3SamplePoint = v3Start + v3SampleRay * 0.5;
			
				// Now loop through the sample rays
				float3 v3FrontColor = float3(0.0, 0.0, 0.0);
				for(int i=int(fSamples); i>0; i--)
				{
					float fHeight = length(v3SamplePoint);
                    float fDepth = exp(fScaleOverScaleDepth * (fInnerRadius - fHeight));
					float fLightAngle = dot(_SunLightDirection, v3SamplePoint) / fHeight;
					float fCameraAngle = dot(v3Ray, v3SamplePoint) / fHeight;
					float fScatter = (fStartOffset + fDepth*(scale(fLightAngle) - scale(fCameraAngle)));
					float3 v3Attenuate = exp(-fScatter * (v3InvWavelength * fKr4PI + fKm4PI));
					v3FrontColor += max(0, v3Attenuate * (fDepth * fScaledLength));
					v3SamplePoint += v3SampleRay;
				}
			
    			v2f o;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_OUTPUT(v2f, o);
            UNITY_TRANSFER_INSTANCE_ID(v, o);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    			o.pos = UnityObjectToClipPos(v.vertex);
				o.c0 = v3FrontColor * (v3InvWavelength * fKrESun);
				o.c1 = v3FrontColor * fKmESun;
				o.t0 = v3CameraPos - v3Pos;
    			return o;
			}
			
			// Mie phase function
			float getMiePhase(float fCos, float fCos2, float2 g)
			{
				return 1.5 * ((1.0 - g.y) / (2.0 + g.y)) * (1.0 + fCos2) / pow(1.0 + g.y - 2.0 * g.x * fCos, 1.5);
			}

			// Rayleigh phase function
			float getRayleighPhase(float fCos2)
			{
				return 0.75 + 0.75*fCos2;
			}


            v2f vert(appdata_base v) {
				center = float3(unity_ObjectToWorld[0][3], unity_ObjectToWorld[1][3], unity_ObjectToWorld[2][3]);
				v3CameraPos = _WorldSpaceCameraPos - center;	// The camera's current position
				fCameraHeight = length(v3CameraPos);					// The camera's current height
                if (fCameraHeight >= fOuterRadius) {
                    return vertSky(v);
                } else {
                    return vertAtmos(v);
                }
            }


			half4 frag(v2f IN) : SV_Target {
            UNITY_SETUP_INSTANCE_ID(IN);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN); 
				float fCos = dot(_SunLightDirection, IN.t0) / length(IN.t0);
				float fCos2 = fCos*fCos;
				float3 col = getRayleighPhase(fCos2) * IN.c0 + getMiePhase(fCos, fCos2, g) * IN.c1;
				col = 1.0 - exp(col * -_Brightness);
                #if !UNITY_COLORSPACE_GAMMA
                col.rgb = GammaToLinearSpace(col.rgb);
                #endif
                return saturate(half4(col, Luminance(col) * _GlowIntensity )) ;
				
			}
			
			ENDCG

    	}
	}
	
//	FallBack Off
}
