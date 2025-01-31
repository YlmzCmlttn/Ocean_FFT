Shader "CustomShaders/Ocean"
{
    Properties
    {
        
    }
    SubShader
    {
        Tags { 
            "RenderType"= "Opaque" 
            "LightMode" = "ForwardBase"
        }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UnityPBSLighting.cginc"
            #include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float3 normal : TEXCOORD1;
                float3 worldPosition : TEXCOORD2;
            };

            v2f vert (appdata v)
            {
                v2f o;
				o.position = UnityObjectToClipPos(v.vertex);
				o.worldPosition = mul(unity_ObjectToWorld, v.vertex);
				o.normal = normalize(UnityObjectToWorldNormal(v.normal));

                return o;
            }

            float4 frag(v2f i) : SV_TARGET
            {
                // Get the world-space direction of the main directional light (assumed normalized)
                float3 lightDir = _WorldSpaceLightPos0;

                // Compute the view direction (normalized vector from the fragment to the camera)
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPosition);

                // Compute the halfway vector between the light direction and the view direction
                float3 halfwayDir = normalize(lightDir + viewDir);

                // Define an ambient light component (currently black, no ambient contribution)
                float3 ambient = float3(0.0f, 0.0f, 0.0f);

                // Calculate the diffuse light using Lambert's cosine law (clamped dot product)
                float3 diffuse = _LightColor0.rgb * DotClamped(lightDir, normalize(i.normal));

                // Compute the specular reflection using the Blinn-Phong model
                //  - Uses the dot product between the normal and halfway vector
                //  - The power `50.0f` controls shininess (higher value = smaller highlight)
                float specular = _LightColor0.rgb * pow(DotClamped(i.normal, halfwayDir), 50.0f);

                // Combine ambient, diffuse, and specular lighting and return the final color
                // `saturate()` ensures the color stays within the [0,1] range
                return float4(saturate(ambient + diffuse + specular), 1.0f);
            }
            ENDCG
        }
    }
}