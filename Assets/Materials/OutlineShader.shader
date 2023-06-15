Shader "Unlit/OutlineShader"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)

        _StencilComp("Stencil Comparison", Float) = 8
        _Stencil("Stencil ID", Float) = 0
        _StencilOp("Stencil Operation", Float) = 0
        _StencilWriteMask("Stencil Write Mask", Float) = 255
        _StencilReadMask("Stencil Read Mask", Float) = 255

        _ColorMask("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0

        [Toggle(UNITY_UI_ALPHACLIP)] _OutlineEnabled("Is Outline Enabled", Float) = 0
        _OutlineColor0("Outline Color 0", Color) = (1, 0, 0, 1)
        _OutlineColor1("Outline Color 1", Color) = (0, 1, 0, 1)
        _OutlineColorIndex("Outline Color Index", Float) = 0
        _OutlineWidth("Outline Width", Float) = 4
    }
    SubShader
    {
         Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref[_Stencil]
            Comp[_StencilComp]
            Pass[_StencilOp]
            ReadMask[_StencilReadMask]
            WriteMask[_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest[unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask[_ColorMask]

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

#define OUTLINE_TEXCOORD_BOUNDS_CHECK(pos) saturate(saturate(ceil((pos).x - 1.0) + ceil((pos).y - 1.0)) + saturate(ceil(-(pos).x) + ceil(-(pos).y)))
#define OUTLINE_POINT(name, pos) fixed alpha_##name = tex2D(_MainTex, (pos)).a; alpha_##name = lerp(alpha_##name, 0, OUTLINE_TEXCOORD_BOUNDS_CHECK(pos));

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            float _OutlineEnabled;
            fixed4 _OutlineColor0;
            fixed4 _OutlineColor1;
            float _OutlineColorIndex;
            float _OutlineWidth;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                fixed2 distance = fixed2(_MainTex_TexelSize.x, _MainTex_TexelSize.y) * _OutlineWidth;
                OUTLINE_POINT(up, IN.texcoord + fixed2(0, distance.y));
                OUTLINE_POINT(down, IN.texcoord - fixed2(0, distance.y));
                OUTLINE_POINT(right, IN.texcoord + fixed2(distance.x, 0));
                OUTLINE_POINT(left, IN.texcoord - fixed2(distance.x, 0));
                OUTLINE_POINT(upright, IN.texcoord + distance);
                OUTLINE_POINT(upleft, IN.texcoord + fixed2(-distance.x, distance.y));
                OUTLINE_POINT(downleft, IN.texcoord - distance);
                OUTLINE_POINT(downright, IN.texcoord + fixed2(distance.x, -distance.y));
                fixed factor = (alpha_up * alpha_down * alpha_right * alpha_left * alpha_upright * alpha_upleft * alpha_downleft * alpha_downright);
                factor = lerp(1, factor, _OutlineEnabled);

                half4 outlineColor = lerp(_OutlineColor0, _OutlineColor1, _OutlineColorIndex);
                outlineColor.a *= (color.a);
                outlineColor.rgb *= outlineColor.a;

                return lerp(outlineColor, color, factor);
            }

#undef OUTLINE_POINT
#undef OUTLINE_TEXCOORD_BOUNDS_CHECK

        ENDCG
        }
    }
}
