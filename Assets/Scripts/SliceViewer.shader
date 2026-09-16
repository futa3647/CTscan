Shader "Custom/SliceViewer"
{
    Properties
    {
        // DICOMのTexture3D
        _VolumeTex ("Volume Texture", 3D) = "" {}

        // 表示するZスライス
        _SlicePosition ("Slice Position", Range(0, 1)) = 0
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
            Name "SliceViewer"

            // 両面表示
            Cull Off

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // 頂点情報
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            // Texture3D
            TEXTURE3D(_VolumeTex);
            SAMPLER(sampler_VolumeTex);

            // Materialの値
            CBUFFER_START(UnityPerMaterial)
                float _SlicePosition;
            CBUFFER_END

            // 頂点シェーダー
            Varyings vert(Attributes input)
            {
                Varyings output;

                output.positionHCS =
                    TransformObjectToHClip(
                        input.positionOS.xyz);

                output.uv = input.uv;

                return output;
            }

            // ピクセルシェーダー
            float4 frag(Varyings input) : SV_Target
            {
                // PlaneのUVをTexture3DのXYとして使用
                float3 uvw;

                uvw.x = input.uv.x;
                uvw.y = input.uv.y;
                uvw.z = _SlicePosition;

                // Texture3Dから画素を取得
                float density =
                    SAMPLE_TEXTURE3D(
                        _VolumeTex,
                        sampler_VolumeTex,
                        uvw).r;

                // 白黒で表示
                return float4(
                    density,
                    density,
                    density,
                    1.0);
            }

            ENDHLSL
        }
    }
}