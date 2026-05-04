float4x4 World;
float4x4 ViewProj;

float3 sunPos;
float sunShine;

float3 CamPos;
float3 Color;


struct VSI
{
    float4 pos : POSITION;
    float4 normal : NORMAL;
    float2 tex : TEXCOORD;
};

struct VSO
{
    float4 pos : POSITION;
    float4 normal : NORMAL;
    float3 worldPos : TEXCOORD2;
    float2 tex : TEXCOORD;
};

VSO VS(VSI input)
{
    
    VSO o;
    
    o.normal = input.normal;
    o.tex = input.tex;
    
    o.pos = mul(input.pos, World);
    
    o.worldPos = o.pos.xyz;
    
    o.pos = mul(o.pos, ViewProj);
    
    return o;
}

float4 PS(VSO input) : COLOR
{
    float3 color = Color;
    float3 normal = normalize(input.normal.xyz);
    
    float3 sunDir = normalize((CamPos + sunPos) - input.worldPos);
    float3 camDir = normalize(input.worldPos - CamPos);
    
    float lightPow = saturate(dot(normal, sunDir));
    
    float3 halfWay = normalize(camDir + sunDir);
    
    float shine = pow(saturate(dot(normal, halfWay)), 40);
    
    
    return float4 (color * (lightPow*(0.35)+0.65), 1) + float4(1, 1, 1, 1) * shine * sunShine;
    
}

technique Water
{
    pass P0
    {
        VertexShader = compile vs_4_0_level_9_3 VS();
        PixelShader = compile ps_4_0_level_9_3 PS();
    }
}
