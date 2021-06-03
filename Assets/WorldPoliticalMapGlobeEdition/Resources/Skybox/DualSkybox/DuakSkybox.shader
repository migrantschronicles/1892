Shader "World Political Map/Skybox/DualSkybox" {
Properties {
    _SkyboxTint ("Skybox Tint", Color) = (.5, .5, .5, 1)
    _SunColor ("Sun Tint", Color) = (1,0.9568,0.8392)
    _SunSize ("Sun Size", Range(0,1)) = 0.15
    _SunFlareSize ("Sun Flare Size", Float) = 5
    _SpaceToGround("Space To Ground", Range(0,1)) = 1

    [NoScaleOffset] _Environment ("Environment (HDR)", 2D) = "black" {}

    [Gamma] _Exposure ("Exposure", Range(0, 8)) = 1.0
    _Rotation ("Rotation", Range(0, 360)) = 0

	[NoScaleOffset] _SunFlareTex ("Sun Flare Tex", 2D) = "black" {}

    [NoScaleOffset] _FrontTex ("Space Front [+Z]   (HDR)", 2D) = "grey" {}
    [NoScaleOffset] _BackTex ("Space Back [-Z]   (HDR)", 2D) = "grey" {}
    [NoScaleOffset] _LeftTex ("Space Left [+X]   (HDR)", 2D) = "grey" {}
    [NoScaleOffset] _RightTex ("Space Right [-X]   (HDR)", 2D) = "grey" {}
    [NoScaleOffset] _UpTex ("Space Up [+Y]   (HDR)", 2D) = "grey" {}
    [NoScaleOffset] _DownTex ("Space Down [-Y]   (HDR)", 2D) = "grey" {}
}

SubShader {
    Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
    Cull Off ZWrite Off

    CGINCLUDE
    #include "UnityCG.cginc"

    half _Exposure;
    float _Rotation;
    float _SunFlareSize;
	half3 _SkyboxTint;
	half _SunSize;
	half3 _SunColor;
	sampler2D _SunFlareTex;
    half _SpaceToGround;
    sampler2D _Environment;
    half4 _Environment_HDR;
    float4x4 _CameraRot;

    float3 RotateAroundYInDegrees (float3 vertex, float degrees)
    {
        float alpha = degrees * UNITY_PI / 180.0;
        float sina, cosa;
        sincos(alpha, sina, cosa);
        float2x2 m = float2x2(cosa, -sina, sina, cosa);
        return float3(mul(m, vertex.xz), vertex.y).xzy;
    }

    struct appdata_t {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct v2f {
        float4 pos : SV_POSITION;
        float2 uv : TEXCOORD0;
		float4 scrPos : TEXCOORD1;
		float4 lightPos : TEXCOORD2;
        float3 ray: TEXCOORD3;
        UNITY_VERTEX_OUTPUT_STEREO
    };

    v2f vert (appdata_t v)
    {
        v2f o;
        UNITY_SETUP_INSTANCE_ID(v);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
        float3 rotated = RotateAroundYInDegrees(v.vertex, _Rotation);
        o.pos = UnityObjectToClipPos(rotated);
        o.uv = v.uv;
		o.scrPos = o.pos; 
		o.lightPos = UnityObjectToClipPos(_WorldSpaceLightPos0.xyz);
        o.ray = mul((float3x3)_CameraRot, v.vertex.xyz);
        return o;
    }

	inline half3 LinearColor(half3 color) {
		#if defined(UNITY_COLORSPACE_GAMMA)
			// color properties are passed in gamma space, we need to switch to linear
			color *= color;
		#endif
		return color;
    }

    half3 addSun(v2f i) {

		// sun
		float2 scrDist = i.scrPos.xy/i.scrPos.w - i.lightPos.xy/i.lightPos.w;
		scrDist.x *= _ScreenParams.x/_ScreenParams.y;
		half sunDist = length(scrDist);
        half sunIntensity = 1.0 - smoothstep(0.0, _SunSize, sunDist);
		sunIntensity *= sunIntensity;
		half3 sunColor = LinearColor(_SunColor) * sunIntensity;

        half3 sunFlare = tex2D(_SunFlareTex, scrDist.xy * _SunFlareSize + 0.5).rgb;
	    sunColor += sunFlare;

		return sunColor;
    }

        inline float2 ToRadialCoords(float3 coords)
        {
            float3 normalizedCoords = normalize(coords);
            float latitude = acos(normalizedCoords.y);
            float longitude = atan2(normalizedCoords.z, normalizedCoords.x);
            float2 sphereCoords = float2(longitude, latitude) * float2(0.5/UNITY_PI, 1.0/UNITY_PI);
            return float2(0.5,1.0) - sphereCoords;
        }


    half4 skybox_frag (v2f i, sampler2D smpSpace, half4 smpDecode)
    {
        half4 texSpace = tex2D (smpSpace, i.uv);
        texSpace.rgb = DecodeHDR (texSpace, smpDecode);
        texSpace.rgb = max(texSpace.rgb, addSun(i));

        float2 tc = ToRadialCoords(i.ray);
        half4 texEnvironment = tex2D(_Environment, tc);
        texEnvironment.rgb = DecodeHDR (texEnvironment, _Environment_HDR);

        half4 color = lerp(texEnvironment, texSpace, _SpaceToGround);
        color.rgb *= _SkyboxTint * unity_ColorSpaceDouble.rgb;
		color.rgb *= _Exposure;

        return color;
    }

    ENDCG

    Pass {
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma target 2.0
        sampler2D _FrontTex;
        half4 _FrontTex_HDR;
        half4 frag (v2f i) : SV_Target { return skybox_frag(i,_FrontTex, _FrontTex_HDR); }
        ENDCG
    }
    Pass{
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma target 2.0
        sampler2D _BackTex;
        half4 _BackTex_HDR;
        half4 frag (v2f i) : SV_Target { return skybox_frag(i,_BackTex,_BackTex_HDR); }
        ENDCG
    }
    Pass{
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma target 2.0
        sampler2D _LeftTex;
        half4 _LeftTex_HDR;
        half4 frag (v2f i) : SV_Target { return skybox_frag(i,_LeftTex,_LeftTex_HDR); }
        ENDCG
    }
    Pass{
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma target 2.0
        sampler2D _RightTex;
        half4 _RightTex_HDR;
        half4 frag (v2f i) : SV_Target { return skybox_frag(i,_RightTex,_RightTex_HDR); }
        ENDCG
    }
    Pass{
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma target 2.0
        sampler2D _UpTex;
        half4 _UpTex_HDR;
        half4 frag (v2f i) : SV_Target { return skybox_frag(i,_UpTex,_UpTex_HDR); }
        ENDCG
    }
    Pass{
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma target 2.0
        sampler2D _DownTex;
        half4 _DownTex_HDR;
        half4 frag (v2f i) : SV_Target { return skybox_frag(i,_DownTex,_DownTex_HDR); }
        ENDCG
    }
}
}
