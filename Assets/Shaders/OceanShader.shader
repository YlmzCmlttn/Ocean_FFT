Shader "Custom/OceanShader"
{
    Properties
    {
        _Color ("Main Color", Color) = (0.2, 0.5, 1, 1)
        _Amplitude ("Wave Amplitude", Range(0, 2)) = 0.5
        _Frequency ("Wave Frequency", Range(0, 5)) = 1
        _Speed ("Wave Speed", Range(0, 5)) = 1
        _Direction ("Wave Direction", Vector) = (1, 0.5, 0, 0)
        
        // Second wave parameters
        _Amplitude2 ("Secondary Amplitude", Range(0, 2)) = 0.3
        _Frequency2 ("Secondary Frequency", Range(0, 5)) = 1.5
        _Speed2 ("Secondary Speed", Range(0, 5)) = 0.8
        _Direction2 ("Secondary Direction", Vector) = (0.7, 1, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 wave : TEXCOORD0;
            };

            float4 _Color;
            float _Amplitude, _Frequency, _Speed;
            float2 _Direction;
            float _Amplitude2, _Frequency2, _Speed2;
            float2 _Direction2;

            v2f vert (appdata v)
            {
                v2f o;
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz; //Make it based on world coordinate.
                float time = _Time.y;

                // First wave calculation
                float wave1 = _Amplitude * sin(
                    _Frequency * (worldPos.x * _Direction.x + worldPos.z * _Direction.y) + 
                    time * _Speed
                );

                // Second wave calculation
                float wave2 = _Amplitude2 * sin(
                    _Frequency2 * (worldPos.x * _Direction2.x + worldPos.z * _Direction2.y) + 
                    time * _Speed2
                );

                // Combine waves
                v.vertex.y += wave1 + wave2;

                o.wave.x = wave1;
                o.wave.y = wave2;

                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                
                float waveX = (i.wave.x + _Amplitude) / (2 * _Amplitude);
                float waveY = (i.wave.y + _Amplitude2) / (2 * _Amplitude2);

                float combined = (waveX + waveY) * 0.5;
    
                // Optional: Power curve for visual contrast
                //combined = pow(combined, 2.2);
    
                //return i.wave.x+i.wave.y;

                float foam = saturate(i.wave.x * 2 + i.wave.y * 1.5);
                return _Color * (1 + foam * 0.5);

                

            }
            ENDCG
        }
    }
}
