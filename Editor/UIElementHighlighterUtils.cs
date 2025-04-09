using System;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;

namespace UIElementHighlighterTool.Editor
{
    public static class UIElementHighlighterUtils
    {
        
        private const string ShortcutBindingKey = "UIElementHighlighter_ShortcutBinding";
        
        public static Type GetTypeFromName(string typeName)
        {
            Type type = Type.GetType(typeName);

            if (type != null) return type;

            // Handling common Unity namespaces as a fallback
            string[] commonUnityNamespaces = new string[]
            {
                "UnityEngine.",
                "UnityEngine.UI."
            };

            foreach (string ns in commonUnityNamespaces)
            {
                type = Type.GetType(ns + typeName + ", UnityEngine");
                if (type != null) return type;

                type = Type.GetType(ns + typeName + ", UnityEngine.UI");
                if (type != null) return type;
            }

            // If not found, log an error
            Debug.LogError($"Type not found for component name: {typeName}");
            return null;
        }
        
        public static GUIStyle GetCenteredLabelStyle(int fontSize = 12, Color color = default, TextAnchor anchor = TextAnchor.LowerCenter)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                alignment = anchor,
                fontSize = fontSize,
                fontStyle = FontStyle.Bold
            };

            if (color != default)
            {
                style.normal.textColor = color;
            }

            return style;
        }
        
        public static bool GetSavedBool(string key, bool defaultValue)
        {
            return EditorPrefs.GetBool(key, defaultValue);
        }
    
        public static string GetSavedString(string key, string defaultValue)
        {
            return EditorPrefs.GetString(key, defaultValue);
        }

        public static Color GetSavedColor(string key, Color defaultColor)
        {
            string colorString = EditorPrefs.GetString(key, JsonUtility.ToJson(defaultColor, false));
            return JsonUtility.FromJson<Color>(colorString);
        }

        public static void SaveColor(string key, Color color)
        {
            string colorString = JsonUtility.ToJson(color, false);
            EditorPrefs.SetString(key, colorString);
        }
        
        public static UIElementHighlighterBinding LoadShortcut()
        {
            string json = EditorPrefs.GetString(ShortcutBindingKey, "");
            if (string.IsNullOrEmpty(json))
                return new UIElementHighlighterBinding { mainKey = KeyCode.H, mouseButton = -1 };
            return JsonUtility.FromJson<UIElementHighlighterBinding>(json);
        }

        public static void SaveShortcut(UIElementHighlighterBinding binding)
        {
            string json = JsonUtility.ToJson(binding);
            EditorPrefs.SetString(ShortcutBindingKey, json);
        }
    }
}