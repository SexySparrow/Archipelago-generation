Shader "Custom/TerrainShader"
{
    Properties
    {
       testTexture("Texture", 2D) = "white"{}
       testScale("Scale", float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        const static int maxColorCount = 8;
        const static float epsilon = 0.0001;

        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
        };
        float minHeight;
        float maxHeight;
        int ColorCount;
        float3 Colors[maxColorCount];
        float Heights[maxColorCount];
        float baseBlends[maxColorCount];
        float baseColorStrength[maxColorCount];
        float baseTextureScales[maxColorCount];

        sampler2D testTexture;
        float testScale;

        UNITY_DECLARE_TEX2DARRAY(baseTextures);

        float InverseLerp (float a, float b, float x)
        {
            return saturate((x - a) / (b - a));
        }

        float3 triplanar (float3 worldPos, float scale, float3 blendAxes, int textureIndex)
        {
            float3 scaleWorldPos = worldPos * scale;

            float3 xProj = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaleWorldPos.y, scaleWorldPos.z, textureIndex)) * blendAxes.x;
            float3 yProj = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaleWorldPos.x, scaleWorldPos.z, textureIndex)) * blendAxes.y;
            float3 zProj = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaleWorldPos.x, scaleWorldPos.y, textureIndex)) * blendAxes.z;
            return xProj + yProj + zProj;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float heightPercent = InverseLerp(minHeight, maxHeight, IN.worldPos.y); 
            float3 blendAxes = abs(IN.worldNormal);
            blendAxes /= blendAxes.x + blendAxes.y + blendAxes.z;
            for (int i = 0; i < ColorCount; i++)
            {
                float drawStrength = InverseLerp(-baseBlends[i]/2 - epsilon, baseBlends[i]/2, heightPercent - Heights[i]);
                float3 baseColor = Colors[i] * baseColorStrength[i];
                float3 textureColor = triplanar(IN.worldPos, baseTextureScales[i], blendAxes, i) * (1 - baseColorStrength[i]);

                o.Albedo = o.Albedo * (1 - drawStrength) + (baseColor + textureColor) * drawStrength;
            }


        }
        ENDCG
    }
    FallBack "Diffuse"
}
