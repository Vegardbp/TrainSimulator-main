Shader "Lit/GrassShader"
{
    Properties
    {
        _color1 ("Color1", Color) = (0.5, 1, 0.5, 1)
        _color2 ("Color2", Color) = (0.5, 1, 0.5, 1)
        _mainTex ("Base Texture", 2D) = "white" {}
        _normalTex ("Normal Map", 2D) = "bump" {}
        _noiseTex ("Noise Texture", 2D) = "white" {}
        _scale ("Scale", Vector) = (1,1,1,0)
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5
        _colorNoiseScale ("ColorNoiseScale", Float) = 0.1
        _windNoiseScale ("WindNoiseScale", Float) = 0.1
        _windMagnitude ("WindMagnitude", Float) = 0.1
        _windRate ("WindRate", Float) = 0.1
        _MinHeight ("Min Height", Float) = -1
        _MaxHeight ("Max Height", Float) = 1
        _normalStrength ("Normal Strength", Range(0, 32)) = 0.5
        _normalVerticality ("Normal Verticality", Range(0, 0.5)) = 0.2
        _AAScale ("Ambient Occlusion Scale", Range(0.5, 3)) = 1.5
    }
    SubShader
    {
        Tags {
            "RenderType" = "Geometry"
            "Queue" = "Geometry"
            "DisableBatching" = "True" 
        }

        Cull Off

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float colorBlend : TEXCOORD2;
                float3 tangent : TEXCOORD3;
                float3 bitangent : TEXCOORD4;
                float3 normal : TEXCOORD5;
                float2 texOffset : TEXCOORD6;
            };

            struct Grass{
                float4 pos;
                float4 rot;
            };

            StructuredBuffer<Grass> _GrassBuffer;
            StructuredBuffer<int> _highLODGrassIndicies;
            sampler2D _noiseTex;
            sampler2D _mainTex;
            sampler2D _normalTex;
            float4 _color1;
            float4 _color2;
            float4 _scale;
            float _Cutoff;
            float _colorNoiseScale;
            float _windRate;
            float _windMagnitude;
            float _windNoiseScale;
            float _MinHeight;
            float _MaxHeight;
            float _normalStrength;
            float _normalVerticality;
            float _AAScale;

            float3 rotate_vector(float4 q, float3 v)
            {
                return v + 2.0 * cross(q.xyz, cross(q.xyz, v) + q.w * v);
            }

            v2f vert (appdata v, uint instanceID : SV_InstanceID)
            {
                v2f o;

                uint grassID = _highLODGrassIndicies[instanceID];
                Grass grassData = _GrassBuffer[grassID];
                float3 worldPos = grassData.pos.xyz;
                float4 rotation = grassData.rot;

                // Transform vertex position
                v.vertex.x *= _scale.x;
                v.vertex.y *= _scale.y;
                v.vertex.z *= _scale.z;
                float scaledHeight = (v.vertex.y - _MinHeight) / (_MaxHeight - _MinHeight);
                scaledHeight *= scaledHeight;
                v.vertex.xyz = rotate_vector(rotation, v.vertex.xyz);
                v.vertex.xyz += worldPos;

                // Apply wind
                float2 samplePos = float2(worldPos.xz) * 0.001 * _windNoiseScale + _Time.xy * _windRate;
                float2 sample = tex2Dlod(_noiseTex, float4(samplePos, 0, -1)).xy;
                float2 windOffset = (sample.xy - float2(0.2, 0.2)) * _windMagnitude * scaledHeight;
                v.vertex.xz += windOffset;
                v.vertex.y -= pow(length(windOffset),2);

                o.worldPos = v.vertex.xyz;
                o.pos = TransformWorldToHClip(o.worldPos);
                o.uv = v.uv;

                // Transform the mesh normal, tangent, and compute bitangent
                float3 worldNormal = rotate_vector(rotation,v.normal.xyz);
                float3 worldTangent = rotate_vector(rotation,v.tangent.xyz);
                float3 worldBitangent = cross(worldNormal, worldTangent) * v.tangent.w;

                o.normal = worldNormal;
                o.tangent = worldTangent;
                o.bitangent = worldBitangent;

                // Color blend
                float2 colorSamplePos = o.worldPos.xz * _colorNoiseScale * 0.001;
                o.colorBlend = tex2Dlod(_noiseTex, float4(colorSamplePos, 0, 0)).r;

                int grassType = grassID % 4;
                float offsetX = (grassType % (uint)2) * 0.5; // 0 or 0.5
                float offsetY = (grassType / (uint)2) * 0.5; // 0 or 0.5
                o.texOffset = float2(offsetX, offsetY);

                return o;
            }

            half4 frag (v2f i, bool isFrontFace : SV_IsFrontFace) : SV_Target
            {
                // Adjust UVs to sample from the correct quadrant
                float2 adjustedUV = i.uv * 0.5 + i.texOffset;
                half4 col = tex2D(_mainTex, adjustedUV);
                clip(col.a - _Cutoff);

                // Sample normal map with adjusted UVs
                float4 normalTex = tex2D(_normalTex, adjustedUV);

                float3 norm = UnpackNormal(normalTex);

                // Apply normal map
                float3 tangentNormal = normalize(lerp(float3(0, 0, 1), norm, _normalStrength));
                // No need to flip tangentNormal.z manually, as the TBN matrix handles orientation
                float3x3 TBN = float3x3(i.tangent, i.bitangent, i.normal);
                float3 worldNormal = normalize(mul(TBN, tangentNormal));
                if(!isFrontFace)
                    worldNormal -= i.normal*2;
                worldNormal = normalize(lerp(float3(0,1,0), worldNormal, _normalVerticality));

                float scaledHeight = i.uv.y;

                // Lighting calculations
                float4 shadowCoord = TransformWorldToShadowCoord(i.worldPos);
                Light mainLight = GetMainLight(shadowCoord);
                float3 lightDir = mainLight.direction;
                float diffuse = max(0, dot(worldNormal, lightDir));
                float3 lighting = diffuse * mainLight.color * mainLight.shadowAttenuation;

                float3 ambient = SampleSH(float3(0,1,0));
                float2 samplePos = float2(i.worldPos.xz) * 0.001 * _colorNoiseScale;
                float blend = saturate(i.colorBlend.r * 2);
                return length(col) * lerp(_color1, _color2, blend) * float4((lighting + ambient).rgb * saturate(pow(saturate(scaledHeight),_AAScale)), 1);
            }
            ENDHLSL
        }
    }
}