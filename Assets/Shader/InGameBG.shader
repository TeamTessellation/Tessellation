Shader "Custom/InGameBG"
{
    Properties
    {
        _BaseMap    ("Base Map", 2D) = "white" {}
        _BaseColor  ("Base Color", Color) = (1,1,1,1)
        _TileSize ("Tile Size", float) = 1
        _LineSize ("Line Size", Range(0, 100)) = 40
        _Angle ("Angle", float) = 0

        _XSpeed     ("X Wave Speed", Range(0.0, 10.0)) = 1.0
        _BrightMin  ("Min Brightness", Range(0.0, 2.0)) = 0.4
        _BrightMax  ("Max Brightness", Range(0.0, 2.0)) = 1.8 
        _BrightLineSize ("Bright Line Size", float) = 1

        _Amplitude ("Wave Amplitude", Range(0, 0.1)) = 0.03
        _Frequency ("Wave Frequency", Range(0, 50))  = 10
        _Speed     ("Wave Speed", Range(0, 10))      = 2
        _Direction ("Wave Direction (XY)", Vector)   = (1,0,0,0)
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Name "UnlitWorldPosUI"

            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off
            ZTest Always      // 항상 화면 위에

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 worldPos    : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float _TileSize;
                float _Angle;
                float _LineSize;

                float _XSpeed;
                float _BrightMin;
                float _BrightMax;
                float _BrightLineSize;

                float _Amplitude;
                float _Frequency;
                float _Speed;
                float4 _Direction;
            CBUFFER_END

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            
            TEXTURE2D(_NoiseTexture);
            SAMPLER(sampler_NoiseTexture);

            float3 CoorToWorld(float3 coor, float size)
            {
                float x = coor.x * sqrt(3.0) + coor.y * sqrt(3.0) * 0.5;
                float y = -coor.y * 1.5;
                return float3(x, y, 0) * size;
            }

            float3 WorldToCoor(float2 world, float size)
            {
                world /= size;
                float cy = -world.y / 1.5;
                float cx = (world.x / sqrt(3.0)) - (cy * 0.5);
                float cz = -(cx + cy);
                float rx = round(cx);
                float ry = round(cy);
                float rz = round(cz);
                float dx = abs(rx - cx);
                float dy = abs(ry - cy);
                float dz = abs(rz - cz);
                if (dx > dy && dx > dz)
                {
                    rx = -ry - rz;
                }
                else if (dy > dz)
                {
                    ry = -rx - rz;
                }
                else
                {
                    rz = -rx - ry;
                }
                return float3(rx, ry, rz);
            }

            float HexPercentFromGrid(float2 local)
            {
                const float s = sqrt(3) * 0.5;
                const float2 n1 = float2(1.0, 0.0);
                const float2 n2 = float2(0.5, -s);
                const float2 n3 = float2(-0.5, -s);

                float p1 = abs(dot(local, n1));
                float p2 = abs(dot(local, n2));
                float p3 = abs(dot(local, n3));
                float m = max(p1, max(p2, p3));

                if (m > s) return 2.0;
                return m / s;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.worldPos = worldPos;

                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 dir = normalize(_Direction.xy + float2(1e-4, 0));
                float2 centeredUV = IN.uv - 0.5;
                float phase = dot(centeredUV, dir) * _Frequency + _Time.y * _Speed;
                float2 waveOffset = dir * (sin(phase) * _Amplitude);

                float2 worldPos = IN.worldPos;
                worldPos = worldPos + waveOffset;

                float3 coor = WorldToCoor(worldPos, _TileSize);
                float3 centerPos = CoorToWorld(coor, _TileSize);
                float2 local = (worldPos.xy - centerPos.xy) / _TileSize;
                float hexPercent = HexPercentFromGrid(local);
                if (hexPercent < 1 - _LineSize * 0.01)
                    discard;


                float t = _Time.y * _XSpeed * 0.01;
                t = frac(t);
                float distance = abs(IN.uv.y - t);
                distance = clamp(distance, 0, _BrightLineSize * 0.01) / (_BrightLineSize * 0.01);
                float bright = lerp(_BrightMax, _BrightMin, distance);

                half4 baseCol = half4(SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv).rgb + 0.001 + bright, 1);
                return baseCol;
            }

            ENDHLSL
        }
    }
}