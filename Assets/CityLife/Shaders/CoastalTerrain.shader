Shader "CityLife/CoastalTerrain"
{
    Properties
    {
        _BaseColor("Overall tint",Color)=(1,1,1,1)
        _Sand("Warm sand",Color)=(0.63,0.39,0.19,1)
        _Ochre("Ochre stone",Color)=(0.57,0.25,0.095,1)
        _Pale("Pale sandstone strata",Color)=(0.74,0.47,0.24,1)
        _Rust("Terracotta strata",Color)=(0.39,0.13,0.07,1)
        _SeaLevel("Water elevation",Float)=-2
        _BaseMap("Shadow caster base",2D)="white"{}
        _Cutoff("Cutoff",Range(0,1))=.5
        _Cull("Cull",Float)=2
    }
    SubShader
    {
        Tags {"RenderType"="Opaque" "RenderPipeline"="UniversalPipeline"}
        Pass
        {
            Name "ForwardLit"
            Tags {"LightMode"="UniversalForward"}
            Cull Back
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor,_Sand,_Ochre,_Pale,_Rust,_BaseMap_ST;
                float _SeaLevel,_Cutoff,_Cull;
            CBUFFER_END
            struct Attributes {float4 positionOS:POSITION;float3 normalOS:NORMAL;};
            struct Varyings {float4 positionCS:SV_POSITION;float3 positionWS:TEXCOORD0;float3 normalWS:TEXCOORD1;float fog:TEXCOORD2;};
            float Hash(float3 p){p=frac(p*.1031);p+=dot(p,p.yzx+33.33);return frac((p.x+p.y)*p.z);}
            float Noise(float3 p)
            {
                float3 i=floor(p),f=frac(p);f=f*f*(3-2*f);
                return lerp(lerp(lerp(Hash(i),Hash(i+float3(1,0,0)),f.x),lerp(Hash(i+float3(0,1,0)),Hash(i+float3(1,1,0)),f.x),f.y),
                    lerp(lerp(Hash(i+float3(0,0,1)),Hash(i+float3(1,0,1)),f.x),lerp(Hash(i+float3(0,1,1)),Hash(i+1),f.x),f.y),f.z);
            }
            Varyings Vert(Attributes input)
            {
                Varyings o;o.positionWS=TransformObjectToWorld(input.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.positionWS);
                o.normalWS=TransformObjectToWorldNormal(input.normalOS);o.fog=ComputeFogFactor(o.positionCS.z);return o;
            }
            half4 Frag(Varyings i):SV_Target
            {
                float3 p=i.positionWS,n=normalize(i.normalWS);
                float broad=Noise(p*.15),grain=Noise(p*3.2);
                float strata=p.y*1.35+sin(p.x*.085+p.z*.047)*.47+(broad-.5)*.5;
                float layer=.5+.5*sin(strata*6.2831853);
                float narrow=1-smoothstep(.055,.15,abs(frac(strata*.37)-.48));
                float cliff=smoothstep(.10,.60,1-saturate(n.y));
                float high=smoothstep(3,16,p.y);
                float3 rock=lerp(_Ochre.rgb,_Pale.rgb,smoothstep(.23,.87,layer)*.60);
                rock=lerp(rock,_Rust.rgb,narrow*.22);
                float3 sand=_Sand.rgb*lerp(.90,1.11,broad);
                float3 albedo=lerp(sand,rock,saturate(cliff*.88+high*.40));
                // Grain is filtered toward its mean at distance; no sparkling screen-space noise.
                float fineVisibility=1-saturate(length(fwidth(p))*2);
                albedo*=1+(grain-.5)*.11*fineVisibility;
                float damp=1-smoothstep(_SeaLevel-.2,_SeaLevel+1.0,p.y);
                albedo*=lerp(1,.73,damp);
                InputData input=(InputData)0;input.positionWS=p;input.normalWS=n;
                input.viewDirectionWS=GetWorldSpaceNormalizeViewDir(p);input.shadowCoord=TransformWorldToShadowCoord(p);
                input.bakedGI=SampleSH(n);input.normalizedScreenSpaceUV=GetNormalizedScreenSpaceUV(i.positionCS);input.shadowMask=1;
                SurfaceData surface=(SurfaceData)0;surface.albedo=albedo*_BaseColor.rgb;surface.alpha=1;
                surface.smoothness=lerp(.12,.22,damp);surface.metallic=0;surface.occlusion=1;surface.normalTS=float3(0,0,1);
                half4 color=UniversalFragmentPBR(input,surface);color.rgb=MixFog(color.rgb,i.fog);return color;
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }
}
