using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CityLife.World.Editor
{
    public static partial class KokerboomRender
    {
        private static bool coastalMode;
        private static GameObject coast;
        public static void RenderCoastalSlice(){coastalMode=true;ph02FamilyMode=true;hybridMode=true;Run(false);}

        private static void Coastal()
        {
            Cosmic();StrongCoolFill();
            cosmicFloor.SetActive(false);companions.SetActive(false);
            if(coast==null)
            {
                coast=new GameObject("Starfall coastal slice v1 - separate world study");
                GameObject terrain=CoastalTerrain.Create(coast.transform);
                GameObject rocks=CoastalRocks.Create(coast.transform);
                GameObject water=CoastalWater.Create(coast.transform);
                subjects.Add(new Subject{Root=terrain,Kind="coastal-terrain"});
                subjects.Add(new Subject{Root=rocks,Kind="coastal-rocks-and-succulents"});
                subjects.Add(new Subject{Root=water,Kind="coastal-water-surface"});
                BuildCoastalGalaxy();
                File.WriteAllText(Path.Combine(outputDirectory,"coastal-definition.json"),
                    "{\"worldId\":\"starfall.coastal-slice.v1\",\"terrainSeed\":1904242,\"treeSeed\":4242,\"rockSeed\":4242,\"version\":\"1\",\"reference\":\"User attachments 02/03/06 provisional; exact requested frame pending\",\"boundsMetres\":{\"minX\":-90,\"maxX\":90,\"minZ\":-55,\"maxZ\":145},\"waterLevelMetres\":-2,\"sideCamera\":{\"position\":[21,4.6,-25],\"target\":[0,3.4,14],\"fieldOfView\":52},\"acceptance\":\"First actual Unity composition; not an exact artwork match or playable world\"}");
            }
            coast.SetActive(true);
            camera.GetComponent<UniversalAdditionalCameraData>().requiresDepthTexture=true;
            camera.GetComponent<UniversalAdditionalCameraData>().requiresColorTexture=true;
            camera.farClipPlane=1500;
        }

        private static void CoastalCamera(Vector3 position,Vector3 aim)
        {
            Coastal();Perspective(position,aim,width*9/16);camera.fieldOfView=52;
        }

        private static List<Shot> BuildCoastalShots()=>new List<Shot>{
            new Shot{Id="01-coastal-side-composition",Purpose="Fixed provisional side-view: frozen R19 tree on rocky bank, wrapping turquoise river toward deep sea, canyon mountains, blue gas giant and distant procedural galaxy. Actual Unity geometry; no artwork billboard.",Height=width*9/16,Configure=()=>CoastalCamera(new Vector3(21,4.6f,-25),new Vector3(0,3.4f,14))},
            new Shot{Id="02-water-to-sea",Purpose="Fixed surface material/depth comparison from the river toward open sea; no underwater-life or swimming claim.",Height=width*9/16,Configure=()=>CoastalCamera(new Vector3(10,-.1f,13),new Vector3(-2,-1,64))},
            new Shot{Id="03-rocky-tree-bank",Purpose="Fixed gameplay-distance rock bank, grounding and secondary succulent surface view; collision geometry exists but native input not yet verified.",Height=width*9/16,Configure=()=>CoastalCamera(new Vector3(13,1.9f,-12),new Vector3(2,-.4f,-3))},
            new Shot{Id="04-canyon-opening",Purpose="Fixed overview of layered canyon banks, river continuity and opening to sea. Element critique separate from combined scene.",Height=width*9/16,Configure=()=>CoastalCamera(new Vector3(-17,10,5),new Vector3(11,6,46))}
        };

        private static void BuildCoastalGalaxy()
        {
            var mesh=new Mesh{name="Procedural distant galaxy sky plane"};
            mesh.vertices=new[]{new Vector3(-650,-60,600),new Vector3(650,-60,600),new Vector3(650,420,600),new Vector3(-650,420,600)};
            mesh.uv=new[]{Vector2.zero,Vector2.right,Vector2.one,Vector2.up};
            mesh.triangles=new[]{0,2,1,0,3,2};mesh.RecalculateBounds();owned.Add(mesh);
            var sky=new GameObject("Distant galaxy - procedural dust and stellar band");sky.transform.SetParent(backdrop.transform,false);
            sky.AddComponent<MeshFilter>().sharedMesh=mesh;
            var renderer=sky.AddComponent<MeshRenderer>();renderer.sharedMaterial=TemporaryShaderMaterial("Coastal galaxy sky",CoastalGalaxyShader);renderer.shadowCastingMode=ShadowCastingMode.Off;
        }

        private const string CoastalGalaxyShader=@"
Shader ""Hidden/Starfall/CoastalGalaxy"" {
SubShader {Tags {""RenderPipeline""=""UniversalPipeline"" ""Queue""=""Background""}
Pass {Cull Off ZWrite Off
HLSLPROGRAM
#pragma vertex Vert
#pragma fragment Frag
#include ""Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl""
struct A{float4 p:POSITION;float2 uv:TEXCOORD0;};struct V{float4 p:SV_POSITION;float2 uv:TEXCOORD0;};
V Vert(A i){V o;o.p=TransformObjectToHClip(i.p.xyz);o.uv=i.uv;return o;}
float h(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
float n(float2 p){float2 q=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(h(q),h(q+float2(1,0)),f.x),lerp(h(q+float2(0,1)),h(q+1),f.x),f.y);}
half4 Frag(V i):SV_Target{
float2 uv=i.uv;float y=uv.y-(.65-.42*uv.x);float dust=n(uv*25)+.5*n(uv*65)+.25*n(uv*180);
float band=exp(-y*y*180)*saturate(dust*.8-.15);float r=(y+.013*(n(uv*37)-.5))*85;float rift=exp(-r*r)*.68;
float3 c=lerp(float3(.006,.016,.052),float3(.022,.058,.14),saturate(1-uv.y));
c+=band*(1-rift)*lerp(float3(.09,.13,.36),float3(.20,.36,.57),n(uv*39));
float star=pow(saturate(n(uv*2200)),85)*.9;c+=star*float3(.72,.85,1);
return half4(c,1);}
ENDHLSL
}}}";
    }
}
