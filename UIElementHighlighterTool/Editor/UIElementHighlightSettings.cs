using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[InitializeOnLoad]
public class UIElementHighlighterSettings : EditorWindow
{

    private static Color _enabledButtonColor = new Color(0f, 0.33f, 0.11f);
    private static Color _disabledButtonColor = new Color(0.33f, 0f, 0.02f);
    
    // Default values
    private static Color defaultFillColor = new Color(1, 1, 0, 0.25f);
    private static Color defaultOutlineColor = Color.yellow;
    private static bool isExtensionEnabled;
    private static int _ignoredLayerMask;

    // Keys for EditorPrefs
    internal const string FillColorKey = "UIElementHighlighter_FillColor";
    internal const string OutlineColorKey = "UIElementHighlighter_OutlineColor";
    internal const string IgnoredLayerMaskKey = "UIElementHighlighter_IgnoredLayerMask";
    internal const string IgnoredTagsKey = "UIElementHighlighter_IgnoredTags";
    private const string AutoCloseOnSelect = "UIElementHighlighter_AutoClose";
    private const string SelectComponent = "UIElementHighlighter_SelectComponent";
    private const string ExtensionEnabledKey = "UIElementHighlighter_Enabled";

    private const string _enabledText = "Extension is ENABLED \n Click to disable";
    private const string _disabledText = "Extension is DISABLED \n Click to enable";
    
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
        isExtensionEnabled = EditorPrefs.GetBool(ExtensionEnabledKey, true);
        
        DrawToggleExtensionButton();
        
        // If the extension is disabled, draw a semi-transparent overlay
        if (isExtensionEnabled == false)
        {
            EditorGUI.DrawRect(new Rect(0, 60, position.width, position.height - 60), new Color(0, 0, 0, 0.5f));
            EditorGUI.BeginDisabledGroup(true);
        }
        
        GUILayout.Space(30f);
        
        string selectComponent = EditorGUILayout.TextField("Select Component", GetSavedString(SelectComponent, "RectTransform"));
        bool autoCloseOnSelect = EditorGUILayout.Toggle("Auto Close On Select", GetSavedBool(AutoCloseOnSelect, false));
        
        GUILayout.Space(10f);

        Color fillColor = EditorGUILayout.ColorField("Fill Color", GetSavedColor(FillColorKey, defaultFillColor));
        Color outlineColor = EditorGUILayout.ColorField("Outline Color", GetSavedColor(OutlineColorKey, defaultOutlineColor));

        int ignoredLayerMask = DrawIgnoredLayersSection();
        DrawIgnoredTagsSection();
        
        if (isExtensionEnabled == false)
        {
            EditorGUI.EndDisabledGroup();
        }

        if (EditorGUI.EndChangeCheck())
        {
            SaveColor(FillColorKey, fillColor);
            SaveColor(OutlineColorKey, outlineColor);
            EditorPrefs.SetBool(AutoCloseOnSelect, autoCloseOnSelect);
            EditorPrefs.SetString(SelectComponent, selectComponent);
            EditorPrefs.SetInt(IgnoredLayerMaskKey, ignoredLayerMask);
            EditorPrefs.SetString(IgnoredTagsKey, string.Join(",", ignoredTags));
        }
    }

    private void DrawToggleExtensionButton()
    {
        // Toggle button for enabling/disabling the extension
        GUIStyle toggleButtonStyle = new GUIStyle(GUI.skin.button);
        toggleButtonStyle.normal.textColor = Color.white;
        toggleButtonStyle.fontStyle = FontStyle.Bold;
        
        // Change button color based on the enabled state
        toggleButtonStyle.normal.background = MakeColoredTexture(isExtensionEnabled ? _enabledButtonColor :_disabledButtonColor);

        if (GUILayout.Button(isExtensionEnabled ? _enabledText : _disabledText, toggleButtonStyle, GUILayout.Height(40)))
        {
            OnEnableButtonClick();
        }
    }

    private void OnEnableButtonClick()
    {
        isExtensionEnabled = !isExtensionEnabled;
        EditorPrefs.SetBool(ExtensionEnabledKey, isExtensionEnabled);
        if (UIElementSelector.IsOpen == true)
        {
            GetWindow<UIElementSelector>("UI Element Selector")?.Close();   
        }
        UIElementClickDetection.ExtensionEnabled(isExtensionEnabled);
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
    
    private static bool GetSavedBool(string key, bool defaultValue)
    {
        return EditorPrefs.GetBool(key, defaultValue);
    }
    
    private static string GetSavedString(string key, string defaultValue)
    {
        return EditorPrefs.GetString(key, defaultValue);
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
    
    public static string GetSelectedComponent()
    {
        return GetSavedString(SelectComponent, "RectTransform");
    }

    public static bool GetAutoCloseOnSelect()
    {
        return GetSavedBool(AutoCloseOnSelect, false);
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
    
    // Helper method to create a colored texture
    private Texture2D MakeColoredTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }
}
