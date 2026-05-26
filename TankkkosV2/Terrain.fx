
float4x4 World;
float4x4 ViewProj;

float3 sunPos;
float sunShine;

float3 CamPos;


texture2D grassTex;
sampler grassTexSampler = sampler_state
{
    Texture = <grassTex>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture2D rockTex;
sampler rockTexSampler = sampler_state
{
    Texture = <rockTex>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture2D sandTex;
sampler sandTexSampler = sampler_state
{
    Texture = <sandTex>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};


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

float4 readTex(float3 worldPos, float3 normal, float2 texPos)
{
    
    float4 sandColor = float4(tex2D(sandTexSampler, texPos).rgb, 16);
    float4 rockColor = float4(tex2D(rockTexSampler, texPos).rgb, 30);
    float4 grassColor = float4(tex2D(grassTexSampler, texPos).rgb, 10);
    
    float steepness = sqrt(abs(normal.x) + abs(normal.z)) / normal.y;
    float grassRockRatio = steepness * 4 - 3;
    float sandRatio = (1 - saturate(steepness)) * (5 - worldPos.y);
    
    float4 grassRockColor = lerp(grassColor, rockColor, saturate( grassRockRatio ));
    return lerp(grassRockColor, sandColor, saturate(sandRatio));
    
}

float2 getPB(float3 normal, float3 worldPos, float shininess)
{
        
    float3 sunDir = normalize((CamPos + sunPos) - worldPos);
    
    float3 camDir = normalize(CamPos - worldPos);
    
    float lightPow = saturate(dot(normal, sunDir));
    
    float3 halfWay = normalize(camDir + sunDir);
    
    float shine = pow(saturate(dot(normal, halfWay)), shininess);
    
    return float2(lightPow, shine);

}

float4 PS(VSO input) : COLOR
{
    
    clip(input.worldPos.y);
    
    float3 normal = normalize(input.normal.xyz);
    
    float4 texOut = readTex(input.worldPos, normal, input.tex);
    float3 color = texOut.rgb;
    
    float2 pb = getPB(normal, input.worldPos, texOut.a);
    float lightPow = pb.x;
    float shine = pb.y;
    
    return float4(color * (lightPow * (0.4) + 0.7), 1) + float4(1, 1, 1, 1) * shine * sunShine;
    
}

float4 PS_Height(VSO vso) : COLOR
{
    return float4(vso.worldPos.y, 0, 0, 0);
}

float4 PS_Refraction(VSO input) : COLOR
{
    clip(2 - input.worldPos.y);
    
    float3 normal = normalize(input.normal.xyz);
    
    float4 texOut = readTex(input.worldPos, normal, input.tex);
    float3 color = texOut.rgb;
    
    float2 pb = getPB(normal, input.worldPos, texOut.a);
    float lightPow = pb.x;
    float shine = pb.y;
    
    return float4(color * (lightPow * (0.4) + 0.6), 1) + float4(1, 1, 1, 1) * shine * sunShine;

}


technique Water
{
    pass P0
    {
        VertexShader = compile vs_4_0_level_9_3 VS();
        PixelShader = compile ps_4_0_level_9_3 PS();
    }

    pass P1
    {
        VertexShader = compile vs_4_0_level_9_3 VS();
        PixelShader = compile ps_4_0_level_9_3 PS_Height();
    }

    pass P2
    {
        VertexShader = compile vs_4_0_level_9_3 VS();
        PixelShader = compile ps_4_0_level_9_3 PS_Refraction();
    }
}
