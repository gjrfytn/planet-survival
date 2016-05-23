Shader "Custom/TransparentBlur"
{
	Properties
	{
		[HideInInspector]
		_MainTex ("Texture", 2D) = "white" {}
		_Size ("Size", Range(0, 20)) = 1
		[Toggle] _IgnoreTransparent("IgnoreFullyTransparent", Int) = 0
	}

	SubShader
	{
		Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }

		ZWrite Off

		GrabPass{ "_SharedGrabTexture1" }

		//Tags { "LightMode" = "Always" } ?

		Pass
		{
			CGPROGRAM
			#pragma vertex vertShader
			#pragma fragment pixShader
			#pragma fragmentoption ARB_precision_hint_fastest

			#include "UnityCG.cginc"

			struct VertexIn
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct VertexOut
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 grabUV : TEXCOORD1;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _SharedGrabTexture1;
			float4 _SharedGrabTexture1_TexelSize;
            float _Size;
            bool _IgnoreTransparent;

			VertexOut vertShader (VertexIn vIn)
			{
				VertexOut vOut;
				vOut.vertex = mul(UNITY_MATRIX_MVP, vIn.vertex);
				#if UNITY_UV_STARTS_AT_TOP
                    float scale = -1.0;
                #else
                    float scale = 1.0;
                #endif
				vOut.grabUV=(float2(vOut.vertex.x, vOut.vertex.y*scale) + vOut.vertex.w) * 0.5;
				vOut.uv = TRANSFORM_TEX(vIn.uv, _MainTex);
				return vOut;
			}
			
			fixed4 pixShader (VertexOut vOut) : SV_Target
			{
				fixed4 backBlur = fixed4(0,0,0,0);

				if(_IgnoreTransparent==0||tex2D(_MainTex,vOut.uv).a!=0)
				{
					#define GRABPIXEL(weight,kernelx) tex2Dproj( _SharedGrabTexture1, UNITY_PROJ_COORD(float4(vOut.grabUV.x + _SharedGrabTexture1_TexelSize.x * kernelx*_Size, vOut.grabUV.y, vOut.vertex.z, vOut.vertex.w))) * weight

                    backBlur += GRABPIXEL(0.05, -4.0);
                    backBlur += GRABPIXEL(0.09, -3.0);
                    backBlur += GRABPIXEL(0.12, -2.0);
                    backBlur += GRABPIXEL(0.15, -1.0);
                    backBlur += GRABPIXEL(0.18,  0.0);
                    backBlur += GRABPIXEL(0.15, +1.0);
                    backBlur += GRABPIXEL(0.12, +2.0);
                  	backBlur += GRABPIXEL(0.09, +3.0);
                   	backBlur += GRABPIXEL(0.05, +4.0);
               	}
                else
                	backBlur=GRABPIXEL(1,  0.0);
                return backBlur;
			}
			ENDCG
		}

		GrabPass{ "_SharedGrabTexture2" }

		Pass
		{
			CGPROGRAM
			#pragma vertex vertShader
			#pragma fragment pixShader
			#pragma fragmentoption ARB_precision_hint_fastest
			
			#include "UnityCG.cginc"

			struct VertexIn
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct VertexOut
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 grabUV : TEXCOORD1;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _SharedGrabTexture2;
			float4 _SharedGrabTexture2_TexelSize;
            float _Size;
            bool _IgnoreTransparent;

			VertexOut vertShader (VertexIn vIn)
			{
				VertexOut vOut;
				vOut.vertex = mul(UNITY_MATRIX_MVP, vIn.vertex);
				#if UNITY_UV_STARTS_AT_TOP
                    float scale = -1.0;
                #else
                    float scale = 1.0;
                #endif
				vOut.grabUV=(float2(vOut.vertex.x, vOut.vertex.y*scale) + vOut.vertex.w) * 0.5;
				vOut.uv = TRANSFORM_TEX(vIn.uv, _MainTex);
				return vOut;
			}
			
			fixed4 pixShader (VertexOut vOut) : SV_Target
			{
				fixed4 backBlur = fixed4(0,0,0,0);
				fixed4 col=tex2D(_MainTex,vOut.uv);
 				if(_IgnoreTransparent==0||col.a!=0)
				{
					#define GRABPIXEL(weight,kernely) tex2Dproj( _SharedGrabTexture2, UNITY_PROJ_COORD(float4(vOut.grabUV.x, vOut.grabUV.y + _SharedGrabTexture2_TexelSize.y * kernely*_Size, vOut.vertex.z, vOut.vertex.w))) * weight

                    backBlur += GRABPIXEL(0.05, -4.0);
                    backBlur += GRABPIXEL(0.09, -3.0);
                    backBlur += GRABPIXEL(0.12, -2.0);
                    backBlur += GRABPIXEL(0.15, -1.0);
                    backBlur += GRABPIXEL(0.18,  0.0);
                    backBlur += GRABPIXEL(0.15, +1.0);
                    backBlur += GRABPIXEL(0.12, +2.0);
                    backBlur += GRABPIXEL(0.09, +3.0);
                    backBlur += GRABPIXEL(0.05, +4.0);
				}
				else
                    backBlur=GRABPIXEL(1,  0.0);
                return backBlur*(1-col.a)+col*col.a;
			}
			ENDCG
		}
	}
}
