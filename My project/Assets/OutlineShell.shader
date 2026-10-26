Shader "Custom/Outline Shell"
{
    Properties
    {
        _OutlineWidth ("Outline Width", Float) = 0.03
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Geometry+1"
        }

        Pass
        {
            Name "OutlineShell"
            Tags { "LightMode" = "UniversalForwardOnly" }

            Cull Front
            ZWrite Off
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _OutlineWidth;
                half4 _OutlineColor;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                // Laajenna mesh-verteksiä sen normaalin suuntaan reunuksen paksuuden verran.
                float3 expandedPositionOS = input.positionOS.xyz + input.normalOS * _OutlineWidth;
                output.positionHCS = TransformObjectToHClip(expandedPositionOS);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                // Etupuolien poiston ansiosta näkyviin jää laajennetun kuoren siluetti.
                return _OutlineColor;
            }
            ENDHLSL
        }
    }
}