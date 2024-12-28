using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DOSCOMPANY
{
    internal struct UnlitInfo
    {
        public Material SampledMaterial { get; private set; }
        public Texture2D SampledTexture { get; private set; }
        public Renderer SourceRenderer { get; private set; }
        public Color InitialColor { get; private set; }
        public Color PostProcessedColor { get; private set; }
        public PostProcessBase Processor { get; private set; }


        public UnlitInfo(Material mat, Renderer renderer, Texture2D tex, Color initialColor, Color postProcessedColor, PostProcessBase processor)
        {
            SampledMaterial = mat;
            SampledTexture = tex;
            SourceRenderer = renderer;
            InitialColor = initialColor;
            PostProcessedColor = postProcessedColor;
            Processor = processor;

            Plugin.DebugLog("New Unlit Info. Material: " + SampledMaterial.name + ", Texture: " + SampledTexture?.name + " Renderer: " + renderer?.gameObject.name + ", Initial Color: " + ColorToString(InitialColor) + ", Processed Color: " + ColorToString(PostProcessedColor) + ", PostProcess Tag: " + processor?.PostProcessName);
        }

        public static string ColorToString(Color color)
        {
            return ("(r:" + color.r.ToString("F2") + ",g:" + color.g.ToString("F2") + ",b:" + color.b.ToString("F2") + ")");
        }
    }
}
