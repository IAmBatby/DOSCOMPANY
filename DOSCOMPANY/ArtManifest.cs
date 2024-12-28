using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;

namespace DOSCOMPANY
{
    [CreateAssetMenu(fileName = "ArtManifest", menuName = "ScriptableObjects/DOSCOMPANY/ArtManifest", order = 1)]
    public class ArtManifest : ScriptableObject
    {
        [field: SerializeField] public Material ColorUnlit { get; private set; }
        [field: SerializeField] public Material TextureUnlit { get; private set; }
        [field: SerializeField] public Material TransparentUnlit { get; private set; }
        [field: SerializeField] public Material TerrainUnlit { get; private set; }
        [field: SerializeField] public Material ShadowedUnlit { get; private set; }
        [field: SerializeField] public Material Skybox { get; private set; }

        [field: SerializeField] public TMP_FontAsset DOSFont { get; private set; }

        [field: SerializeField] public Color EclipsedColor { get; private set; }

        internal static AssetBundle AssetBundle { get; private set; }
        internal static ArtManifest Instance => _instance;
        private static ArtManifest _instance;

        internal static void LoadAssets(string bundleName)
        {
            string sAssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            AssetBundle = AssetBundle.LoadFromFile(Path.Combine(sAssemblyLocation, bundleName));
            if (AssetBundle == null)
            {
                Plugin.DebugLogError("Failed To Load AssetBundle!");
                return;
            }

            _instance = AssetBundle.LoadAllAssets<ArtManifest>()[0];
        }
    }
}
