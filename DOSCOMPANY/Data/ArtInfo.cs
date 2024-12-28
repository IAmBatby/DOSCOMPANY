using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DOSCOMPANY
{
    internal enum ProcessState { Constructed, Analyzed, Initialized, Processed }
    internal enum TextureType { Null, Texture2D, RenderTexture, Cubemap }
    internal class ArtInfo
    {
        internal ProcessState CurrentState { get; private set; }

        internal Material Material { get; private set; }
        internal Renderer SourceRenderer { get; private set; }
        internal GameObject SourceObject { get; private set; }

        internal Color PreferredColor { get; private set; }
        internal Texture PreferredTexture { get; private set; }
        internal Shader PreferredShader { get; private set; }


        internal Color ProcessedColor { get; private set; }

        internal int PreferedColorID { get; private set; } = -1;
        internal int PreferedTextureID { get; private set; } = -1;

        internal Component SpecialObject { get; private set; }

        private Texture2D PreferredTexture2D;
        private Cubemap PreferredTextureCubemap;
        private RenderTexture PreferredTextureRenderTexture;

        internal ArtInfo(Material material, Renderer sourceRenderer = null, GameObject sourceObject = null, Component specialObject = null)
        {
            Material = material;
            SourceRenderer = sourceRenderer;
            SourceObject = sourceObject;
            SpecialObject = specialObject;
            CurrentState = ProcessState.Constructed;
        }

        internal void SetArtAnalysis(int prefColorID, int prefTextureID)
        {
            PreferedColorID = prefColorID;
            PreferedTextureID = prefTextureID;
            CurrentState = ProcessState.Analyzed;
        }

        internal void SetArtInitialization(Color newPreferredColor, Shader newPreferredShader, Texture newPrefferedTexture = null)
        {
            PreferredColor = newPreferredColor;
            PreferredTexture = newPrefferedTexture;
            PreferredShader = newPreferredShader;

            if (PreferredTexture != null)
            {
                if (PreferredTexture is Texture2D texture2D)
                    PreferredTexture2D = texture2D;
                else if (PreferredTexture is Cubemap textureCubemap)
                    PreferredTextureCubemap = textureCubemap;
                else if (PreferredTexture is RenderTexture renderTexture)
                    PreferredTextureRenderTexture = renderTexture;
            }

            if (PreferredTexture != null && PreferedTextureID != -1)
                Material.SetTexture(PreferedTextureID, PreferredTexture);
            if (PreferedColorID != -1)
                Material.SetColor(PreferedColorID, PreferredColor);
            CurrentState = ProcessState.Initialized;
        }

        internal void FinalizeArt()
        {
            Material.shader = PreferredShader;

            Material.TrySetColor(PreferedColorID, PreferredColor);

            if (PreferredTexture2D != null)
            {
                PreferredTexture2D = AssetAnalyzer.Remake(PreferredTexture2D, PreferredColor);
                PreferredTexture = PreferredTexture2D;
                Material.TrySetTexture(PreferedTextureID, PreferredTexture2D);
            }



            CurrentState = ProcessState.Processed;
        }
    }
}
