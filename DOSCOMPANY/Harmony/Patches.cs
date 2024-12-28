using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace DOSCOMPANY
{
    internal static class Patches
    {
        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.Awake)), HarmonyPrefix]
        internal static void StartOfRoundPatch()
        {
            foreach (SelectableLevel level in Resources.FindObjectsOfTypeAll<SelectableLevel>())
                level.videoReel = null;

            Transform terminalBlackCover = Object.FindObjectOfType<Terminal>().transform.Find("BlackCover");
            if (terminalBlackCover != null && terminalBlackCover.TryGetComponent(out Image image))
            {
                Debug.Log("Fixing Terminal Background!");
                image.enabled = true;
            }
        }

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.Start)), HarmonyPostfix]
        internal static void RoundMangerPatch()
        {
            //Disables transparent hud overlay which breaks rn with this mod
            GameObject face = GameObject.Find("PlayerHUDHelmetModel");
            if (face != null)
                face.SetActive(false);

            StartOfRound.Instance.StartCoroutine(ShaderManager.TryApplyAll(0.01f));

            //Cheat to make all moons free for testing
            foreach (TerminalNode node in Resources.FindObjectsOfTypeAll<TerminalNode>())
                node.itemCost = 0;
        }

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.FinishGeneratingNewLevelClientRpc)), HarmonyPostfix]
        internal static void LevelPatch()
        {
            StartOfRound.Instance.StartCoroutine(ShaderManager.TryApplyAll(0.1f));

            Transform sunTexture = TimeOfDay.Instance.sunAnimator.transform.Find("SunTexture");
            if (sunTexture != null && sunTexture.TryGetComponent(out MeshRenderer meshRenderer))
                ShaderManager.sunRenderer = meshRenderer;

        }

        [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.Start)), HarmonyPostfix]
        internal static void EnemyPatch(EnemyAI __instance)
        {
            foreach (MeshRenderer renderer in __instance.gameObject.GetComponentsInChildren<MeshRenderer>(includeInactive: true))
                ShaderManager.SetRenderer(renderer);
            foreach (SkinnedMeshRenderer renderer in __instance.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true))
                ShaderManager.SetRenderer(renderer);  
        }

        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.SetToCurrentLevelWeather)), HarmonyPostfix]
        internal static void RoundManagerCurrentLevelWeatherPatch()
        {
            //ShaderManager.RefreshSky(Color.black, onlyConditionally: true);
        }

        [HarmonyPatch(typeof(TimeOfDay), nameof(TimeOfDay.Update)), HarmonyPostfix]
        internal static void TimeOfDayUpdatePatch()
        {
            if (SceneManager.sceneCount == 1) return;
            ShaderManager.RefreshAll();

        }
    }
}
