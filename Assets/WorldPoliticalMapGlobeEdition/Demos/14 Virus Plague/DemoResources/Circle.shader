Shader "World Political Map/Demos/CirclePaint"
{
	Properties
	{
		_MaskTex("Mask Tex", 2D) = "white" {}
		_Color ("Color (RGBA)", Color) = (0,0,1,0.5)
		_CenterAndRadius("Center (XY) Radius (Z)", Vector) = (0.5,0.5,0.1)
		_UVRect ("UV Rect", Vector) = (0,0,1,1)
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" }

		Blend SrcAlpha One
		ZWrite Off
		Cull Off
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MaskTex;
			float3 _CenterAndRadius;
			fixed4 _Color;
			float4 _UVRect;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = lerp(_UVRect.xy, _UVRect.xy + _UVRect.zw, v.uv);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 c = tex2D(_MaskTex, i.uv);
				#if !UNITY_COLORSPACE_GAMMA
				c.rgb = LinearToGammaSpace(c.rgb);
				#endif
				clip(0.1 - c.b);

				float dx = i.uv.x - _CenterAndRadius.x;
				dx *= 1.6;
				float dy = i.uv.y - _CenterAndRadius.y;
				float sqrDist = dot(float2(dx, dy), float2(dx, dy));
				return _Color * saturate(_CenterAndRadius.z / sqrDist);
			}
			ENDCG
		}
	}
}
