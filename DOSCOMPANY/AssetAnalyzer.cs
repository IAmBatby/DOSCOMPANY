using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

namespace DOSCOMPANY
{
    internal static class AssetAnalyzer
    {
        private static Dictionary<Texture, Color> avereragedColorDict = new Dictionary<Texture, Color>();
        private static Dictionary<Texture, Color[]> cachedTextureDataDict = new Dictionary<Texture, Color[]>();

        public static Color GetAverageColor(Texture texture)
        {
            if (texture is Texture2D texture2d)
                return GetAverageColor(texture2d);
            else if (texture is Cubemap cubemap)
                return (GetAverageColor(cubemap));
            return (Color.magenta);
        }

        public static Color GetAverageColor(Cubemap cubemap)
        {
            List<Color> colorList = new List<Color>();
            foreach (Texture2D texture in Copy(cubemap))
                colorList.AddRange(texture.GetPixels());

            return (GetAverageColor(colorList.ToArray()));
        }

        public static Color GetAverageColor(Texture2D texture)
        {
            if (texture == null)
            {
                Plugin.DebugLogError("Texture Null! Can't Average!");
                return (Color.magenta);
            }
            if (avereragedColorDict == null)
                avereragedColorDict = new Dictionary<Texture, Color>();
            if (avereragedColorDict.TryGetValue(texture, out Color color))
                return (color);
            else
            {
                Color returnColor = GetAverageColor(GetPixels(texture));
                avereragedColorDict.Add(texture, returnColor);
                return (returnColor);
            }
        }

        public static Color GetAverageColor(params Texture2D[] textures)
        {
            List<Color> allPixels = new List<Color>();
            foreach (Texture2D tex in textures)
                allPixels.AddRange(GetPixels(tex));

            return (GetAverageColor(allPixels.ToArray()));
        }

        public static Color GetAverageColor(TerrainData terrainData)
        {
            List<TerrainPixel> pixels = new List<TerrainPixel>();
            float[,,] maps = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);
            for (int x = 0; x < maps.GetLength(0); x++)
                for (int y = 0; y < maps.GetLength(1); y++)
                    for (int z = 0; z < maps.GetLength(2); z++)
                        pixels.Add(new TerrainPixel(x, y, z, maps[x, y, z]));
            Dictionary<int, int> layerUsageCountDict = new Dictionary<int, int>();
            foreach (TerrainPixel kvp in pixels)
            {
                if (kvp.BlendValue > 0.5f)
                {
                    if (layerUsageCountDict.ContainsKey(kvp.TerrainLayerIndex))
                        layerUsageCountDict[kvp.TerrainLayerIndex]++;
                    else
                        layerUsageCountDict.Add(kvp.TerrainLayerIndex, 1);
                }
            }

            List<(Texture2D, int)> sortedTextures = new List<(Texture2D, int)>();
            foreach (KeyValuePair<int, int> kvp in layerUsageCountDict)
                sortedTextures.Add((terrainData.terrainLayers[kvp.Key].diffuseTexture, kvp.Value));
            sortedTextures = sortedTextures.OrderBy(t => t.Item2).Reverse().ToList();


            Color returnColor = GetAverageColor(terrainData.terrainLayers[0].diffuseTexture);
            if (sortedTextures.Count > 0)
            {
                returnColor = GetAverageColor(sortedTextures[0].Item1);
                for (int i = 0; i < sortedTextures.Count; i++)
                    returnColor = Color.Lerp(GetAverageColor(sortedTextures[i].Item1), returnColor, Mathf.InverseLerp(0,sortedTextures.Count, i));
                returnColor = Color.Lerp(GetAverageColor(sortedTextures[0].Item1), returnColor, 0.5f);
            }

            return (returnColor);
        }

        public static Texture2D GetMostUsedTerrainLayerTexture(TerrainData terrainData)
        {
            List<TerrainPixel> pixels = new List<TerrainPixel>();
            float[,,] maps = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);
            for (int x = 0; x < maps.GetLength(0); x++)
                for (int y = 0; y < maps.GetLength(1); y++)
                    for (int z = 0; z < maps.GetLength(2); z++)
                        pixels.Add(new TerrainPixel(x, y, z, maps[x, y, z]));
            Dictionary<int, int> layerUsageCountDict = new Dictionary<int, int>();
            foreach (TerrainPixel kvp in pixels)
            {
                if (kvp.BlendValue > 0.5f)
                {
                    if (layerUsageCountDict.ContainsKey(kvp.TerrainLayerIndex))
                        layerUsageCountDict[kvp.TerrainLayerIndex]++;
                    else
                        layerUsageCountDict.Add(kvp.TerrainLayerIndex, 1);
                }
            }

            List<(Texture2D, int)> sortedTextures = new List<(Texture2D, int)>();
            foreach (KeyValuePair<int, int> kvp in layerUsageCountDict)
                sortedTextures.Add((terrainData.terrainLayers[kvp.Key].diffuseTexture, kvp.Value));
            sortedTextures = sortedTextures.OrderBy(t => t.Item2).Reverse().ToList();

            return (sortedTextures.First().Item1);
        }

        public static Color GetAverageColor(Color[] colors)
        {
            Color returnColor = Color.white;
            int appliedColorCount = 0;
            float r = 0f, g = 0f, b = 0f;
            foreach (Color color in colors)
            {
                if (color.a > 0.1f && !(color.r == 0 && color.g == 0 && color.b == 0)) //put if not 0,0,0 here
                {
                    r += color.r;
                    g += color.g;
                    b += color.b;
                    appliedColorCount++;
                }
            }

            r /= appliedColorCount;
            g /= appliedColorCount;
            b /= appliedColorCount;

            Color averageColor = new Color(r, g, b, 1f);
            Color multiColor = new Color(r * 8, g * 8, b * 8);
            if ((r * 8) + (g * 8) + (b * 8) < 2.75f)
                returnColor = multiColor;
            else
                returnColor = averageColor;

            return (returnColor);
        }

        public static float GetAlphaPercentage(Texture2D tex)
        {
            Color[] pixels = GetPixels(tex);
            int alphaCount = 0;

            for (int i = 0; i < pixels.Length; i++)
                if (pixels[i].a == 0f)
                    alphaCount++;

            return (Mathf.InverseLerp(0, pixels.Length, alphaCount));
        }

        public static Color[] GetPixels(Texture2D tex)
        {
            Color32[] returnPixels32;
            Color[] returnPixels;
            if (cachedTextureDataDict.TryGetValue(tex, out Color[] value))
                return (value);
            else if (tex.isReadable)
                returnPixels32 = tex.GetPixels32();
            else
                returnPixels32 = Copy(tex).GetPixels32();

            returnPixels = new Color[returnPixels32.Length];
            for (int i = 0; (i < returnPixels32.Length); i++)
                returnPixels[i] = returnPixels32[i];
            cachedTextureDataDict.Add(tex, returnPixels);
            return (returnPixels);
        }


        public static Texture2D Copy(Texture2D source)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary(
            source.width,
            source.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear);

            Graphics.Blit(source, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            Texture2D readableText = new Texture2D(source.width, source.height);
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            return readableText;
        }

        public static Texture2D[] Copy(Cubemap source)
        {
            Texture2D[] faces = new Texture2D[6];
            for (int i = 0; i < faces.Length; i++)
            {
                //int previewSize = System.Math.Clamp(64, source.width >> (source.mipmapCount - 1), source.width);
                //faces[i] = new Texture2D(previewSize, previewSize, source.format, false);
                //int mipToCopy = (int)(System.Math.Log(source.width, 2) - System.Math.Log(previewSize, 2));
                
                //Graphics.CopyTexture(source, i, mipToCopy, faces[i], 0, 0);

                faces[i] = new Texture2D(source.width,source.height);
                //faces[i].SetPixels32(source.GetPixels(CubemapFace.PositiveX, source.mipmapCount - 1));
            }

            return (faces);
        }

        public static Texture2D Create(Color color)
        {
            Texture2D newTexture = new Texture2D(64, 64);
            Color[] colors = new Color[64];
            for (int i = 0;i < colors.Length;i++)
                colors[i] = color;
            newTexture.SetPixels(colors);

            return (newTexture);
        }

        //i probably need to check for colour here?
        private static Dictionary<(Texture2D, Color), Texture2D> remadeTextures = new Dictionary<(Texture2D, Color), Texture2D>();
        public static Texture2D Remake(Texture2D texture, Color color)
        {
            if (texture == null) return (null);
            if (remadeTextures.TryGetValue((texture, color), out Texture2D remadeTexture))
                return (remadeTexture);
            color = new Color(color.r, color.g, color.b, 1);
            Texture2D returnTexture = texture.isReadable ? texture : Copy(texture);
            returnTexture = Resize(returnTexture, 16, 16);
            returnTexture.requestedMipmapLevel = returnTexture.mipmapCount - 1;
            returnTexture.filterMode = FilterMode.Point;
            //remadeTexture.Apply();
            Color[] colors = returnTexture.GetPixels();
            for (int i = 0; i < colors.Length; i++)
            {
                if (colors[i].a != 0)
                    colors[i] = color;
            }

            returnTexture.SetPixels(colors);
            returnTexture.Apply();
            remadeTextures.Add((texture, color), returnTexture);
            return (returnTexture);
        }

        static Texture2D Resize(Texture2D texture2D, int targetX, int targetY)
        {
            RenderTexture rt = new RenderTexture(targetX, targetY, 24);
            RenderTexture.active = rt;
            Graphics.Blit(texture2D, rt);
            Texture2D result = new Texture2D(targetX, targetY);
            result.ReadPixels(new Rect(0, 0, targetX, targetY), 0, 0);
            //result.Apply();
            return result;
        }
    }
}
