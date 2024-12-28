using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DOSCOMPANY
{
    internal abstract class PostProcessBase
    {
        private string _postProcessName = "No Processing";
        public virtual string PostProcessName { get => _postProcessName ; protected set => _postProcessName = value; }
        public abstract Color PostProcess(Color unlitColor, Renderer renderer, Texture2D texture);

        public bool TryPostProcess(Color unlitColor, Renderer renderer, Texture2D texture, out Color processedColor)
        {
            processedColor = PostProcess(unlitColor, renderer, texture);
            return (processedColor != unlitColor);
        }
    }

    internal class PostProcessGrass : PostProcessBase
    {
        public override string PostProcessName => "Grass";

        public override Color PostProcess(Color unlitColor, Renderer renderer, Texture2D texture)
        {
            return (renderer == null || !renderer.gameObject.CompareTag("Grass") ? unlitColor : new Color(unlitColor.r, unlitColor.g * 1.25f, unlitColor.b));
        }
    }

    internal class PostProcessSnow : PostProcessBase
    {
        public override string PostProcessName => "Snow";

        public override Color PostProcess(Color unlitColor, Renderer renderer, Texture2D texture)
        {
            return (renderer == null || !renderer.gameObject.CompareTag("Snow") ? unlitColor : new Color(unlitColor.r * 5, unlitColor.g * 5, unlitColor.b * 5));
        }
    }

    internal class PostProcessWood : PostProcessBase
    {
        public override string PostProcessName => "Wood";

        public override Color PostProcess(Color unlitColor, Renderer renderer, Texture2D texture)
        {
            return (renderer == null || !renderer.gameObject.CompareTag("Wood") ? unlitColor : new Color(unlitColor.r * 1.2f, unlitColor.g * 1.1f, unlitColor.b * 1.1f));
        }
    }

    internal class PostProcessGravel : PostProcessBase
    {
        public override string PostProcessName => "Gravel";

        public override Color PostProcess(Color unlitColor, Renderer renderer, Texture2D texture)
        {
            return (renderer == null || !renderer.gameObject.CompareTag("Gravel") ? unlitColor : new Color(unlitColor.r * 1.1f, unlitColor.g * 1.1f, unlitColor.b * 1.1f));
        }
    }

    internal class PostProcessConcrete : PostProcessBase
    {
        public override string PostProcessName => "Concrete";

        public override Color PostProcess(Color unlitColor, Renderer renderer, Texture2D texture)
        {
            if (renderer == null || !renderer.gameObject.CompareTag("Concrete")) return (unlitColor);
            Color returnColor = unlitColor;
            returnColor = new Color(returnColor.r, Mathf.Max(returnColor.r, returnColor.g), Mathf.Max(returnColor.r, returnColor.b));
            returnColor = new Color(returnColor.r * 0.5f, returnColor.g * 0.5f, returnColor.b * 0.5f);
            return (returnColor);
        }
    }

    internal class PostProcessTerrain : PostProcessBase
    {
        public override string PostProcessName => "Terrain";

        public override Color PostProcess(Color unlitColor, Renderer renderer, Texture2D texture)
        {
            return (renderer == null || renderer != ShaderManager.primaryRenderer ? unlitColor : new Color(unlitColor.r * 1.25f, unlitColor.g * 1.25f, unlitColor.b * 1.25f));
        }
    }
}
