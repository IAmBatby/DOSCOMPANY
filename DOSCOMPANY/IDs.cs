using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DOSCOMPANY
{
    internal static class IDs
    {
        internal static int T_MainTex { get; private set; } = Shader.PropertyToID("_MainTex");
        internal static int T_BaseColorMap { get; private set; } = Shader.PropertyToID("_BaseColorMap");
        internal static int T_BaseColorMap1 { get; private set; } = Shader.PropertyToID("_BaseColorMap1");
        internal static int T_BaseAlbedoSmoothness { get; private set; } = Shader.PropertyToID("_BaseAlbedoSmoothness");
        internal static int T_BarkBaseColorMap { get; private set; } = Shader.PropertyToID("_BarkBaseColorMap");
        internal static int T_TrunkBaseColorMap { get; private set; } = Shader.PropertyToID("_TrunkBaseColorMap");
        internal static int T_Albedo0 { get; private set; } = Shader.PropertyToID("_Albedo_0");
        internal static int T_Albedo1 { get; private set; } = Shader.PropertyToID("_Albedo_1");

        internal static int C_Color { get; private set; } = Shader.PropertyToID("_Color");
        internal static int C_MainColor { get; private set; } = Shader.PropertyToID("_MainColor");
        internal static int C_BaseColor { get; private set; } = Shader.PropertyToID("_BaseColor");
        internal static int C_EmissiveColor { get; private set; } = Shader.PropertyToID("_EmissiveColor");

        internal static int F_UseEmissiveIntensity { get; private set; } = Shader.PropertyToID("_UseEmissiveIntensity");
        internal static int F_AlphaCutoffEnable { get; private set; } = Shader.PropertyToID("_AlphaCutoffEnable");

        internal static Shader S_ColorUnlit => ArtManifest.Instance.ColorUnlit.shader;
        internal static Shader S_TextureUnlit => ArtManifest.Instance.TextureUnlit.shader;
        internal static Shader S_TransparentUnlit => ArtManifest.Instance.TransparentUnlit.shader;
        internal static Shader S_TerrainUnlit => ArtManifest.Instance.TerrainUnlit.shader;
    }
}
