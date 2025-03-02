Shader "Custom/FFT_Ocean" {
		
		Properties {
			[Enum(Off, 0, On, 1)] _ZWrite ("Z Write", Float) = 1
		}

	SubShader {
		Tags {
			"LightMode" = "ForwardBase"
		}

		Pass {

			ZWrite On

			CGPROGRAM

			#pragma vertex vp
			#pragma fragment fp

			#include "UnityPBSLighting.cginc"
            #include "AutoLight.cginc"

			struct VertexData {
				float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct v2f {
				float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
				float3 normal : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
			};



			

			
            Texture2D _HeightTex;
            SamplerState point_repeat_sampler, linear_repeat_sampler;
            sampler2D _NormalTex;

			#define TILE 0.98

			v2f vp(VertexData v) {
				v2f i;
                i.worldPos = mul(unity_ObjectToWorld, v.vertex);
                i.normal = normalize(UnityObjectToWorldNormal(v.normal));				
				float4 heightDisplacement = _HeightTex.SampleLevel(linear_repeat_sampler, v.uv * TILE + 0.01f, 0);

                i.pos = UnityObjectToClipPos(v.vertex + float3(heightDisplacement.g,  heightDisplacement.r, heightDisplacement.b));
				i.uv = v.uv;
				//i.pos = UnityObjectToClipPos(v.vertex);
				
				return i;
			}

			float4 fp(v2f i) : SV_TARGET {
                
				//return _HeightTex.Sample(point_repeat_sampler, i.uv * TILE).r;
				return float4(1.0f,1.0f,1.0f, 1.0f);
			}

			ENDCG
		}
	}
}