Shader "Custom/SliceViewer"
{
    Properties
    {
        _VolumeTex ("Volume Texture", 3D) = "" {}

        _StartSlice ("Start Slice", Range(0, 1)) = 0
        _EndSlice ("End Slice", Range(0, 1)) = 1
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
            Cull Off

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

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

            TEXTURE3D(_VolumeTex);
            SAMPLER(sampler_VolumeTex);

            CBUFFER_START(UnityPerMaterial)

                float _StartSlice;
                float _EndSlice;

            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;

                output.positionHCS =
                    TransformObjectToHClip(
                        input.positionOS.xyz
                    );

                output.uv = input.uv;

                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float3 uvw;

                uvw.x = input.uv.x;
                uvw.y = input.uv.y;

                // 範囲の中央を取得
                float slicePosition =
                    (_StartSlice + _EndSlice) * 0.5;

                uvw.z = slicePosition;

                float density =
                    SAMPLE_TEXTURE3D(
                        _VolumeTex,
                        sampler_VolumeTex,
                        uvw
                    ).r;

                return float4(
                    density,
                    density,
                    density,
                    1.0
                );
            }

            ENDHLSL
        }
    }
}