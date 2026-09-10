using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object=UnityEngine.Object;

namespace CityLife.World.Editor
{
    /// <summary>Bakes the inspected local stage, never an IslandBootstrap or user world/save.</summary>
    public static class CosmicPreviewBuild
    {
        public static void Build(Camera camera,GameObject ground,Dictionary<string,string> shaderSources,string evidenceDirectory)
        {
            if(!Application.isBatchMode)throw new InvalidOperationException("Use isolated preview batch.");
            IslandValidation.Run();
            string id=DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            string folder="Assets/CityLife/GeneratedPreview-"+id;
            Directory.CreateDirectory(folder);AssetDatabase.Refresh();
            var persisted=new Dictionary<Object,Object>();int assetIndex=0;
            Object Persist(Object source)
            {
                if(source==null)return null;
                if(persisted.TryGetValue(source,out Object cached))return cached;
                string existing=AssetDatabase.GetAssetPath(source);
                if(!string.IsNullOrEmpty(existing))return source;
                if(source is Shader shader)
                {
                    if(!shaderSources.TryGetValue(shader.name,out string code))return source;
                    string shaderPath=folder+"/Shader-"+(assetIndex++)+".shader";
                    File.WriteAllText(shaderPath,code);AssetDatabase.ImportAsset(shaderPath,ImportAssetOptions.ForceSynchronousImport);
                    var result=AssetDatabase.LoadAssetAtPath<Shader>(shaderPath);persisted.Add(source,result);return result;
                }
                Object clone=Object.Instantiate(source);clone.hideFlags=HideFlags.None;
                persisted.Add(source,clone);
                if(clone is Material material)
                {
                    material.shader=(Shader)Persist(material.shader);
                    foreach(string property in material.GetTexturePropertyNames())
                        if(material.GetTexture(property)!=null)material.SetTexture(property,(Texture)Persist(material.GetTexture(property)));
                }
                AssetDatabase.CreateAsset(clone,folder+"/Asset-"+(assetIndex++)+".asset");return clone;
            }
            foreach(MeshFilter filter in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                filter.sharedMesh=(Mesh)Persist(filter.sharedMesh);
            foreach(Renderer renderer in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            {
                Material[] materials=renderer.sharedMaterials;
                for(int i=0;i<materials.Length;i++)materials[i]=(Material)Persist(materials[i]);
                renderer.sharedMaterials=materials;
            }
            MeshCollider collider=ground.GetComponent<MeshCollider>();if(collider==null)collider=ground.AddComponent<MeshCollider>();
            collider.sharedMesh=ground.GetComponent<MeshFilter>().sharedMesh;
            foreach(MeshFilter filter in Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None))
                if(filter.name=="Bark")
                {var c=filter.gameObject.AddComponent<MeshCollider>();c.sharedMesh=filter.sharedMesh;}
            camera.enabled=true;camera.tag="MainCamera";
            ground.layer=8;
            var explorer=camera.gameObject.AddComponent<CosmicPreviewExplorer>();explorer.Camera=camera;explorer.GroundMask=1<<8;
            if(camera.GetComponent<AudioListener>()==null)camera.gameObject.AddComponent<AudioListener>();

            var pipeline=Object.Instantiate((UniversalRenderPipelineAsset)GraphicsSettings.defaultRenderPipeline);
            pipeline.hideFlags=HideFlags.None;
            var pipelineSettings=new SerializedObject(pipeline);var renderers=pipelineSettings.FindProperty("m_RendererDataList");
            for(int i=0;i<renderers.arraySize;i++)
            {
                var renderer=Object.Instantiate(renderers.GetArrayElementAtIndex(i).objectReferenceValue);renderer.hideFlags=HideFlags.None;
                AssetDatabase.CreateAsset(renderer,folder+"/Renderer-"+i+".asset");renderers.GetArrayElementAtIndex(i).objectReferenceValue=renderer;
            }
            pipelineSettings.ApplyModifiedPropertiesWithoutUndo();AssetDatabase.CreateAsset(pipeline,folder+"/Pipeline.asset");
            string oldCompany=PlayerSettings.companyName,oldProduct=PlayerSettings.productName,oldVersion=PlayerSettings.bundleVersion;
            int oldWidth=PlayerSettings.defaultScreenWidth,oldHeight=PlayerSettings.defaultScreenHeight,oldQuality=QualitySettings.GetQualityLevel();
            bool oldBackground=PlayerSettings.runInBackground;FullScreenMode oldMode=PlayerSettings.fullScreenMode;
            RenderPipelineAsset oldGraphics=GraphicsSettings.defaultRenderPipeline;
            var oldPipelines=new RenderPipelineAsset[QualitySettings.names.Length];
            try
            {
                PlayerSettings.companyName="LocalWorldStudy";PlayerSettings.productName="Kooker Starfall";PlayerSettings.bundleVersion="0.0.1-wip";
                PlayerSettings.defaultScreenWidth=1600;PlayerSettings.defaultScreenHeight=900;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.runInBackground=true;
                GraphicsSettings.defaultRenderPipeline=pipeline;
                for(int i=0;i<oldPipelines.Length;i++){QualitySettings.SetQualityLevel(i);oldPipelines[i]=QualitySettings.renderPipeline;QualitySettings.renderPipeline=pipeline;}
                QualitySettings.SetQualityLevel(oldQuality);
                string scene=folder+"/CosmicPreview.unity";EditorSceneManager.SaveScene(camera.gameObject.scene,scene);AssetDatabase.SaveAssets();
                string output="Builds/KookerStarfall-"+id+"/KookerStarfall.exe";Directory.CreateDirectory(Path.GetDirectoryName(output));
                var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{scene},locationPathName=Path.GetFullPath(output),target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
                var evidence=new BuildEvidence{status=report.summary.result.ToString(),output=output,scene=scene,product=PlayerSettings.productName,bytes=(long)report.summary.totalSize,seconds=report.summary.totalTime.TotalSeconds,errors=(int)report.summary.totalErrors,warnings=(int)report.summary.totalWarnings,scope="Separate local WIP tree and blue-giant inspection stage. No IslandBootstrap, saved world, multiplayer or bot integrations. Visual gate not passed; source hybrid and runtime movement remain under review.",utc=DateTime.UtcNow.ToString("O")};
                File.WriteAllText(Path.Combine(evidenceDirectory,"preview-build.json"),JsonUtility.ToJson(evidence,true));
                if(report.summary.result!=BuildResult.Succeeded)throw new InvalidOperationException("Cosmic WIP preview build failed.");
                Debug.Log("COSMIC_PREVIEW_BUILD_SUCCEEDED "+output);
            }
            finally
            {
                PlayerSettings.companyName=oldCompany;PlayerSettings.productName=oldProduct;PlayerSettings.bundleVersion=oldVersion;
                PlayerSettings.defaultScreenWidth=oldWidth;PlayerSettings.defaultScreenHeight=oldHeight;PlayerSettings.fullScreenMode=oldMode;PlayerSettings.runInBackground=oldBackground;
                GraphicsSettings.defaultRenderPipeline=oldGraphics;
                for(int i=0;i<oldPipelines.Length;i++){QualitySettings.SetQualityLevel(i);QualitySettings.renderPipeline=oldPipelines[i];}
                QualitySettings.SetQualityLevel(oldQuality);AssetDatabase.SaveAssets();
            }
        }
        [Serializable]sealed class BuildEvidence{public string status,output,scene,product,scope,utc;public long bytes;public double seconds;public int errors,warnings;}
    }
}
