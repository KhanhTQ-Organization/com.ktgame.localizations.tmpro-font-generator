using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using TMPro;

namespace com.localizations.tmpro_font_generator.editor
{
    public class TMProBuildPreprocessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            var mainFontAsset = TMP_Settings.defaultFontAsset;
            if (mainFontAsset != null)
            {
                for (var i = mainFontAsset.fallbackFontAssetTable.Count - 1; i >= 0; i--)
                {
                    var fallbackFontAssetExist = mainFontAsset.fallbackFontAssetTable[i];
                    if (fallbackFontAssetExist.atlasPopulationMode == AtlasPopulationMode.Static)
                    {
                        mainFontAsset.fallbackFontAssetTable.Remove(fallbackFontAssetExist);
                    }
                }
            }
        }
    }
}