#ifndef CUSTOM_UNLIT_PASS_INCLUDED
#define CUSTOM_UNLIT_PASS_INCLUDED

#include "../ShaderLibrary/Common.hlsl"

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);

CBUFFER_START(UnityPerMaterial)
    float4 _BaseColor;
    float4 _BaseMap_ST;
    float _Cutoff;
CBUFFER_END

struct Attributes
{
    float3 positionOS : POSITION;
    float2 baseUV : TEXCOORD0;
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float2 baseUV : VAR_BASE_UV;
};

Varyings UnlitPassVertex(Attributes input)
{
    Varyings output;
    float3 positionWS = TransformObjectToWorld(input.positionOS);
    output.positionCS = TransformWorldToHClip(positionWS);
    float4 baseST = _BaseMap_ST;
    output.baseUV = input.baseUV * baseST.xy + baseST.zw;
    return output;
}

float4 UnlitPassFragment(Varyings input) : SV_TARGET
{
    float4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.baseUV);
    float4 baseColor = _BaseColor;
    float4 base = baseMap * baseColor;
    #if defined(_CLIPPING)
        clip(base.a - _Cutoff);
    #endif
    return base;
}
#endif
