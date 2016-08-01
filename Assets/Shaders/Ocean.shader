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

		_WhiteCapTex ("White Cap", 2D) = "white" {}
//		_FlowSpeed ("Flow Speed", float) 
		_WhiteCapScrollX ("White Cap Scroll X", float) = 1
		_WhiteCapScrollY ("White Cap Scroll Y", float) = 1
		_WhiteCapScaleX ("White Cap Scale X", float) = 1
		_WhiteCapScaleY ("White Cap Scale Y", float) = 1

		_WhiteCapScroll2X ("White Cap Scroll 2 X", float) = 1
		_WhiteCapScroll2Y ("White Cap Scroll 2 Y", float) = 1
		_WhiteCapScale2X ("White Cap Scale 2 X", float) = 1
		_WhiteCapScale2Y ("White Cap Scale 2 Y", float) = 1

		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Standard vertex:vert fullforwardshadows

		#pragma target 4.0

		struct Input
		{
			float2 uv_MainTex;
			float3 localPos;
			float4 color;
			float3 normal;
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

		half _WhiteCapScrollX;
		half _WhiteCapScrollY;
		half _WhiteCapScaleX;
		half _WhiteCapScaleY;

		half _WhiteCapScroll2X;
		half _WhiteCapScroll2Y;
		half _WhiteCapScale2X;
		half _WhiteCapScale2Y;

		sampler2D _WhiteCapTex;
//		half _FlowSpeed;

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		void vert (inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input,o);
			o.localPos = v.vertex.xyz;
			o.color = v.color;
			o.normal = v.normal;
		}

		void surf (Input IN, inout SurfaceOutputStandard o)
		{
			half3 col = tex2D(_MainTex, IN.uv_MainTex).rgb * _Color;

			// flow
//			float2 dir = IN.color.gb * 2 - 1;
//			dir *= _FlowSpeed;
//			float phase0 = frac(_Time.x * 0.5f + 0.5f);
//			float phase1 = frac(_Time.x * 0.5f + 1.0f);
//			half3 wctex0 = tex2D(_WhiteCapTex, IN.uv_MainTex + dir * phase0);
//			half3 wctex1 = tex2D(_WhiteCapTex, IN.uv_MainTex + dir * phase1);
//			float flowLerp = abs((0.5f - phase0) / 0.5f);
//			half3 flowColor = lerp(wctex0, wctex1, flowLerp);
//			col += flowColor;

//			float2 wcspeed = float2(_Time.x * _WhiteCapScrollX * -IN.color.g, _Time.x * _WhiteCapScrollY * -IN.color.g);
			float2 wcspeed = float2(_Time.x * _WhiteCapScrollX, _Time.x * _WhiteCapScrollY);
			float2 wcuv = float2((IN.uv_MainTex.x * _WhiteCapScaleX) + wcspeed.x, (IN.uv_MainTex.y * _WhiteCapScaleY) + wcspeed.y);
			fixed4 whiteCap = tex2D (_WhiteCapTex, wcuv);
//			float2 wcspeed2 = float2(_Time.x * _WhiteCapScroll2X * -IN.color.g, _Time.x * _WhiteCapScroll2Y * -IN.color.b);
			float2 wcspeed2 = float2(_Time.x * _WhiteCapScroll2X, _Time.x * _WhiteCapScroll2Y);
			float2 wcuv2 = float2((IN.uv_MainTex.x * _WhiteCapScale2X) + wcspeed2.x, (IN.uv_MainTex.y * _WhiteCapScale2Y) + wcspeed2.y);
			fixed4 whiteCap2 = tex2D (_WhiteCapTex, wcuv2);
			col = col + (whiteCap * whiteCap2 * ((IN.color.r * .3)+.3) * IN.color.r);

			float2 uvA = float2((IN.uv_MainTex.x * _HeightAScaleX) + (_Time.x * _HeightAScrollX), (IN.uv_MainTex.y * _HeightAScaleY) + (_Time.y * _HeightAScrollY));
			half4 heightA = tex2D(_HeightMapA, uvA);
			float2 uvB = float2((IN.uv_MainTex.x * _HeightBScaleX) + (_Time.x * _HeightBScrollX), (IN.uv_MainTex.y * _HeightBScaleY) + (_Time.y * _HeightBScrollY));
			half4 heightB = tex2D(_HeightMapB, uvB);

			half height = (heightA * heightB) * _HeightIntensity;
			o.Albedo = col.rgb + height;
//			o.Albedo = abs(IN.color);
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = 1.0;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
