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
            float4 _Center;
            fixed4 _Color;
            fixed4 _BgColor;
            float _EdgeWidth;
            float _Thresh;

            v2f vert(appdata_t v)
            {
                v2f o;
                v.vertex.xy *= _Radius * 4;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.vertex.xy * 0.5 + 0.5; // Normalize to [0,1]
                return o;
            }

            float NearestCircleSDF(float2 pos) {
                float minDist = 1e5; // A large value to start with
                
                // Loop through circle positions and find the nearest distance
                for (int i = 0; i < 1; ++i) {
                    float2 center = tex2D(_PositionsTex, float2(i, 0)).xy * 0.5; // Normalize center to [0,1]
                    float radius = _Radius;
                    
                    float dist = distance(pos, center);
                    minDist = min(minDist, dist);
                }
        
                return minDist;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // float dist = NearestCircleSDF(i.uv);

                // Set color based on the signed distance
                // float circle = 1- smoothstep(-_Thresh, _Thresh, dist);
                // // if (circle < 0.01) discard;
                // return fixed4(dist, dist, circle, 1.0);
                
                float2 center = tex2D(_PositionsTex, float2(0, 0)).rg; // Normalize center to [0,1]
                float dist = distance(i.uv, center);
                float circle = smoothstep(_Radius - _EdgeWidth * _Radius, _Radius, dist);
                if (dist > _Radius) discard;
                return fixed4(_Color.rgb * (1-circle) + _BgColor * circle, 1);
            }
            ENDCG
        }
    }
}