using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[InitializeOnLoad]
public class UIElementHighlighterSettings : EditorWindow
{
    // Default values
    private static Color defaultFillColor = new Color(1, 1, 0, 0.25f);
    private static Color defaultOutlineColor = Color.yellow;

    // Keys for EditorPrefs
    internal const string FillColorKey = "UIElementHighlighter_FillColor";
    internal const string OutlineColorKey = "UIElementHighlighter_OutlineColor";
    internal const string IgnoredLayerMaskKey = "UIElementHighlighter_IgnoredLayerMask";
    internal const string IgnoredTagsKey = "UIElementHighlighter_IgnoredTags";
    
    private static int _ignoredLayerMask;
    private static List<string> ignoredTags = new List<string>();

    [MenuItem("Tools/UI-EHT/Highlighter Settings")]
    public static void ShowWindow()
    {
        GetWindow<UIElementHighlighterSettings>("Highlighter Settings");
    }

    private void OnGUI()
    {
        // Using EditorPrefs to get and set colors
        EditorGUI.BeginChangeCheck();
        
        GUILayout.Label("Settings", EditorStyles.boldLabel);

        Color fillColor = EditorGUILayout.ColorField("Fill Color", GetSavedColor(FillColorKey, defaultFillColor));
        Color outlineColor = EditorGUILayout.ColorField("Outline Color", GetSavedColor(OutlineColorKey, defaultOutlineColor));
        
        int ignoredLayerMask = DrawIgnoredLayersSection();
        DrawIgnoredTagsSection();

        if (EditorGUI.EndChangeCheck())
        {
            SaveColor(FillColorKey, fillColor);
            SaveColor(OutlineColorKey, outlineColor);
            EditorPrefs.SetInt(IgnoredLayerMaskKey, ignoredLayerMask);
            EditorPrefs.SetString(IgnoredTagsKey, string.Join(",", ignoredTags));
        }
    }

    private int DrawIgnoredLayersSection()
    {
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        GUILayout.Label("Ignored Layers", EditorStyles.boldLabel);
        int displayMask = LayerMaskToConcatenatedLayersMask(EditorPrefs.GetInt("IgnoredLayerMask", 0));
        int newDisplayMask = EditorGUILayout.MaskField("", displayMask, InternalEditorUtility.layers);

        // Convert back before saving
        if (newDisplayMask != displayMask)
        {
            _ignoredLayerMask = ConcatenatedLayersMaskToLayerMask(newDisplayMask);
            EditorPrefs.SetInt("IgnoredLayerMask", _ignoredLayerMask);
        }

        return _ignoredLayerMask;
    }

    private void DrawIgnoredTagsSection()
    {
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        GUILayout.Label("Ignored Tags", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        // Load ignored tags from EditorPrefs
        string savedIgnoredTags = EditorPrefs.GetString(IgnoredTagsKey, "");
        ignoredTags = new List<string>(savedIgnoredTags.Split(','));

        foreach (string tag in InternalEditorUtility.tags)
        {
            bool isIgnored = ignoredTags.Contains(tag);
            bool shouldBeIgnored = EditorGUILayout.Toggle(tag, isIgnored);
            if (shouldBeIgnored && !isIgnored)
            {
                ignoredTags.Add(tag);
            }
            else if (!shouldBeIgnored && isIgnored)
            {
                ignoredTags.Remove(tag);
            }
        }

        EditorGUI.indentLevel--;
    }

    private static Color GetSavedColor(string key, Color defaultColor)
    {
        string colorString = EditorPrefs.GetString(key, JsonUtility.ToJson(defaultColor, false));
        return JsonUtility.FromJson<Color>(colorString);
    }

    private static void SaveColor(string key, Color color)
    {
        string colorString = JsonUtility.ToJson(color, false);
        EditorPrefs.SetString(key, colorString);
    }
    
    public static Color GetFillColor()
    {
        return GetSavedColor(FillColorKey, defaultFillColor);
    }

    public static Color GetOutlineColor()
    {
        return GetSavedColor(OutlineColorKey, defaultOutlineColor);
    }
    
    // Wrapper methods for EditorGUILayout's static methods
    private int LayerMaskToConcatenatedLayersMask(int originalMask)
    {
        return InternalEditorUtility.LayerMaskToConcatenatedLayersMask(originalMask);
    }

    private int ConcatenatedLayersMaskToLayerMask(int concatenatedMask)
    {
        return InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(concatenatedMask);
    }
}
