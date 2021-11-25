Shader "Custom/WireframeShader"
{
    Properties 
    {
        [Header (Color)][Space (10)]
        _WireColor ("Wire Color", Color) = (1, 1, 1, 1)
        _WireColorPow ("Wire Color Power", Float) = 1
        _WireScale ("Wire Scale", Range (0, 800)) = 1
    }

    // 메인라이트
    SubShader 
    {
        Blend SrcAlpha OneMinusSrcAlpha

        Tags 
        { 
            "RenderType"="Transparent"
            "Queue"="Transparent" 
        }

        Pass 
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag                    
            
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"
            
            struct v2g
            {
                float4 pos         : SV_POSITION;
                float4 wolrdPos     : TEXCOORD1;

                float2 uv          : TEXCOORD0;
                float3 normal		: NORMAL;

                float3 viewDir      : TEXCOORD2;
                float3 lightDir     : TEXCOORD3;
            };

            struct g2f
            {
                float4 pos         : SV_POSITION;
                float4 wolrdPos     : TEXCOORD1;

                float2 uv          : TEXCOORD0;
                float3 normal		: NORMAL;

                float3 viewDir      : TEXCOORD2;
                float3 lightDir     : TEXCOORD3;

                float4 dist : TEXCOORD4;
            };

            fixed4 _MainColor;
            fixed4 _WireColor;
            //sampler2D _MainTex;
            fixed _WireColorPow;
            fixed _WireScale;

            v2g vert (appdata_tan v)  
            { 
                v2g o;
                o.normal = v.normal;
                o.uv = v.texcoord.xy;
                o.wolrdPos = mul (unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.viewDir = normalize(ObjSpaceViewDir(v.vertex));
                o.lightDir = WorldSpaceLightDir (o.pos);

                return o;
            }

            [maxvertexcount (3)]
            void geom (triangle v2g i[3], inout TriangleStream <g2f> triangleStream)
            {
                float2 p0 = i[0].pos.xy / i[0].pos.w;
                float2 p1 = i[1].pos.xy / i[1].pos.w;
                float2 p2 = i[2].pos.xy / i[2].pos.w;

                float2 edge0 = p2 - p1;
                float2 edge1 = p2 - p0;
                float2 edge2 = p1 - p0;

                float area = abs (edge1.x * edge2.y - edge1.y * edge2.x);
                float wireThickness = 800 - _WireScale;

                g2f o;
                o.wolrdPos = i[0].wolrdPos;
                o.pos = i[0].pos;
                o.uv = i[0].uv;
                o.normal = i[0].normal;
                o.viewDir = i[0].viewDir;
                o.dist.xyz = float3 ((area/length (edge0)), 0, 0) * o.pos.w * _WireScale;
                o.dist.w = 1.0 / o.pos.w;
                o.lightDir = i[0].lightDir;
                triangleStream.Append (o);

                o.wolrdPos = i[1].wolrdPos;
                o.pos = i[1].pos;
                o.uv = i[1].uv;
                o.normal = i[1].normal;
                o.viewDir = i[1].viewDir;
                o.dist.xyz = float3 (0, (area/length (edge1)), 0) * o.pos.w * _WireScale;
                o.dist.w = 1.0 / o.pos.w;
                o.lightDir = i[1].lightDir;
                triangleStream.Append (o);

                o.wolrdPos = i[2].wolrdPos;
                o.pos = i[2].pos;
                o.uv = i[2].uv;
                o.normal = i[2].normal;
                o.viewDir = i[2].viewDir;
                o.dist.xyz = float3 (0, 0, (area/length (edge2))) * o.pos.w * _WireScale;
                o.dist.w = 1.0 / o.pos.w;
                o.lightDir = i[2].lightDir;
                triangleStream.Append (o);

            }

            fixed4 frag(g2f i) : COLOR
            {
                fixed4 ret = _WireColor;
                fixed minDistanceToEdge = min(i.dist[0], min(i.dist[1], i.dist[2])) * i.dist[3];
                ret.a *= pow ((1-minDistanceToEdge), _WireColorPow);
                ret.a = saturate (ret.a);

                // 빛 계산
                float nlDot = dot (i.normal, i.lightDir);
                nlDot = saturate (nlDot);
                ret.a *= nlDot;

                ret.rgb *= _LightColor0.rgb;

                return ret;
            }
            ENDCG
        }
    }
}