Shader "Custom/StageClear"
{
    Properties
    {
        _BaseMap    ("Base Map", 2D) = "white" {}
        _BaseColor  ("Base Color", Color) = (1,1,1,1)
        _TileSize ("Tile Size", float) = 1
        _Interval ("Interval", float) = 1
        _Progress ("Progress", Range(0, 1)) = 1
        _XCount ("XCount", float) = 2
        _StartX ("X Start", float) = 1
        _EndX ("X End", float) = 2
        _Direction ("Direction (0 = down->up / 1 = up->down)", float) = 1
        _Angle ("Angle", float) = 0
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
                float _Interval;
                float _Progress;
                float _StartX;
                float _EndX;
                float _XCount;
                float _Direction;
                float _Angle;
            CBUFFER_END

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

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

            float2 Rotate2D(float2 p, float angleRad)
            {
                float s = sin(angleRad);
                float c = cos(angleRad);
                return float2(
                    c * p.x - s * p.y,
                    s * p.x + c * p.y
                );
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
            //return half4(1, 1, 1, 1);
            float progress = 1 - _Progress;
            float2 worldPos = Rotate2D(IN.worldPos.xy, _Angle);
            float3 coor = WorldToCoor(worldPos, _TileSize);
            float3 centerPos = CoorToWorld(coor, _TileSize);
            float2 local = (worldPos.xy - centerPos.xy) / _TileSize;
            float hexPercent = HexPercentFromGrid(local);
            float order;
            UNITY_BRANCH
            if (_Direction == 0)
                order = _EndX - coor.x;
            else
                order = coor.x - _StartX;
            float correct = (_Interval * (_XCount - 1) + 1) / _XCount;
            if (hexPercent > progress * _XCount * correct  - _Interval * order)
                discard;

            half4 baseCol = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;
            return baseCol;
        }

            ENDHLSL
        }
    }
}