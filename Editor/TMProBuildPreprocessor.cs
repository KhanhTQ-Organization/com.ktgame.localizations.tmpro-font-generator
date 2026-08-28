using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using TMPro;

namespace com.localizations.tmpro_font_generator.editor
{
    public class TMProBuildPreprocessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        private static List<TMP_FontAsset> _removedFallbacks = new List<TMP_FontAsset>();

        public void OnPreprocessBuild(BuildReport report)
        {
            _removedFallbacks.Clear();
            var mainFontAsset = TMP_Settings.defaultFontAsset;
            if (mainFontAsset != null && mainFontAsset.fallbackFontAssetTable != null)
            {
                for (var i = mainFontAsset.fallbackFontAssetTable.Count - 1; i >= 0; i--)
                {
                    var fallbackFontAssetExist = mainFontAsset.fallbackFontAssetTable[i];
                    if (fallbackFontAssetExist != null && fallbackFontAssetExist.atlasPopulationMode == AtlasPopulationMode.Static)
                    {
                        _removedFallbacks.Add(fallbackFontAssetExist);
                        mainFontAsset.fallbackFontAssetTable.RemoveAt(i);
                    }
                }
                if (_removedFallbacks.Count > 0)
                {
                    EditorUtility.SetDirty(mainFontAsset);
                }
            }
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            var mainFontAsset = TMP_Settings.defaultFontAsset;
            if (mainFontAsset != null && _removedFallbacks.Count > 0)
            {
                if (mainFontAsset.fallbackFontAssetTable == null)
                    mainFontAsset.fallbackFontAssetTable = new List<TMP_FontAsset>();
                
                for (int i = _removedFallbacks.Count - 1; i >= 0; i--)
                {
                    if (!mainFontAsset.fallbackFontAssetTable.Contains(_removedFallbacks[i]))
                    {
                        mainFontAsset.fallbackFontAssetTable.Add(_removedFallbacks[i]);
                    }
                }
                
                EditorUtility.SetDirty(mainFontAsset);
                AssetDatabase.SaveAssets();
                _removedFallbacks.Clear();
            }
        }
    }
}