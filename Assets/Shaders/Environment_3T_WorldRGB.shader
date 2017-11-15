// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Test/Environment_3T_WorldRGB"
{
    Properties
	{
    	_WorldTiling ("RGB World Tiling", Float) = 20
    	
    	_Black ("(VC: Black) (TC: RGB) (UV: 1) - Bottom Layer", 2D) = "white" {}
    	
		_RedBlend ("Red weight", Range(-0.99,0.99)) = 0
		_Red ("(VC: Red) (TC: RGBA) (UV: World) - Second Layer", 2D) = "black" {}
		
		_GreenBlend ("Green weight", Range(-0.99,0.99)) = 0
		_Green ("(VC: Green) (TC: RGBA) (UB: World) - Third Layer", 2D) = "black" {}
    }
    
    SubShader
	{
        Tags
		{
			"RenderType"="Opaque"
			"Queue"="Geometry"
		}
		
        CGPROGRAM
        #pragma surface surf Lambert vertex:vert exclude_path:prepass halfasview
        
        uniform half _RedBlend, _GreenBlend;
        uniform sampler2D _Black, _Red, _Green;
		uniform float4 _Black_ST;
        uniform float _WorldTiling;
        
        struct Input
		{
			float2 BlackUV;
            float2 worldCoord;
            float4 vertCol : COLOR;
        };
        
        void vert(inout appdata_full v, out Input o)
		{
 			UNITY_INITIALIZE_OUTPUT(Input, o); // Required

			// GREEN CHANNEL
			float zeroPoint = max(_GreenBlend, 0);
			float maxPoint = 1 + min(_GreenBlend, 0);
			o.vertCol.g = (v.color.g + min(_GreenBlend, 0)) / (maxPoint - zeroPoint);

 			// RED CHANNEL
 			zeroPoint = max(_RedBlend, 0);
 			maxPoint = 1 + min(_RedBlend, 0);
			o.vertCol.r = (v.color.r + min(_RedBlend, 0)) / (maxPoint - zeroPoint);

			o.vertCol.a = v.color.a;
			
			o.worldCoord = mul(unity_ObjectToWorld, v.vertex).xz / _WorldTiling;

			o.BlackUV = TRANSFORM_TEX(v.texcoord, _Black);
		}
 		
        void surf(Input IN, inout SurfaceOutput o)
		{
        	// Make sure the blending is in the 0..1 range
			IN.vertCol.g = saturate(IN.vertCol.g);
			IN.vertCol.r = saturate(IN.vertCol.r);

			fixed4 green = tex2D(_Green, IN.worldCoord);
			fixed4 red = tex2D(_Red, IN.worldCoord);
			fixed4 black = tex2D(_Black, IN.worldCoord);

			fixed greenBlend = saturate(green.a * (IN.vertCol.g));
			fixed redBlend = saturate(red.a * (IN.vertCol.r - greenBlend));
			fixed blackBlend = saturate(1.0 - greenBlend - redBlend);

        	fixed3 finalColor = green.rgb * greenBlend;
        	finalColor += red.rgb * redBlend;
			finalColor += black.rgb * blackBlend;
            
            o.Albedo = saturate(finalColor);
			o.Alpha = 1.0;
        }
        ENDCG
    }
    
    FallBack "Diffuse"
}