Shader "Test/Land" {
	Properties {
		_WorldTiling ("RGB World Tiling", Float) = 20

		_SandTex ("Sand (RGB)", 2D) = "white" {}
		_GrassTex ("Grass (RGB)", 2D) = "white" {}
		_RockTex ("Rock (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Lambert// vertex:vert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _SandTex;
		sampler2D _GrassTex;
		sampler2D _RockTex;

		uniform float4 _SandTex_ST;
		uniform float _WorldTiling;

		struct Input {
			float2 sandTexCoord;
			float2 worldCoord;
			float4 vertCol : COLOR;
		};

		void vert(inout appdata_full v, out Input o)
		{
 			UNITY_INITIALIZE_OUTPUT(Input, o); // Required

			o.vertCol = v.color;
			
			o.worldCoord = mul(unity_ObjectToWorld, v.vertex).xz / _WorldTiling;

			o.sandTexCoord = TRANSFORM_TEX(v.texcoord, _SandTex);
		}

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		//UNITY_INSTANCING_CBUFFER_START(Props)
			// put more per-instance properties here
		//UNITY_INSTANCING_CBUFFER_END

		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 sandCol = tex2D (_SandTex, IN.worldCoord) * IN.vertCol.r;
			fixed4 grassCol = tex2D (_GrassTex, IN.worldCoord) * IN.vertCol.g;
			fixed4 rockCol = tex2D (_RockTex, IN.worldCoord) * IN.vertCol.b;

			o.Albedo = sandCol + grassCol + rockCol;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
