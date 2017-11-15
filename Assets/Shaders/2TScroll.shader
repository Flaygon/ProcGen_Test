// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Tanks/2TScroll"
{
	Properties
	{
		_TintColor("Tint Color", Color) = (0.0, 0.0, 0.0, 0.0)
		_ScrollingTex1("Texture 1 (TC: RGBA) (UV: 1)", 2D) = "black" {}
		_ScrollTex1ScrollX("Texture 1 Scroll X", Float) = 0.0
		_ScrollTex1ScrollY("Texture 1 Scroll Y", Float) = 0.0

		_ScrollingTex2("Texture 2 (TC: RGBA) (UV: 1)", 2D) = "black" {}
		_ScrollTex2ScrollX("Texture 2 Scroll X", Float) = 0.0
		_ScrollTex2ScrollY("Texture 2 Scroll Y", Float) = 0.0
	}

	SubShader
	{
		Tags
		{
			"RenderType" = "Transparent"
			"Queue" = "Transparent"
		}
		LOD 100

		Pass
		{
			Cull Off
			ZWrite Off
			Lighting Off
			Blend SrcAlpha OneMinusSrcAlpha 
			ZTest LEqual

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 position : POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
			};

			struct v2f
			{
				float4 position : SV_POSITION;
				float4 uv : TEXCOORD0;
				float4 color : COLOR;
				UNITY_FOG_COORDS(1)
			};

			sampler2D _ScrollingTex1, _ScrollingTex2;
			float4 _ScrollingTex1_ST, _ScrollingTex2_ST;
			fixed4 _TintColor;
			float _ScrollTex1ScrollX, _ScrollTex1ScrollY, _ScrollTex2ScrollX, _ScrollTex2ScrollY;

			v2f vert(appdata input)
			{
				v2f output;

				output.position = UnityObjectToClipPos(input.position);

				output.uv.xy = TRANSFORM_TEX(input.uv, _ScrollingTex1) + half2(_ScrollTex1ScrollX, _ScrollTex1ScrollY) * _Time.yy;
				output.uv.zw = TRANSFORM_TEX(input.uv, _ScrollingTex2) + half2(_ScrollTex2ScrollX, _ScrollTex2ScrollY) * _Time.yy;

				output.color = input.color;

				UNITY_TRANSFER_FOG(output, output.position);

				return output;
			}

			fixed4 frag(v2f input) : SV_Target
			{
				fixed4 firstTexture = tex2D(_ScrollingTex1, input.uv.xy);
				fixed4 secondTexture = tex2D(_ScrollingTex2, input.uv.zw);

				fixed4 finalColor = firstTexture + secondTexture;
				finalColor.a *= input.color.a;

				finalColor *= _TintColor;

				UNITY_APPLY_FOG(input.fogCoord, finalColor);

				return finalColor;
			}
			ENDCG
		}
	}
}