Shader "Unlit/CircleShader"
{
    Properties
    {
        _Radius ("Radius", Range(0.1, 10)) = 1
        _NumPositions ("Num Positions", Int) = 1
        _PositionsTex("Positions", 2D) = "white" {}
        _Center ("Center", Vector) = (0.5, 0.5, 0, 0)
        _Color ("Color", Color) = (1, 1, 1, 1)
        _BgColor ("Background", Color) = (0, 0, 0, 0)
        _EdgeWidth ("Edge Width Percentage", Range(0, 0.1)) = 0.01
        _Thresh("SDF Threshold", Range(0, 4.0)) = 0.1
        _SmoothingCoef("Smoothing Coefficient", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float _Radius;
            int _NumPositions;
            sampler2D _PositionsTex;
            fixed4 _Color;
            fixed4 _BgColor;
            float _EdgeWidth;
            float _Thresh;
            float _SmoothingCoef;

            v2f vert(appdata_t v)
            {
                v2f o;
                v.vertex.xy *= 10;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.vertex.xy * 0.5 + 0.5; // Normalize to [0,1]
                return o;
            }

            float SmoothingSDF(float2 pos) {
                float minDist0 = 1e5; // The Smallest distance
                float minDist1 = 1e5; // The Second smallest distance
                
                // Loop through circle positions and find the nearest distance
                for (int i = 0; i < _NumPositions; i++) {
                    float2 center = tex2D(_PositionsTex, float2((i + 0.5)/(_NumPositions), 0.5)).rg; // Normalize center to [0,1]
                    
                    float dist = distance(pos, center);

                    minDist1 = min(minDist1, max(minDist0, dist));
                    minDist0 = min(minDist0, dist);
                }
                minDist0 = minDist0 - _Radius * _Radius;
                minDist1 = minDist1 - _Radius * _Radius;

                float k = _SmoothingCoef * _Radius;
                float h = clamp(0.5 + 0.5 * (minDist1 - minDist0) / k, 0.0, 1.0);
                return (lerp(minDist1, minDist0, h) - k * h * (1.0 - h)) - _Radius;

                // return minDist0 - _Radius;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float dist = SmoothingSDF(i.uv.xy); 
                float circle = smoothstep((_Thresh - _EdgeWidth) * _Radius - 0.001, _Thresh * _Radius, dist);
                if (dist > _Thresh * _Radius) discard;
                return fixed4(_Color.rgb * (1-circle) + _BgColor.rgb * circle, 1);
            }
            ENDCG
        }
    }
}