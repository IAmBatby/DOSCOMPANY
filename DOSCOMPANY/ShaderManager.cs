using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Rendering;
using UnityEngine;
using System.Collections;
using Object = UnityEngine.Object;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine.AI;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

namespace DOSCOMPANY
{
    internal static class ShaderManager
    {
        internal static List<PostProcessBase> PostProcessors = new List<PostProcessBase>();
        internal static List<UnlitInfo> allUnlits = new List<UnlitInfo>();
        internal static Dictionary<Material, UnlitInfo> unlitDict = new Dictionary<Material, UnlitInfo>();
        internal static List<Material> processedMats = new List<Material>();
        internal static Renderer primaryRenderer = null;
        internal static List<Camera> cameras = new List<Camera>();
        internal static MeshRenderer sunRenderer;
        internal static bool hasRefreshed;
        internal static Color LatestNaturalSkyColor { get; private set; }

        internal static List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>();

        internal static List<UnityEngine.Object> foundObjects = new List<UnityEngine.Object>();

        internal static Color minBlack = new Color(0.2f, 0.2f, 0.2f, 1);

        internal static void KillPipeline()
        {
            RenderPipelineManager.CleanupRenderPipeline();
            GraphicsSettings.renderPipelineAsset = null;
            QualitySettings.renderPipeline = null;
            Plugin.DebugLog("Killed Pipeline.");
        }

        internal static void InitializePostProcessers()
        {
            PostProcessors = new List<PostProcessBase>();
            PostProcessors.Add(new PostProcessTerrain());
            PostProcessors.Add(new PostProcessGrass());
            PostProcessors.Add(new PostProcessSnow());
            PostProcessors.Add(new PostProcessWood());
            PostProcessors.Add(new PostProcessGravel());
            PostProcessors.Add(new PostProcessConcrete());
        }

        internal static float GetTimeLerp()
        {
            float lerpValue = Mathf.InverseLerp(0, (TimeOfDay.Instance.totalTime * 0.9f), TimeOfDay.Instance.currentDayTime);
            return (lerpValue);
        }

        internal static void RefreshAll()
        {
            RefreshSky();

            for (int i = 0; i < allUnlits.Count; i++)
                allUnlits[i].SampledMaterial.color = Color.Lerp(TintColor(allUnlits[i].PostProcessedColor), Color.black, GetTimeLerp());
        }

        internal static Color TintColor(Color color)
        {
            return (Color.Lerp(Color.black, color, Mathf.Clamp(Mathf.InverseLerp(0, 500, TimeOfDay.Instance.sunDirect.intensity), 0.1f, 1.0f)));
        }

        internal static IEnumerator TryApplyAll(float delay)
        {
            yield return new WaitForSeconds(delay);


            if (SceneManager.sceneCount == 1)
            {
                foreach (LODGroup lODGroup in Object.FindObjectsOfType<LODGroup>(includeInactive: true))
                    DisableLODGroup(lODGroup);


                foreach (Renderer renderer in Resources.FindObjectsOfTypeAll<Renderer>())
                    SetRenderer(renderer);

                foreach (TextMeshProUGUI text in Resources.FindObjectsOfTypeAll<TextMeshProUGUI>())
                    text.font = ArtManifest.Instance.DOSFont;

                foreach (Image image in Resources.FindObjectsOfTypeAll<Image>())
                    if (image.sprite != null && image.sprite.texture is Texture2D texture)
                        texture.requestedMipmapLevel = texture.mipmapCount;

                foreach (ParticleSystem particleSystem in Resources.FindObjectsOfTypeAll<ParticleSystem>())
                {
                    if (particleSystem.TryGetComponent(out ParticleSystemRenderer renderer) && renderer.material != null)
                    {
                        renderer.material.shader = ArtManifest.Instance.TransparentUnlit.shader;
                        if (renderer.material.TryGetTexture2D(IDs.T_MainTex, out Texture2D mainTex))
                            renderer.material.mainTexture = AssetAnalyzer.Remake(mainTex, AssetAnalyzer.GetAverageColor(mainTex));
                    }
                }

            }
            else
            {
                TryFindTerrain();
                ApplyTerrains();

                foreach (Renderer renderer in SearchInLatestScene<Renderer>())
                    SetRenderer(renderer);

                foreach (ParticleSystem particleSystem in SearchInLatestScene<ParticleSystem>())
                {
                    if (particleSystem.TryGetComponent(out ParticleSystemRenderer renderer) && renderer.material != null)
                    {
                        renderer.material.shader = ArtManifest.Instance.TransparentUnlit.shader;
                        if (renderer.material.TryGetTexture2D(IDs.T_MainTex, out Texture2D mainTex))
                            renderer.material.mainTexture = AssetAnalyzer.Remake(mainTex, AssetAnalyzer.GetAverageColor(mainTex));
                    }
                }

                if (sunRenderer != null)
                    sunRenderer.material.shader = ArtManifest.Instance.TransparentUnlit.shader;

                TryFindSky();
            }




            //RefreshSky(Color.red);

            hasRefreshed = true;
            yield return null;

        }

        internal static List<T> SearchInLatestScene<T>() where T : UnityEngine.Object
        {
            List<T> returnList = new List<T>();
            foreach (GameObject sceneObject in SceneManager.GetSceneAt(SceneManager.sceneCount - 1).GetRootGameObjects())
                foreach (T component in sceneObject.GetComponentsInChildren<T>())
                    returnList.Add(component);
            return (returnList);
        }

        internal static void DisableLODGroup(LODGroup lodGroup)
        {
            if (lodGroup.transform.root.gameObject.scene.name == "SampleSceneRelay") return;
            if (lodGroup.lodCount > 0)
                lodGroup.ForceLOD(1);
        }

        internal static void RefreshSky()
        {
            if (TimeOfDay.Instance == null) return;
            Color newColor = LatestNaturalSkyColor;
            if (TimeOfDay.Instance.currentLevelWeather == LevelWeatherType.Eclipsed)
                newColor = ArtManifest.Instance.EclipsedColor;

           
            newColor = Color.Lerp(newColor, Color.black, GetTimeLerp());

            //ShaderManager.cameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).ToList();
            foreach (Camera camera in cameras)
            {
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = newColor;
            }
            //Plugin.DebugLog("Refreshed Sky! (" + newColor.r + "," + newColor.g + "," + newColor.b + ")");
        }

        internal static void ApplyTerrains()
        {
            foreach (Terrain terrain in Terrain.activeTerrains)
                if (terrain.terrainData != null && terrain.terrainData.terrainLayers.Length > 0)
                {
                    List<Texture2D> terrainTextures = terrain.terrainData.terrainLayers.Select(t => t.diffuseTexture).ToList();
                    Color color = PostProcessUnlitColor(AssetAnalyzer.GetAverageColor(terrain.terrainData), null, terrain.terrainData.terrainLayers[0].diffuseTexture).Item1;
                    terrain.materialTemplate = new Material(ArtManifest.Instance.TerrainUnlit);
                    terrain.drawInstanced = false;
                    terrain.materialTemplate.color = color;
                    terrain.treeDistance = 50000;
                    terrain.treeBillboardDistance = 50000;
                    terrain.detailObjectDistance = 50000;
                    terrain.treeMaximumFullLODCount = 50000;
                    LatestNaturalSkyColor = color;
                    terrain.Flush();
                    Plugin.DebugLog("APPLYING TERRAIN");
                }
        }

        internal static void TryFindSky()
        {
            Texture cubemap = null;
            foreach (Volume volume in Object.FindObjectsOfType<Volume>())
            {
                if (volume.gameObject.scene != SceneManager.GetActiveScene())
                    if (volume.profile != null)
                        foreach (VolumeComponent component in volume.profile.components)
                            if (component is HDRISky skyComponent)
                                if (skyComponent.active == true && skyComponent.hdriSky != null)
                                    if (skyComponent.hdriSky.value != null)
                                        cubemap = skyComponent.hdriSky.value;
            }

            if (cubemap != null)
            {
                Plugin.DebugLog("Found Cubemap: " + cubemap.name);
                //RenderSettings.skybox.mainTexture = cubemap;
                LatestNaturalSkyColor = AssetAnalyzer.GetAverageColor(cubemap);
            }
        }

        internal static void TryFindTerrain()
        {
            primaryRenderer = null;
            if (sources.Count > 0) return;
            NavMeshSurface surface = Object.FindFirstObjectByType<NavMeshSurface>();
            if (surface == null) return;

            sources = new List<NavMeshBuildSource>();
            List<NavMeshBuildMarkup> markups = new List<NavMeshBuildMarkup>();
            NavMeshBuilder.CollectSources(new Bounds(surface.center, surface.size), surface.layerMask, surface.useGeometry, surface.defaultArea, markups, sources);

            Plugin.DebugLog("Collected Sources (Count: " + sources.Count + ")");
            //buildSources = buildSources.OrderBy(x => (CombinedSize(x.size))).ToList();
            Vector3 biggestSize = Vector3.zero;
            MeshCollider biggestMesh = null;
            foreach (NavMeshBuildSource buildSource in sources)
            {
                if (buildSource.component == null)
                {
                    Plugin.DebugLog("Source: " + buildSource.component.name);
                    continue;
                }
                else if (buildSource.component.TryGetComponent(out MeshCollider meshCollider))
                {
                    Plugin.DebugLog("Source: " + buildSource.component.gameObject.name + " , Size: " + meshCollider.bounds.size);
                    if (biggestMesh == null || CombinedSize(meshCollider.bounds.size) > CombinedSize(biggestSize))
                    {
                        biggestSize = meshCollider.bounds.size;
                        biggestMesh = meshCollider;
                    }
                }
                else
                    Plugin.DebugLog("Source: " + buildSource.component.name + " (No Collider)");

            }

            if (biggestMesh != null && biggestMesh.TryGetComponent(out MeshRenderer renderer) && renderer.material != null)
            {
                primaryRenderer = renderer;
                Color color = PostProcessUnlitColor(GetUnlitColor(renderer.material), renderer, renderer.material.GetTexture(GetPreferedTexture(renderer.material)) as Texture2D).Item1;
                color = Color.Lerp(Color.blue, color, 0.9f);
                color = new Color(color.b * 0.6f, color.g * 0.3f, color.r * 0.5f);
                LatestNaturalSkyColor = color;
                RefreshSky();
            }
            else
            {
                Plugin.DebugLog("COULDN'T FIND TERRAIN!");
                LatestNaturalSkyColor = Color.blue;
                RefreshSky();
            }
        }

        internal static float CombinedSize(Vector3 size) => (size.x + size.y + size.z);


        internal static List<Material> changedMats = new List<Material>();
        internal static void SetRenderer(Renderer renderer)
        {
            changedMats.Clear();
            renderer.GetSharedMaterials(changedMats);
            for (int i = 0; i < changedMats.Count; i++)
            {
                if (changedMats[i] != null)
                    SetMaterial(changedMats[i], renderer);
                else if (renderer.TryGetComponent(out KillLocalPlayer killPlayer) && killPlayer.causeOfDeath == CauseOfDeath.Gravity)
                {
                    changedMats[i] = new Material(ArtManifest.Instance.ColorUnlit);
                    changedMats[i].name = "GravityMat";
                    changedMats[i].TrySetColor(IDs.C_Color, Color.black);
                    SetMaterial(changedMats[i], renderer);
                    renderer.enabled = true;
                }
            }

            renderer.SetSharedMaterials(changedMats);
        }

        internal static void SetMaterial(Material mat, Renderer renderer)
        {
            if (processedMats.Contains(mat)) return;
            try
            {
                Texture2D tex = null;
                int textureID = GetPreferedTexture(mat);
                if (mat.TryGetTexture2D(textureID, out Texture2D result))
                    tex = result;
                Color matColor = GetUnlitColor(mat);
                (Color color, PostProcessBase processor) output = PostProcessUnlitColor(matColor, renderer, tex);

                if (mat.TryGetTexture(IDs.T_MainTex, out Texture mainTexture) && mainTexture is RenderTexture)
                    mat.shader = ArtManifest.Instance.TextureUnlit.shader;
                else if (mat.name.Contains("Terrain") || mat.name.Contains("terrain") || mat.shader.name.Contains("Terrain"))
                    mat.shader = ArtManifest.Instance.TerrainUnlit.shader;
                else if (mat.TryGetFloat(IDs.F_AlphaCutoffEnable, out float cutoffValue) && cutoffValue == 1)
                    mat.shader = ArtManifest.Instance.TransparentUnlit.shader;
                else
                    mat.shader = ArtManifest.Instance.ColorUnlit.shader;

                if (tex != null && GetPreferedTexture(mat) != -1)
                    mat.TrySetTexture(GetPreferedTexture(mat), AssetAnalyzer.Remake(tex, output.color));

                mat.TrySetColor(IDs.C_Color, output.color);

                processedMats.Add(mat);
                UnlitInfo newUnlitInfo = new UnlitInfo(mat, renderer, tex, matColor, output.color, output.processor);
                unlitDict.Add(mat, newUnlitInfo);
                allUnlits.Add(newUnlitInfo);
            }
            catch { }
        }

        private static Color GetUnlitColor(Material mat)
        {
            Color matColor = Color.magenta;
            if (mat.name.Contains("Water") || mat.shader.name.Contains("Water"))
                matColor = Color.Lerp(Color.blue, LatestNaturalSkyColor, 0.5f);
            else if (!mat.HasTexture(IDs.T_MainTex) && mat.TryGetTexture(IDs.T_BaseColorMap, out Texture baseTexture))
                matColor = AssetAnalyzer.GetAverageColor(baseTexture);
            else if (mat.TryGetTexture(IDs.T_BaseAlbedoSmoothness, out Texture baseAlbedoTexture))
                matColor = AssetAnalyzer.GetAverageColor(baseAlbedoTexture);
            else if (mat.TryGetTexture(IDs.T_BaseColorMap1, out Texture baseColorMap1Texture))
                matColor = AssetAnalyzer.GetAverageColor(baseColorMap1Texture);
            else if (mat.TryGetTexture(IDs.T_BaseColorMap, out Texture baseColorMapTexture))
                matColor = AssetAnalyzer.GetAverageColor(baseColorMapTexture);
            else if (mat.TryGetTexture(IDs.T_BarkBaseColorMap, out Texture barkBaseColorMapTexture))
                matColor = AssetAnalyzer.GetAverageColor(barkBaseColorMapTexture);
            else if (mat.TryGetTexture(IDs.T_TrunkBaseColorMap, out Texture trunkBaseColorMapTexture))
                matColor = AssetAnalyzer.GetAverageColor(trunkBaseColorMapTexture);
            else if (mat.TryGetTexture(IDs.T_Albedo1, out Texture abledo1Texture))
                matColor = AssetAnalyzer.GetAverageColor(abledo1Texture);
            else if (mat.TryGetTexture(IDs.T_Albedo0, out Texture albedo0Texture))
                matColor = AssetAnalyzer.GetAverageColor(albedo0Texture);
            else if (mat.TryGetFloat(IDs.F_UseEmissiveIntensity, out float floatValue) && floatValue == 1 && mat.TryGetColor(IDs.C_EmissiveColor, out Color emisColor))
                matColor = emisColor;
            else if (mat.TryGetTexture(IDs.T_MainTex, out Texture mainTexture))
                matColor = AssetAnalyzer.GetAverageColor(mainTexture);
            else if (mat.TryGetColor(IDs.C_Color, out Color color))
                matColor = color;
            else if (mat.TryGetColor(IDs.C_BaseColor, out Color baseColor))
                matColor = baseColor;
            else
                Plugin.DebugLogError("Failed To Find Color To Sample For: " + mat.name);

            matColor = new Color(matColor.r * 1.1f, matColor.g * 1.1f, matColor.b * 1.1f);

            return (matColor);
        }

        private static int GetPreferedTexture(Material mat)
        {
            if (mat.HasTexture(IDs.T_BaseColorMap1))
                return (IDs.T_BaseColorMap1);
            if (mat.HasTexture(IDs.T_BaseColorMap))
                return (IDs.T_BaseColorMap);
            if (mat.HasTexture(IDs.T_MainTex))
                return (IDs.T_MainTex);

            return (-1);
        }

        private static (Color, PostProcessBase) PostProcessUnlitColor(Color color, Renderer renderer, Texture2D tex)
        {
            foreach (PostProcessBase processor in PostProcessors)
                if (processor.TryPostProcess(color, renderer, tex, out Color processedColor))
                    return ((processedColor, processor));

            return ((color, null));
        }
    }
}
