Shader "Custom/VolumeRendering"
{
    Properties
    {
        _VolumeTex ("Volume Texture", 3D) = "" {}
        _Density ("Density", Range(0.01, 10)) = 1
        _Threshold ("Threshold", Range(0, 1)) = 0.1
        _StepSize ("Step Size", Range(0.001, 0.02)) = 0.005
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
        }

        Pass
        {
            Name "VolumeRendering"

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Front

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionOS : TEXCOORD0;
            };

            TEXTURE3D(_VolumeTex);
            SAMPLER(sampler_VolumeTex);

            CBUFFER_START(UnityPerMaterial)
                float _Density;
                float _Threshold;
                float _StepSize;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;

                output.positionHCS =
                    TransformObjectToHClip(input.positionOS.xyz);

                output.positionOS = input.positionOS.xyz;

                return output;
            }

            float2 RayBoxIntersection(
                float3 rayOrigin,
                float3 rayDirection)
            {
                float3 boxMin = float3(-0.5, -0.5, -0.5);
                float3 boxMax = float3( 0.5,  0.5,  0.5);

                float3 invDir = 1.0 / rayDirection;

                float3 t0 = (boxMin - rayOrigin) * invDir;
                float3 t1 = (boxMax - rayOrigin) * invDir;

                float3 tMin = min(t0, t1);
                float3 tMax = max(t0, t1);

                float tEnter = max(
                    max(tMin.x, tMin.y),
                    tMin.z);

                float tExit = min(
                    min(tMax.x, tMax.y),
                    tMax.z);

                return float2(tEnter, tExit);
            }

            float4 frag(Varyings input) : SV_Target
            {
                // カメラ位置をオブジェクト空間へ
                float3 cameraOS =
                    TransformWorldToObject(_WorldSpaceCameraPos);

                // カメラからピクセル方向へのレイ
                float3 rayDirection =
                    normalize(input.positionOS - cameraOS);

                float2 hit =
                    RayBoxIntersection(
                        cameraOS,
                        rayDirection);

                if (hit.x > hit.y)
                    discard;

                float tStart = max(hit.x, 0.0);
                float tEnd = hit.y;

                float4 accumulated =
                    float4(0, 0, 0, 0);

                float t = tStart;

                [loop]
                for (int i = 0; i < 512; i++)
                {
                    if (t > tEnd)
                        break;

                    float3 positionOS =
                        cameraOS + rayDirection * t;

                    // -0.5～0.5 → 0～1
                    float3 uv =
                        positionOS + 0.5;

                    float density =
                        SAMPLE_TEXTURE3D(
                            _VolumeTex,
                            sampler_VolumeTex,
                            uv).r;

if (density > _Threshold)
{
    // 濃度を0～1に正規化
    float value = saturate(
        (density - _Threshold) /
        (1.0 - _Threshold)
    );

    // 白黒の濃淡
    // 0 = 黒
    // 1 = 白
    float3 color =
        float3(value, value, value);

    float alpha =
        value *
        _Density *
        _StepSize *
        5.0;

    alpha = saturate(alpha);

    accumulated.rgb +=
        (1.0 - accumulated.a) *
        color *
        alpha;

    accumulated.a +=
        (1.0 - accumulated.a) *
        alpha;

    if (accumulated.a > 0.98)
        break;
}

                    t += _StepSize;
                }

                return accumulated;
            }

            ENDHLSL
        }
    }
}