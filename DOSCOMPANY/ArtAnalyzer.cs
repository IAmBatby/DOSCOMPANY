using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DOSCOMPANY
{
    internal static class ArtAnalyzer
    {
        private static Color invalidColor = new Color(5000, 5000, 5000, 1);
        internal static List<ArtInfo> AllInfos = new List<ArtInfo>();
        internal static Dictionary<Material, Texture> preferredTextureDict = new Dictionary<Material, Texture>();
        internal static Dictionary<(Material, Texture), Shader> preferredShaderDict = new Dictionary<(Material, Texture), Shader>();

        internal static void ProcessArt(Material material, GameObject sourceObject = null, Renderer sourceRenderer = null, Component specialObject = null)
        {
            int preferredColorID = -1;
            int preferredTextureID = -1;
            Color preferredColor = invalidColor;
            Texture preferredTexture = null;
            Shader preferredShader = null;

            ArtInfo newInfo = new ArtInfo(material, sourceRenderer, sourceObject, specialObject);

            TryFindPreferredColorID(newInfo, out preferredColorID);
            TryFindPreferredTextureID(newInfo, out preferredTextureID);

            newInfo.SetArtAnalysis(preferredColorID, preferredTextureID);

            TryFindPreferedTexture(newInfo, out preferredTexture);
            TryFindPreferedUnlitColor(newInfo, preferredTexture, out preferredColor);
            TryFindPreferredShader(newInfo, preferredColor, preferredTexture, out preferredShader);

            newInfo.SetArtInitialization(preferredColor, preferredShader, preferredTexture);

            //PostProcess Stuff

            newInfo.FinalizeArt();

            AllInfos.Add(newInfo);
        }



        internal static bool TryFindPreferedTexture(ArtInfo artInfo, out Texture returnTexture)
        {
            returnTexture = null;
            Material mat = artInfo.Material;

            if (preferredTextureDict.TryGetValue(mat, out Texture cachedTexture))
                returnTexture = cachedTexture;
            else if (artInfo.SpecialObject != null && artInfo.SpecialObject is Terrain terrain && terrain.terrainData != null)
                returnTexture = AssetAnalyzer.GetMostUsedTerrainLayerTexture(terrain.terrainData);
            else if (mat.TryGetTexture(artInfo.PreferedTextureID, out Texture preferredTexture))
                returnTexture = preferredTexture;

            if (!preferredTextureDict.ContainsKey(mat))
                preferredTextureDict.Add(mat, returnTexture);

            return (returnTexture != null);
        }

        internal static bool TryFindPreferedUnlitColor(ArtInfo artInfo, Texture preferedTexture, out Color returnColor)
        {
            returnColor = invalidColor;
            Material mat = artInfo.Material;

            if (mat.name.Contains("Water") || mat.shader.name.Contains("Water"))
                returnColor = Color.Lerp(Color.blue, ShaderManager.LatestNaturalSkyColor, 0.5f);
            else if (preferedTexture != null && mat.TryGetTexture(IDs.T_MainTex, out Texture mainResult) && mainResult != preferedTexture)
                returnColor = AssetAnalyzer.GetAverageColor(preferedTexture);
            else if (mat.TryGetFloat(IDs.F_UseEmissiveIntensity, out float floatValue) && floatValue == 1 && mat.TryGetColor(IDs.C_EmissiveColor, out Color emisColor))
                returnColor = emisColor;
            else if (preferedTexture != null && mat.TryGetTexture(IDs.T_MainTex, out Texture mainResult1) && mainResult1 == preferedTexture)
                returnColor = AssetAnalyzer.GetAverageColor(mainResult1);
            else if (mat.TryGetColor(artInfo.PreferedColorID, out Color preferedColor))
                returnColor = preferedColor;

            return (returnColor != invalidColor);
        }

        internal static bool TryFindPreferredShader(ArtInfo artInfo, Color preferredColor, Texture preferredTexture, out Shader returnShader)
        {
            returnShader = null;
            Material mat = artInfo.Material;

            if (artInfo.SpecialObject != null && artInfo.SpecialObject is ParticleSystem particleSystem)
                returnShader = IDs.S_TransparentUnlit;
            if (preferredTexture != null && preferredTexture is RenderTexture renderTexture)
                returnShader = IDs.S_TextureUnlit;
            else if (mat.name.Contains("Terrain") || mat.name.Contains("terrain") || mat.shader.name.Contains("Terrain"))
                returnShader = IDs.S_TerrainUnlit;
            else if (mat.TryGetFloat(IDs.F_AlphaCutoffEnable, out float cutoffValue) && cutoffValue == 1)
                mat.shader = IDs.S_TransparentUnlit;
            else
                mat.shader = IDs.S_ColorUnlit;


            return (returnShader != null);
        }

        internal static bool TryFindPreferredColorID(ArtInfo artInfo, out int returnID)
        {
            returnID = -1;
            Material mat = artInfo.Material;

            if (mat.TryGetColor(IDs.C_Color, out Color _))
                returnID = IDs.C_Color;
            else if (mat.TryGetColor(IDs.C_BaseColor, out Color _))
                returnID = IDs.C_BaseColor;

            return (returnID != -1);
        }

        internal static bool TryFindPreferredTextureID(ArtInfo artInfo, out int returnID)
        {
            returnID = -1;
            Material mat = artInfo.Material;

            if (!mat.HasTexture(IDs.T_MainTex) && mat.TryGetTexture(IDs.T_BaseColorMap, out Texture _))
                returnID = IDs.T_BaseColorMap;
            else if (mat.TryGetTexture(IDs.T_BaseAlbedoSmoothness, out Texture _))
                returnID = IDs.T_BaseAlbedoSmoothness;
            else if (mat.TryGetTexture(IDs.T_BaseColorMap1, out Texture _))
                returnID = IDs.T_BaseColorMap1;
            else if (mat.TryGetTexture(IDs.T_BaseColorMap, out Texture _))
                returnID = IDs.T_BaseColorMap;
            else if (mat.TryGetTexture(IDs.T_BarkBaseColorMap, out Texture _))
                returnID = IDs.T_BarkBaseColorMap;
            else if (mat.TryGetTexture(IDs.T_TrunkBaseColorMap, out Texture _))
                returnID = IDs.T_TrunkBaseColorMap;
            else if (mat.TryGetTexture(IDs.T_Albedo1, out Texture _))
                returnID = IDs.T_Albedo1;
            else if (mat.TryGetTexture(IDs.T_Albedo0, out Texture _))
                returnID = IDs.T_Albedo0;
            else if (mat.TryGetTexture(IDs.T_MainTex, out Texture _))
                returnID = IDs.T_MainTex;

            return (returnID != -1);
        }
    }
}
