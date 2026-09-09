using System;
using System.Collections;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace CityLife.World
{
    public sealed class IslandBootstrap : MonoBehaviour
    {
        private string stage = "Preparing CityLife island";
        private string error;
        private bool ready;
        private CancellationTokenSource cancellation;
        public IslandField Field { get; private set; }
        private IEnumerator Start()
        {
            Application.runInBackground = true;
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
            cancellation = new CancellationTokenSource();
            IslandDefinition definition = null;
            try
            {
                definition = JsonUtility.FromJson<IslandDefinition>(Resources.Load<TextAsset>("IslandDefinition").text);
                definition.Validate(); Field = new IslandField(definition);
            }
            catch (Exception ex) { Fail(ex); }
            if (error != null) yield break;
            stage = "Shaping the desert coast · seed " + definition.seed;
            var token = cancellation.Token;
            var work = Task.Run(() => Field.Generate(token), token);
            while (!work.IsCompleted) yield return null;
            if (work.IsFaulted) { Fail(work.Exception.GetBaseException()); yield break; }
            if (work.IsCanceled) yield break;
            stage = "Preparing land, sea and sky";
            yield return null;
            try
            {
                bool isolatedSmoke = Array.IndexOf(Environment.GetCommandLineArgs(), "-citylifeSmoke") >= 0;
                string folder = Path.Combine(Application.persistentDataPath, "Worlds", definition.Fingerprint());
                string editsPath = Path.Combine(folder, "edits.json");
                // This is a new offline world. Never touches browser IndexedDB or invents a service endpoint.
                var edits = !isolatedSmoke && File.Exists(editsPath) ? JsonUtility.FromJson<WorldEdits>(File.ReadAllText(editsPath)) : WorldEdits.Empty(definition);
                if (edits == null) throw new InvalidDataException("Cannot read world edits. The original save has been preserved.");
                edits.Apply(Field);
                if (!isolatedSmoke)
                {
                    Directory.CreateDirectory(folder);
                    string definitionPath = Path.Combine(folder, "world.json");
                    if (!File.Exists(definitionPath)) File.WriteAllText(definitionPath, JsonUtility.ToJson(definition, true));
                    if (!File.Exists(editsPath)) edits.Save(editsPath);
                }
                var cam = Camera.main;
                if (cam == null) { var go = new GameObject("Explorer Camera"); go.tag = "MainCamera"; cam = go.AddComponent<Camera>(); }
                cam.nearClipPlane = 0.15f; cam.farClipPlane = 24000; cam.fieldOfView = 55;
                var renderer = gameObject.AddComponent<IslandRenderer>();
                renderer.Initialize(Field, cam);
                var explorer = gameObject.AddComponent<IslandExplorer>();
                explorer.Initialize(Field, cam, renderer);
                Debug.Log("CITYLIFE_WORLD_READY " + JsonUtility.ToJson(new WorldEvidence {
                    worldId = definition.worldId, fingerprint = definition.Fingerprint(), baseHeightHash = Field.BaseHash(),
                    widthMetres = definition.Width, landKm2 = Field.LandAreaKm2, gentleLandKm2 = Field.FlatAreaKm2,
                    maxHeightMetres = Field.MaxHeight, generationMs = Field.GenerationMilliseconds,
                    graphicsDevice = SystemInfo.graphicsDeviceName, graphicsMemoryMb = SystemInfo.graphicsMemorySize,
                    sourceCommit = definition.sourceCommit, editsRevision = edits.revision
                }));
                ready = true;
            }
            catch (Exception ex) { Fail(ex); }
        }
        private void Fail(Exception ex) { error = ex.Message; stage = "World could not open"; Debug.LogException(ex); }
        private void OnDestroy() { cancellation?.Cancel(); cancellation?.Dispose(); }
        private void OnGUI()
        {
            if (ready) return;
            GUI.backgroundColor = new Color(0.025f, 0.065f, 0.1f, 0.98f);
            var title = new GUIStyle(GUI.skin.label) { fontSize = 30, alignment = TextAnchor.MiddleCenter };
            var body = new GUIStyle(GUI.skin.label) { fontSize = 17, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "");
            GUI.Label(new Rect(0, Screen.height / 2 - 80, Screen.width, 50), "CITYLIFE", title);
            GUI.Label(new Rect(40, Screen.height / 2 - 15, Screen.width - 80, 100), stage + (error == null ? "" : "\n" + error), body);
        }
        [Serializable] private sealed class WorldEvidence
        {
            public string worldId, fingerprint, baseHeightHash, sourceCommit, graphicsDevice;
            public float widthMetres, landKm2, gentleLandKm2, maxHeightMetres;
            public long generationMs;
            public int graphicsMemoryMb, editsRevision;
        }
    }
}
