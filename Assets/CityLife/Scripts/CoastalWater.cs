using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace CityLife.World
{
    /// <summary>
    /// A bounded surface study for the separate coastal slice. Geometry is fixed; shader waves
    /// are local visual motion. This does not implement swimming, water physics or sea life.
    /// </summary>
    public static class CoastalWater
    {
        public const float Level = -2f;

        /// <summary>
        /// Creates the 180 x 200 metre surface at world y=-2. Enable the scene camera's URP depth
        /// texture for measured shallow colour and shore contact. A sea-direction colour fallback
        /// remains usable without a depth texture. The caller owns the generated mesh/material.
        /// </summary>
        public static GameObject Create(Transform parent)
        {
            const int columns = 91, rows = 101;
            var positions = new Vector3[columns * rows];
            var normals = new Vector3[positions.Length];
            var uv = new Vector2[positions.Length];
            var indices = new int[(columns - 1) * (rows - 1) * 6];
            for (int z = 0; z < rows; z++)
            for (int x = 0; x < columns; x++)
            {
                int i = z * columns + x;
                positions[i] = new Vector3(-90f + x * 2f, 0, -55f + z * 2f);
                normals[i] = Vector3.up;
                uv[i] = new Vector2(x / (float)(columns - 1), z / (float)(rows - 1));
            }
            int index = 0;
            for (int z = 0; z < rows - 1; z++)
            for (int x = 0; x < columns - 1; x++)
            {
                int a = z * columns + x, b = a + 1, c = a + columns, d = c + 1;
                indices[index++] = a; indices[index++] = c; indices[index++] = b;
                indices[index++] = b; indices[index++] = c; indices[index++] = d;
            }
            var mesh = new Mesh { name = "Coastal water 180x200m - 2m grid" };
            mesh.vertices = positions; mesh.normals = normals; mesh.uv = uv; mesh.triangles = indices;
            // Vertex waves stay inside this vertical envelope; no per-frame CPU mesh update.
            mesh.bounds = new Bounds(new Vector3(0, 0, 45), new Vector3(180, .6f, 200));

            Shader shader = Shader.Find("CityLife/CoastalWater");
            bool fallback = shader == null || !shader.isSupported;
            if (fallback) shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null || !shader.isSupported)
            {
                UnityEngine.Object.DestroyImmediate(mesh);
                throw new InvalidOperationException("No supported coastal-water or URP Unlit shader is available.");
            }
            var material = new Material(shader) { name = "Coastal luminous turquoise - surface study" };
            if (fallback)
            {
                // Explicit opaque turquoise fallback avoids a pink error material. Its simplified
                // appearance is reported; it is not evidence of the intended depth/wave shader.
                material.SetColor("_BaseColor", new Color(.015f, .48f, .46f, 1));
                Debug.LogWarning("COASTAL_WATER_FALLBACK: custom shader unavailable; using opaque URP Unlit turquoise. Depth/wave appearance remains unverified.");
            }
            else
            {
                material.SetFloat("_WaterLevel", Level);
                material.SetFloat("_UseSceneDepth", 1);
            }
            var surface = new GameObject("Coastal water - luminous river and sea");
            surface.transform.position = new Vector3(0, Level, 0);
            surface.transform.SetParent(parent, true);
            surface.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = surface.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            return surface;
        }
    }
}
