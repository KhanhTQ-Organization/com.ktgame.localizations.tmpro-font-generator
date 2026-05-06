// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using System.Reflection;
// using System.Text;
// using UnityEditor;
// using UnityEngine;
// using I2.Loc;
// using TMPro;
// using UnityEngine.TextCore.LowLevel;
// using Object = UnityEngine.Object;
//
// namespace com.localizations.tmpro_font_generator.editor
// {
// 	public class TMProFontGenerator : AssetModificationProcessor
// 	{
// 		private const string NotoSans = "Packages/com.localizations.tmpro-font-generator/Runtime/NotoSans.ttf";
// 		private const string NotoSansJP = "Packages/com.localizations.tmpro-font-generator/Runtime/NotoSansJP.ttf";
// 		private const string NotoSansKR = "Packages/com.localizations.tmpro-font-generator/Runtime/NotoSansKR.ttf";
// 		private const string NotoSansSC = "Packages/com.localizations.tmpro-font-generator/Runtime/NotoSansSC.ttf";
// 		private const string NotoSansTC = "Packages/com.localizations.tmpro-font-generator/Runtime/NotoSansTC.ttf";
// 		private const string NotoSansThai = "Packages/com.localizations.tmpro-font-generator/Runtime/NotoSansThai.ttf";
// 		private const string EnglishCharacters = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~…";
// 		private const string EnglishFontName = "English";
// 		private const string LocalizationAssetName = "I2Languages";
// 		private const int SamplingPointSize = 32;
// 		private const int Padding = 8;
// 		private static bool _isGenerating;
// 		private static readonly string SavePath;
// 		private static readonly Dictionary<string, Font> Fonts;
//
// 		static TMProFontGenerator()
// 		{
// 			_isGenerating = false;
// 			SavePath = Path.Combine("Assets", "Resources", "Fonts");
// 			Fonts = new Dictionary<string, Font>
// 			{
// 				{ "English", AssetDatabase.LoadAssetAtPath<Font>(NotoSans) },
// 				{ "Japanese", AssetDatabase.LoadAssetAtPath<Font>(NotoSansJP) },
// 				{ "Korean", AssetDatabase.LoadAssetAtPath<Font>(NotoSansKR) },
// 				{ "ChineseSC", AssetDatabase.LoadAssetAtPath<Font>(NotoSansSC) },
// 				{ "ChineseTC", AssetDatabase.LoadAssetAtPath<Font>(NotoSansTC) },
// 				{ "Thailand", AssetDatabase.LoadAssetAtPath<Font>(NotoSansThai) }
// 			};
// 		}
// 		
// 		private static string[] OnWillSaveAssets(string[] paths)
// 		{
// 			if (_isGenerating)
// 			{
// 				return paths;
// 			}
//
// 			var localizationAssetChanged = paths.Any(x => x.Contains(LocalizationAssetName));
//
// 			if (localizationAssetChanged)
// 			{
// 				_isGenerating = true;
// 				GenerateTMProFont();
// 				_isGenerating = false;
// 			}
//
// 			return paths;
// 		}
//
// 		private static void GenerateTMProFont()
// 		{
// 			var localizationAsset = GetAssetOfType<LanguageSourceAsset>(LocalizationAssetName);
// 			if (localizationAsset == null)
// 			{
// 				return;
// 			}
//
// 			var sourceData = localizationAsset.SourceData;
// 			if (sourceData == null)
// 			{
// 				return;
// 			}
//
// 			var languages = sourceData.mLanguages;
// 			if (languages == null || languages.Count == 0)
// 			{
// 				return;
// 			}
//
// 			var terms = sourceData.mTerms;
// 			if (terms == null || terms.Count == 0)
// 			{
// 				return;
// 			}
//
// 			var languageBuilderArray = new StringBuilder[languages.Count];
// 			for (var i = 0; i < languageBuilderArray.Length; i++)
// 			{
// 				languageBuilderArray[i] = new StringBuilder();
// 			}
//
// 			var charListArray = new HashSet<char>[languages.Count];
// 			for (var i = 0; i < charListArray.Length; i++)
// 			{
// 				charListArray[i] = new HashSet<char>();
// 			}
//
// 			foreach (var termData in terms)
// 			{
// 				for (var i = 0; i < languages.Count; ++i)
// 				{
// 					AppendToCharSet(charListArray[i], termData.Languages[i], LocalizationManager.IsRTL(languages[i].Code));
// 				}
// 			}
//
// 			var englishStaticFontAsset = CreateFontAsset(GetFont(EnglishFontName), EnglishCharacters);
// 			if (englishStaticFontAsset != null)
// 			{
// 				englishStaticFontAsset.atlasPopulationMode = AtlasPopulationMode.Static;
// 				englishStaticFontAsset = SaveFontAsset(englishStaticFontAsset, EnglishFontName);
// 			}
//
// 			var englishDynamicFontAsset = CreateDynamicFontAsset(GetFont(EnglishFontName), englishStaticFontAsset);
//
// 			for (var i = 0; i < charListArray.Length; i++)
// 			{
// 				var bytes = Encoding.UTF8.GetBytes(charListArray[i].ToArray().OrderBy(c => c).ToArray());
// 				var charSet = Encoding.UTF8.GetString(bytes);
// 				if (charSet.Length > 0)
// 				{
// 					var charBuilder = new StringBuilder();
// 					foreach (var character in charSet)
// 					{
// 						if (!EnglishCharacters.Contains(character))
// 						{
// 							charBuilder.Append(character);
// 						}
// 					}
//
// 					var language = languages[i].Name;
// 					if (!language.Equals(EnglishFontName))
// 					{
// 						Debug.Log($"{language} ({charBuilder.Length}):{charBuilder}");
// 						var fontAsset = CreateFontAsset(GetFont(language), charBuilder.ToString());
// 						if (fontAsset != null)
// 						{
// 							fontAsset.atlasPopulationMode = AtlasPopulationMode.Static;
// 							SaveFontAsset(fontAsset, language);
// 						}
// 					}
// 				}
// 			}
//
// 			englishStaticFontAsset.fallbackFontAssetTable = new List<TMP_FontAsset>();
//
// 			if (englishDynamicFontAsset != null)
// 			{
// 				englishStaticFontAsset.fallbackFontAssetTable.Add(englishDynamicFontAsset);
// 				EditorUtility.SetDirty(englishStaticFontAsset);
// 			}
//
// 			var languageDynamicFontAsset = Resources.Load<TMP_FontAsset>("LanguageName");
// 			if (languageDynamicFontAsset)
// 			{
// 				englishStaticFontAsset.fallbackFontAssetTable.Add(languageDynamicFontAsset);
// 				EditorUtility.SetDirty(englishStaticFontAsset);
// 			}
//
// 			if (TMP_Settings.defaultFontAsset == null || !TMP_Settings.defaultFontAsset.name.Equals(englishStaticFontAsset.name))
// 			{
// 				var defaultFontAsset = typeof(TMP_Settings).GetField("m_defaultFontAsset", BindingFlags.Instance | BindingFlags.NonPublic);
// 				if (defaultFontAsset != null)
// 				{
// 					defaultFontAsset.SetValue(TMP_Settings.instance, englishStaticFontAsset);
// 				}
//
// 				var defaultFontAssetPath = typeof(TMP_Settings).GetField("m_defaultFontAssetPath", BindingFlags.Instance | BindingFlags.NonPublic);
// 				if (defaultFontAssetPath != null)
// 				{
// 					defaultFontAssetPath.SetValue(TMP_Settings.instance, "Fonts");
// 				}
//
// 				EditorUtility.SetDirty(TMP_Settings.defaultFontAsset);
// 			}
//
// 			AssetDatabase.Refresh();
// 			AssetDatabase.SaveAssets();
// 			TMPro_EventManager.ON_FONT_PROPERTY_CHANGED(true, englishStaticFontAsset);
// 		}
//
// 		private static Font GetFont(string language)
// 		{
// 			switch (language)
// 			{
// 				case "Chinese":
// 				case "Chinese (PRC)":
// 				case "Chinese (Simplified)":
// 				case "Chinese (Singapore)":
// 					return Fonts["ChineseSC"];
// 				case "Chinese (Traditional)":
// 				case "Chinese (Hong Kong)":
// 				case "Chinese (Taiwan)":
// 				case "Chinese (Macau)":
// 					return Fonts["ChineseTC"];
// 				case "Japanese":
// 					return Fonts["Japanese"];
// 				case "Korean":
// 					return Fonts["Korean"];
// 				case "Thai":
// 				case "Thailand":
// 					return Fonts["Thailand"];
// 				default:
// 					return Fonts[EnglishFontName];
// 			}
// 		}
//
// 		private static void AppendToCharSet(HashSet<char> sb, string text, bool isRTL)
// 		{
// 			if (string.IsNullOrEmpty(text))
// 			{
// 				return;
// 			}
//
// 			text = RemoveTagsPrefix(text, "[i2p_");
// 			text = RemoveTagsPrefix(text, "[i2s_");
// 			text = RemoveTagsPrefix(text, "{");
//
// 			if (isRTL)
// 			{
// 				text = RTLFixer.Fix(text);
// 			}
//
// 			foreach (var c in text)
// 			{
// 				sb.Add(char.ToLowerInvariant(c));
// 				sb.Add(char.ToUpperInvariant(c));
// 			}
// 		}
//
// 		private static string RemoveTagsPrefix(string text, string tagPrefix)
// 		{
// 			var idx = 0;
// 			while (idx < text.Length)
// 			{
// 				idx = text.IndexOf(tagPrefix, StringComparison.Ordinal);
// 				if (idx < 0)
// 				{
// 					break;
// 				}
//
// 				var idx2 = text.IndexOf(']', idx);
// 				if (idx2 < 0)
// 				{
// 					break;
// 				}
//
// 				text = text.Remove(idx, idx2 - idx + 1);
// 			}
//
// 			idx = 0;
// 			while (idx < text.Length)
// 			{
// 				idx = text.IndexOf(tagPrefix, StringComparison.Ordinal);
// 				if (idx < 0)
// 				{
// 					break;
// 				}
//
// 				var idx2 = text.IndexOf('}', idx);
// 				if (idx2 < 0)
// 				{
// 					break;
// 				}
//
// 				text = text.Remove(idx, idx2 - idx + 1);
// 			}
//
// 			return text;
// 		}
//
// 		private static TMP_FontAsset CreateFontAsset(Font font, string characters)
// 		{
// 			var atlasSize = CalculateAtlasSize(SamplingPointSize, Padding, characters.Length);
// 			TMP_FontAsset fontAsset = null;
// 			var fitAtlasWidth = atlasSize;
// 			var fitAtlasHeight = atlasSize;
// 			var fitAttempt = 0;
//
// 			while (true)
// 			{
// 				fitAttempt++;
//
// 				if (fontAsset != null)
// 				{
// 					Object.DestroyImmediate(fontAsset);
// 				}
//
// 				fontAsset = TMP_FontAsset.CreateFontAsset(font, SamplingPointSize, Padding, GlyphRenderMode.SDFAA, fitAtlasWidth, fitAtlasHeight);
// 				if (!fontAsset.TryAddCharacters(characters))
// 				{
// 					break;
// 				}
//
// 				var maxAtlasOccupiedX = GetMaxAtlasOccupiedPixelX(fontAsset);
// 				var maxAtlasOccupiedY = GetMaxAtlasOccupiedPixelY(fontAsset);
// 				var pivotX = Mathf.FloorToInt((float)fitAtlasWidth / 2);
// 				var pivotY = Mathf.FloorToInt((float)fitAtlasHeight / 2);
// 				var canShrinkWidth = false;
// 				var canShrinkHeight = false;
//
// 				if (maxAtlasOccupiedX < pivotX)
// 				{
// 					canShrinkWidth = true;
// 					fitAtlasWidth /= 2;
// 				}
//
// 				if (maxAtlasOccupiedY < pivotY)
// 				{
// 					canShrinkHeight = true;
// 					fitAtlasHeight /= 2;
// 				}
//
// 				if ((!canShrinkWidth && !canShrinkHeight) || fitAttempt >= 3)
// 				{
// 					break;
// 				}
// 			}
//
// 			return fontAsset;
// 		}
//
// 		private static TMP_FontAsset CreateDynamicFontAsset(Font font, TMP_FontAsset mainFontAsset)
// 		{
// 			var dynamicFontAsset = TMP_FontAsset.CreateFontAsset(font, SamplingPointSize, Padding, GlyphRenderMode.SDFAA, 512, 512);
// 			typeof(TMP_FontAsset).GetField("m_ClearDynamicDataOnBuild", BindingFlags.Instance | BindingFlags.NonPublic)
// 				?.SetValue(dynamicFontAsset, true);
// 			var outputFont = SaveFontAsset(dynamicFontAsset, "Dynamic");
// 			CopyFontMaterialSettings(outputFont, mainFontAsset);
// 			return outputFont;
// 		}
//
// 		private static TMP_FontAsset SaveFontAsset(TMP_FontAsset fontAsset, string language)
// 		{
// 			if (!Directory.Exists(SavePath))
// 			{
// 				Directory.CreateDirectory(SavePath);
// 			}
//
// 			TMP_FontAsset outputFontAsset = null;
// 			var savePath = $"{SavePath}/{language}.asset";
//
// 			var existedFontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(savePath);
// 			if (existedFontAsset != null && !language.Equals(EnglishFontName))
// 			{
// 				AssetDatabase.DeleteAsset(savePath);
// 				existedFontAsset = null;
// 			}
//
// 			if (existedFontAsset == null)
// 			{
// 				AssetDatabase.CreateAsset(fontAsset, savePath);
// 				var savedFontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(savePath);
//
// 				foreach (var atlasTexture in fontAsset.atlasTextures)
// 				{
// 					AssetDatabase.AddObjectToAsset(atlasTexture, savedFontAsset);
// 				}
//
// 				AssetDatabase.AddObjectToAsset(fontAsset.material, savedFontAsset);
// 				outputFontAsset = savedFontAsset;
// 			}
// 			else
// 			{
// 				existedFontAsset.CopyFontAssetPropertyFrom(fontAsset, "version");
//
// 				var glyphInfoList = existedFontAsset.GetFontAssetProperty<List<TMP_Glyph>>("m_glyphInfoList");
//
// 				if (glyphInfoList != null && glyphInfoList.Count > 0)
// 				{
// 					existedFontAsset.SetFontAssetProperty("m_glyphInfoList", null);
// 				}
//
// 				existedFontAsset.CopyFontAssetPropertyFrom(fontAsset, "atlasRenderMode");
// 				existedFontAsset.CopyFontAssetPropertyFrom(fontAsset, "faceInfo");
// 				existedFontAsset.CopyFontAssetPropertyFrom(fontAsset, "glyphTable");
// 				existedFontAsset.CopyFontAssetPropertyFrom(fontAsset, "characterTable");
// 				existedFontAsset.CallFontAssetMethod("SortAllTables");
//
// 				if (fontAsset.atlasTextures is { Length: > 0 })
// 				{
// 					for (var i = 1; i < fontAsset.atlasTextures.Length; i++)
// 					{
// 						Object.DestroyImmediate(fontAsset.atlasTextures[i], true);
// 					}
// 				}
//
// 				existedFontAsset.SetFontAssetProperty("m_AtlasTextureIndex", 0);
// 				existedFontAsset.CopyFontAssetPropertyFrom(fontAsset, "atlasWidth");
// 				existedFontAsset.CopyFontAssetPropertyFrom(fontAsset, "atlasHeight");
// 				existedFontAsset.CopyFontAssetPropertyFrom(fontAsset, "atlasPadding");
// 				existedFontAsset.CopyFontAssetPropertyFrom(fontAsset, "atlasTextures");
// 				existedFontAsset.CopyFontAssetPropertyFrom(fontAsset, "freeGlyphRects");
// 				existedFontAsset.CopyFontAssetPropertyFrom(fontAsset, "usedGlyphRects");
// 				outputFontAsset = existedFontAsset;
// 			}
//
// 			AssetDatabase.Refresh();
// 			AssetDatabase.SaveAssets();
// 			AssetDatabase.ImportAsset(savePath);
// 			TMPro_EventManager.ON_FONT_PROPERTY_CHANGED(true, fontAsset);
// 			return outputFontAsset;
// 		}
//
// 		private static void CopyFontMaterialSettings(TMP_FontAsset targetFont, TMP_FontAsset masterFont)
// 		{
// 			var masterMaterial = masterFont.material;
// 			var englishShader = masterMaterial.shader;
// 			var englishShaderKeywordSpace = englishShader.keywordSpace;
// 			var englishOutlineKeyword = englishShaderKeywordSpace.FindKeyword(ShaderUtilities.Keyword_Outline);
// 			var isOutlineEnabled = masterMaterial.IsKeywordEnabled(englishOutlineKeyword.name);
//
// 			var englishUnderlayKeyword = englishShaderKeywordSpace.FindKeyword(ShaderUtilities.Keyword_Underlay);
// 			var isUnderlayEnabled = masterMaterial.IsKeywordEnabled(englishUnderlayKeyword.name);
//
// 			var material = targetFont.material;
// 			var shader = material.shader;
// 			var keywordSpace = shader.keywordSpace;
//
// 			material.SetFloat(ShaderUtilities.ID_WeightNormal, masterFont.normalStyle);
// 			material.SetFloat(ShaderUtilities.ID_WeightBold, masterFont.boldStyle);
//
// 			material.SetColor(ShaderUtilities.ID_FaceColor, masterMaterial.GetColor(ShaderUtilities.ID_FaceColor));
// 			material.SetFloat(ShaderUtilities.ID_FaceDilate, masterMaterial.GetFloat(ShaderUtilities.ID_FaceDilate));
//
// 			var outlineKeyword = keywordSpace.FindKeyword(ShaderUtilities.Keyword_Outline);
// 			material.SetKeyword(outlineKeyword, isOutlineEnabled);
// 			material.SetColor(ShaderUtilities.ID_OutlineColor, masterMaterial.GetColor(ShaderUtilities.ID_OutlineColor));
// 			material.SetFloat(ShaderUtilities.ID_OutlineWidth, masterMaterial.GetFloat(ShaderUtilities.ID_OutlineWidth));
// 			material.SetFloat(ShaderUtilities.ID_OutlineSoftness, masterMaterial.GetFloat(ShaderUtilities.ID_OutlineSoftness));
//
// 			var underlayKeyword = keywordSpace.FindKeyword(ShaderUtilities.Keyword_Underlay);
// 			material.SetKeyword(underlayKeyword, isUnderlayEnabled);
// 			material.SetColor(ShaderUtilities.ID_UnderlayColor, masterMaterial.GetColor(ShaderUtilities.ID_UnderlayColor));
// 			material.SetFloat(ShaderUtilities.ID_UnderlayDilate, masterMaterial.GetFloat(ShaderUtilities.ID_UnderlayDilate));
// 			material.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, masterMaterial.GetFloat(ShaderUtilities.ID_UnderlayOffsetX));
// 			material.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, masterMaterial.GetFloat(ShaderUtilities.ID_UnderlayOffsetY));
// 			material.SetFloat(ShaderUtilities.ID_UnderlaySoftness, masterMaterial.GetFloat(ShaderUtilities.ID_UnderlaySoftness));
//
// 			TMPro_EventManager.ON_FONT_PROPERTY_CHANGED(true, targetFont);
// 		}
//
// 		private static int CalculateAtlasSize(int samplingPointSize, int padding, int characterCount)
// 		{
// 			var sizePerCharacter = samplingPointSize + padding;
// 			var sqrt = Mathf.Sqrt(characterCount);
// 			var maxCharacter = Mathf.CeilToInt(sqrt);
// 			var atlasSize = maxCharacter * sizePerCharacter;
// 			var powerOfTwoSize = FindPowerOfTwoSize(atlasSize, 32);
// 			return powerOfTwoSize;
// 		}
//
// 		private static int FindPowerOfTwoSize(int number, int maxPower)
// 		{
// 			for (var i = 0; i <= maxPower; i++)
// 			{
// 				var powerOfTwo = Mathf.CeilToInt(Mathf.Pow(2, i));
// 				var diff = powerOfTwo - number;
// 				if (diff < 0)
// 				{
// 					continue;
// 				}
//
// 				return powerOfTwo;
// 			}
//
// 			return Mathf.CeilToInt(Mathf.Pow(2, maxPower));
// 		}
//
// 		private static int GetMaxAtlasOccupiedPixelX(TMP_FontAsset fontAsset)
// 		{
// 			var maxX = 0;
// 			foreach (var glyph in fontAsset.glyphTable)
// 			{
// 				var x = glyph.glyphRect.x + glyph.glyphRect.width + fontAsset.atlasPadding;
// 				if (x > maxX)
// 				{
// 					maxX = x;
// 				}
// 			}
//
// 			return maxX;
// 		}
//
// 		private static int GetMaxAtlasOccupiedPixelY(TMP_FontAsset fontAsset)
// 		{
// 			var maxY = 0;
// 			foreach (var glyph in fontAsset.glyphTable)
// 			{
// 				var y = glyph.glyphRect.y + glyph.glyphRect.height + fontAsset.atlasPadding;
// 				if (y > maxY)
// 				{
// 					maxY = y;
// 				}
// 			}
//
// 			return maxY;
// 		}
//
// 		private static T GetAssetOfType<T>(string name, Type mainType = null) where T : class
// 		{
// 			if (mainType == null)
// 			{
// 				mainType = typeof(T);
// 			}
//
// 			var guids = AssetDatabase.FindAssets(name + " t:" + mainType.Name);
// 			if (guids.Length == 0)
// 			{
// 				return null;
// 			}
//
// 			var guid = guids[0];
// 			var path = AssetDatabase.GUIDToAssetPath(guid);
// 			foreach (var o in AssetDatabase.LoadAllAssetsAtPath(path))
// 			{
// 				var res = o as T;
// 				if (res != null)
// 				{
// 					return res;
// 				}
// 			}
//
// 			return default(T);
// 		}
// 	}
// }
