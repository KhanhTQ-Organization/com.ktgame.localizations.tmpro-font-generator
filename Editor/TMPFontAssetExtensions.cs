using System.Reflection;
using TMPro;
using UnityEngine;

namespace com.localizations.tmpro_font_generator.editor
{
    public static class TMPFontAssetExtensions
    {
        public static void SetFontAssetProperty(this object target, string fieldName, object value)
        {
            var fieldInfo = typeof(TMP_FontAsset).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(target, value);
            }
        }

        public static T GetFontAssetProperty<T>(this object target, string fieldName)
        {
            var fieldInfo = typeof(TMP_FontAsset).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (fieldInfo != null)
            {
                return (T)fieldInfo.GetValue(target);
            }

            return default(T);
        }

        public static void CopyFontAssetPropertyFrom(this TMP_FontAsset dest, TMP_FontAsset source, string fieldName)
        {
            dest.SetFontAssetProperty(fieldName, source.GetFontAssetProperty<Font>(fieldName));
        }

        public static void CallFontAssetMethod(this TMP_FontAsset fontAsset, string methodName, params object[] parameters)
        {
            var methodInfo = typeof(TMP_FontAsset).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (methodInfo != null)
            {
                methodInfo.Invoke(fontAsset, parameters);
            }
        }
    }
}