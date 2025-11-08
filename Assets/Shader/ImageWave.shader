Shader "Custom/ImageWave"
{
    Properties
    {
        [MainTexture] _BaseMap ("Texture", 2D) = "white" {}
        [MainColor]   _BaseColor ("Tint Color", Color) = (1,1,1,1)

        _Amplitude ("Wave Amplitude", Range(0, 0.1)) = 0.03
        _Frequency ("Wave Frequency", Range(0, 50))  = 10
        _Speed     ("Wave Speed", Range(0, 10))      = 2
        _Direction ("Wave Direction (XY)", Vector)   = (1,0,0,0)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Cull Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            Name "Wave2D"

            HLSLPROGRAM
            #pragma target 4.5

            #pragma vertex   vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            float4 _BaseMap_ST;
            float4 _BaseColor;

            float _Amplitude;
            float _Frequency;
            float _Speed;
            float4 _Direction;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float4 color       : COLOR;
            };

            Varyings vert (Attributes IN)
            {
                Varyings OUT;

                float3 positionWS = TransformObjectToWorld(IN.positionOS);
                OUT.positionHCS   = TransformWorldToHClip(positionWS);

                OUT.uv    = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.color = IN.color * _BaseColor;

                return OUT;
            }

            float4 frag (Varyings IN) : SV_Target
            {
                float2 dir = normalize(_Direction.xy + float2(1e-4, 0));

                float2 centeredUV = IN.uv - 0.5;

                float phase = dot(centeredUV, dir) * _Frequency + _Time.y * _Speed;

                float2 waveOffset = dir * (sin(phase) * _Amplitude);

                float2 uv = IN.uv + waveOffset;

                float4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);

                float4 col = texColor * IN.color;

                return col;
            }
            ENDHLSL
        }
    }

    FallBack Off
}
