Shader "World Political Map/Skybox/6SidedWithSun" {
Properties {
    _Tint ("Sky Tint", Color) = (.5, .5, .5, 1)
    _SunColor ("Sun Tint", Color) = (1,0.9568,0.8392)
    _SunSize ("Sun Size", Range(0,1)) = 0.15
    _SunFlareSize ("Sun Flare Size", Float) = 5
    
    [Gamma] _Exposure ("Exposure", Range(0, 8)) = 1.0
    _Rotation ("Rotation", Range(0, 360)) = 0

	[NoScaleOffset] _SunFlareTex ("Sun Flare Tex", 2D) = "black" {}

    [NoScaleOffset] _FrontTex ("Front [+Z]   (HDR)", 2D) = "grey" {}
    [NoScaleOffset] _BackTex ("Back [-Z]   (HDR)", 2D) = "grey" {}
    [NoScaleOffset] _LeftTex ("Left [+X]   (HDR)", 2D) = "grey" {}
    [NoScaleOffset] _RightTex ("Right [-X]   (HDR)", 2D) = "grey" {}
    [NoScaleOffset] _UpTex ("Up [+Y]   (HDR)", 2D) = "grey" {}
    [NoScaleOffset] _DownTex ("Down [-Y]   (HDR)", 2D) = "grey" {}
}

SubShader {
    Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
    Cull Off ZWrite Off

    CGINCLUDE
    #include "UnityCG.cginc"

    half _Exposure;
    float _Rotation;
    float _SunFlareSize;
	half3 _Tint;
	half _SunSize;
	half3 _SunColor;
	sampler2D _SunFlareTex;

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
	    sunColor += max(0, sunFlare - 0.007);

		return sunColor;
    }


    half4 skybox_frag (v2f i, sampler2D smp, half4 smpDecode)
    {
        half4 tex = tex2D (smp, i.uv);
        half3 color = DecodeHDR (tex, smpDecode);
        color *= _Tint;
        color += addSun(i);
		color *= _Exposure;

		// gamma
		#if defined(UNITY_COLORSPACE_GAMMA)
			color = sqrt(color);
		#endif
        return half4(color, 1);
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
        half4 frag (v2f i) : SV_Target { return skybox_frag(i,_BackTex, _BackTex_HDR); }
        ENDCG
    }
    Pass{
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma target 2.0
        sampler2D _LeftTex;
        half4 _LeftTex_HDR;
        half4 frag (v2f i) : SV_Target { return skybox_frag(i,_LeftTex, _LeftTex_HDR); }
        ENDCG
    }
    Pass{
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma target 2.0
        sampler2D _RightTex;
        half4 _RightTex_HDR;
        half4 frag (v2f i) : SV_Target { return skybox_frag(i,_RightTex, _RightTex_HDR); }
        ENDCG
    }
    Pass{
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma target 2.0
        sampler2D _UpTex;
        half4 _UpTex_HDR;
        half4 frag (v2f i) : SV_Target { return skybox_frag(i,_UpTex, _UpTex_HDR); }
        ENDCG
    }
    Pass{
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma target 2.0
        sampler2D _DownTex;
        half4 _DownTex_HDR;
        half4 frag (v2f i) : SV_Target { return skybox_frag(i,_DownTex, _DownTex_HDR); }
        ENDCG
    }
}
}
