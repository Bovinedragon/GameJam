Shader "Custom/Ocean" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_GradientTex ("Gradient (RGB)", 2D) = "white" {}

		_HeightIntensity ("Height Intensity", float) = 1

		_HeightMapA ("Normal Map A", 2D) = "normal" {}
		_HeightAScrollX ("Normal A Scroll X", float) = 1
		_HeightAScrollY ("Normal A Scroll Y", float) = 1
		_HeightAScaleX ("Normal A Scale X", float) = 1
		_HeightAScaleY ("Normal A Scale Y", float) = 1

		_HeightMapB ("Normal Map B", 2D) = "normal" {}
		_HeightBScrollX ("Normal B Scroll X", float) = 1
		_HeightBScrollY ("Normal B Scroll Y", float) = 1
		_HeightBScaleX ("Normal B Scale X", float) = 1
		_HeightBScaleY ("Normal B Scale Y", float) = 1

		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows

		#pragma target 3.0


		struct Input
		{
			float2 uv_MainTex;
		};

		sampler2D _MainTex;
		sampler2D _GradientTex;

		half _HeightIntensity;

		sampler2D _HeightMapA;
		half _HeightAScrollX;
		half _HeightAScrollY;
		half _HeightAScaleX;
		half _HeightAScaleY;

		sampler2D _HeightMapB;
		half _HeightBScrollX;
		half _HeightBScrollY;
		half _HeightBScaleX;
		half _HeightBScaleY;

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		void surf (Input IN, inout SurfaceOutputStandard o)
		{
			// Albedo comes from a texture tinted by color
			fixed4 col = tex2D (_MainTex, IN.uv_MainTex) * _Color;

			float2 uvA = float2((IN.uv_MainTex.x * _HeightAScaleX) + (_Time.x * _HeightAScrollX), (IN.uv_MainTex.y * _HeightAScaleY) + (_Time.y * _HeightAScrollY));
			half4 heightA = tex2D(_HeightMapA, uvA);

			float2 uvB = float2((IN.uv_MainTex.x * _HeightBScaleX) + (_Time.x * _HeightBScrollX), (IN.uv_MainTex.y * _HeightBScaleY) + (_Time.y * _HeightBScrollY));
			half4 heightB = tex2D(_HeightMapB, uvB);

			half height = (heightA * heightB) * _HeightIntensity;
			o.Albedo = col.rgb + height;
			//o.Normal = normalize(normalA + normalB);
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = 1.0;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
