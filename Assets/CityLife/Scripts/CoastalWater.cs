using System;
using System.Collections.Generic;
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
        public const float VisualSeaHalfWidth = 960f;
        public const float VisualSeaMinZ = 145f;
        public const float VisualSeaMaxZ = 1015f;

        /// <summary>
        /// Creates the 180 x 200 metre surface at world y=-2. Enable the scene camera's URP depth
        /// texture for measured shallow colour and shore contact. A sea-direction colour fallback
        /// remains usable without a depth texture. An additional 1920 x 870 m coarse distant sea
        /// is visual-only: no collider, navigation or bathymetry claim beyond the active slice.
        /// The caller owns both generated meshes and their shared material.
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
                material.SetColor("_BaseColor", new Color(.008f, .64f, .60f, 1));
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
            AddVisualSea(surface.transform, material);
            return surface;
        }

        private static void AddVisualSea(Transform parent, Material material)
        {
            // Keep the active edge's exact 2 m vertex positions, avoiding a wave crack where the
            // two meshes meet. Outer X spans and rows farther out are coarse, at 30 m / 20 m.
            var xs = new List<float>();
            for (float x = -VisualSeaHalfWidth; x < -90; x += 30) xs.Add(x);
            for (float x = -90; x <= 90; x += 2) xs.Add(x);
            for (float x = 120; x <= VisualSeaHalfWidth; x += 30) xs.Add(x);
            var zs = new List<float> { VisualSeaMinZ, VisualSeaMinZ + 2 };
            for (float z = VisualSeaMinZ + 22; z < VisualSeaMaxZ; z += 20) zs.Add(z);
            zs.Add(VisualSeaMaxZ);
            var positions = new Vector3[xs.Count * zs.Count];
            var normals = new Vector3[positions.Length];
            var indices = new int[(xs.Count - 1) * (zs.Count - 1) * 6];
            for (int z = 0; z < zs.Count; z++)
            for (int x = 0; x < xs.Count; x++)
            {
                int i = z * xs.Count + x;
                positions[i] = new Vector3(xs[x], 0, zs[z]); normals[i] = Vector3.up;
            }
            int index = 0;
            for (int z = 0; z < zs.Count - 1; z++)
            for (int x = 0; x < xs.Count - 1; x++)
            {
                int a = z * xs.Count + x, b = a + 1, c = a + xs.Count, d = c + 1;
                indices[index++] = a; indices[index++] = c; indices[index++] = b;
                indices[index++] = b; indices[index++] = c; indices[index++] = d;
            }
            var mesh = new Mesh { name = "Coastal distant sea - visual only 1920x870m" };
            mesh.vertices = positions; mesh.normals = normals; mesh.triangles = indices;
            mesh.bounds = new Bounds(new Vector3(0, 0, (VisualSeaMinZ + VisualSeaMaxZ) * .5f),
                new Vector3(VisualSeaHalfWidth * 2, .6f, VisualSeaMaxZ - VisualSeaMinZ));
            var distant = new GameObject("Distant sea continuation - visual only, no navigation or collider");
            distant.transform.SetParent(parent, false);
            distant.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = distant.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }
    }
}
