using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LethalLevelLoader;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace DOSCOMPANY
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string ModGUID = "IAmBatby.DOSCOMPANY";
        public const string ModName = "DOSCOMPANY";
        public const string ModVersion = "1.0.0.0";

        public static Plugin Instance;
        internal static readonly Harmony Harmony = new Harmony(ModGUID);
        internal static ManualLogSource logger;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;

            logger = Logger;
            Harmony.PatchAll(typeof(Patches));

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;

            ArtManifest.LoadAssets("doscompanybundle");

            ShaderManager.KillPipeline();

            logger.LogInfo("Finished Initalizing DOSCOMPANY.");
        }

        public static void DebugLog(string log)
        {
            logger.LogInfo(log);
        }

        public static void DebugLogError(string log)
        {
            logger.LogError(log);
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ShaderManager.hasRefreshed = false;
            ShaderManager.sources.Clear();
            if (ShaderManager.PostProcessors.Count == 0)
                ShaderManager.InitializePostProcessers(); 
            if (SceneManager.loadedSceneCount == 1)
            {
                ShaderManager.cameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).ToList();
                ShaderManager.RefreshAll();
                //ShaderManager.RefreshSky();
               // RenderSettings.skybox = new Material(ArtManifest.Instance.Skybox);
                //RenderSettings.skybox.SetColor("_Tint", new Color(0, 0, 0, 0));
                //ArtManifest.Instance.Skybox.SetColor("_Tint", Color.black);
                //RenderSettings.skybox = ArtManifest.Instance.Skybox;
            }
        }

        private static void OnSceneUnloaded(Scene scene)
        {
            //ShaderManager.latestNaturalSkyColor = Color.black;
        }
    }
}