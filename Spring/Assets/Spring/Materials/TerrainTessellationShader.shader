Shader "Custom/TessellationShader"
{
 
    // The properties block of the Unity shader. In this example this block is empty
    // because the output color is predefined in the fragment shader code.
    Properties
    {
        _Tess("Tessellation", Range(1, 32)) = 20
        _MaxTessDistance("Max Tess Distance", Range(1, 32)) = 20
        _Noise("Noise", 2D) = "gray" {}
        _NoiseScale ("Noise Scale", Float) = 0.1
 
        _Weight("Displacement Amount", Range(0, 1)) = 0

        [Header (Terrain Setting)]

        _NoiseTex ("Noise Texture", 2D) = "white" {}

        _SandCol ("Sand Color", Color) = (0, 0, 0, 1)
        _GrassCol ("Grass Color", Color) = (0, 0, 0, 1)
        _MountainCol ("Mountain Color", Color) = (0, 0, 0, 1)
        _SnowCol ("Snow Color", Color) = (0, 0, 0, 1)

        _GrassCutoff ("Grass Cutoff", Float) = -0.5
        _MountainCutoff ("Mountain Cutoff", Float) = 2.5
        _SnowCutoff ("Snow Cutoff", Float) = 25

    }

        // The SubShader block containing the Shader code. 
        SubShader
        {
            // SubShader Tags define when and under which conditions a SubShader block or
            // a pass is executed.
            Tags{ "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline" }

            Pass
            {
                Tags{ "LightMode" = "UniversalForward" }


                // The HLSL code block. Unity SRP uses the HLSL language.
                HLSLPROGRAM
                // The Core.hlsl file contains definitions of frequently used HLSL
                // macros and functions, and also contains #include references to other
                // HLSL files (for example, Common.hlsl, SpaceTransforms.hlsl, etc.). 
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"    
            #include "CustomTessellation.hlsl"
    
    
            #pragma require tessellation
                    // This line defines the name of the vertex shader. 
            #pragma vertex TessellationVertexProgram
                    // This line defines the name of the fragment shader. 
            #pragma fragment frag
                    // This line defines the name of the hull shader. 
            #pragma hull hull
                    // This line defines the name of the domain shader. 
            #pragma domain domain


            sampler2D _Noise;
            float _Weight;

            float _NoiseScale;


            // pre tesselation vertex program
            ControlPoint TessellationVertexProgram(Attributes v)
            {
                ControlPoint p;

                p.vertex = v.vertex;
                p.uv = v.uv;
                p.normal = v.normal;
                p.color = v.color;

                // 추가
                p.worldPos = v.worldPos;

                return p;
            }

            // after tesselation
            Varyings vert(Attributes input)
            {
                Varyings output;

                // 추가
                output.vertex = TransformObjectToHClip(input.vertex.xyz);
                output.worldPos = TransformObjectToWorld (input.vertex.xyz);

                //float Noise = tex2Dlod(_Noise, float4(input.uv, 0, 0)).r;
                float4 noiseUV = float4(output.worldPos.x, output.worldPos.z, 0, 0) * _NoiseScale;
                float Noise = tex2Dlod(_Noise, noiseUV).r;


                input.vertex.xyz += (input.normal) *  Noise * _Weight;
                output.vertex = TransformObjectToHClip(input.vertex.xyz);
                output.color = input.color;
                output.normal = input.normal;
                output.uv = input.uv;

                // 추가
                output.worldPos = TransformObjectToWorld (input.vertex.xyz);

                
                return output;
            }

            [UNITY_domain("tri")]
            Varyings domain(TessellationFactors factors, OutputPatch<ControlPoint, 3> patch, float3 barycentricCoordinates : SV_DomainLocation)
            {
                Attributes v;

            #define DomainPos(fieldName) v.fieldName = \
                        patch[0].fieldName * barycentricCoordinates.x + \
                        patch[1].fieldName * barycentricCoordinates.y + \
                        patch[2].fieldName * barycentricCoordinates.z;

                    DomainPos(vertex)
                    DomainPos(uv)
                    DomainPos(color)
                    DomainPos(normal)

                    return vert(v);
            }

            // 추가 프로퍼티
            sampler2D _NoiseTex;

            float4 _SandCol;
            float4 _GrassCol;
            float4 _MountainCol;
            float4 _SnowCol;

            float _GrassCutoff;
            float _MountainCutoff;
            float _SnowCutoff;

            // The fragment shader definition.            
            half4 frag(Varyings IN) : SV_Target
            {
                half4 tex = tex2D(_Noise, IN.uv);

                float4 retCol = _SandCol;

                float worldY = IN.worldPos.y;

                if (worldY > _SnowCutoff)
                {
                    retCol = _SnowCol;
                }
                else if (worldY > _MountainCutoff)
                {
                    retCol = _MountainCol;
                }
                else if (worldY > _GrassCutoff)
                {
                    retCol = _GrassCol;
                }

                retCol.a = 1;
                return retCol;
            }

            ENDHLSL
        }
    }
}