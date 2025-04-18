using System.Collections.Generic;
using UIElementHighlighterTool.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[InitializeOnLoad]
public class UIElementHighlighterSettings : EditorWindow
{

    private Color _enabledButtonColor = new Color(0f, 0.33f, 0.11f);
    private Color _disabledButtonColor = new Color(0.33f, 0f, 0.02f);
    
    // Default values
    private static Color defaultFillColor = new Color(1, 1, 0, 0.25f);
    private static Color defaultOutlineColor = Color.yellow;
    private bool isExtensionEnabled;
    private int _ignoredLayerMask;
    
    private Texture2D logoTexture;

    // Keys for EditorPrefs
    private const string FillColorKey = "UIElementHighlighter_FillColor";
    private const string OutlineColorKey = "UIElementHighlighter_OutlineColor";
    private const string IgnoredLayerMaskKey = "UIElementHighlighter_IgnoredLayerMask";
    private const string IgnoredTagsKey = "UIElementHighlighter_IgnoredTags";
    private const string AutoCloseOnSelect = "UIElementHighlighter_AutoClose";
    private const string SelectComponent = "UIElementHighlighter_SelectComponent";
    private const string ExtensionEnabledKey = "UIElementHighlighter_Enabled";

    private const string _enabledText = "Extension is ENABLED \n Click to disable";
    private const string _disabledText = "Extension is DISABLED \n Click to enable";
    
    private bool _isListeningForShortcut = false;
    private KeyCode _currentShortcutKey;
    
    private List<string> _ignoredTags = new List<string>();
    

    [MenuItem("Tools/UI Element Highlighter/Highlighter Settings")]
    public static void ShowWindow()
    {
        GetWindow<UIElementHighlighterSettings>("Highlighter Settings");
    }

    private void OnEnable()
    {
        logoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(
            "Packages/com.kjub.uielementhighlighter/Editor/Assets/logo_name.png"
        );
    }

    private void OnGUI()
    {
        // Using EditorPrefs to get and set colors
        float logoHeight = CreateLogo();
        EditorGUI.BeginChangeCheck();
        
        isExtensionEnabled = EditorPrefs.GetBool(ExtensionEnabledKey, true);
        
        DrawToggleExtensionButton();
        
        // If the extension is disabled, draw a semi-transparent overlay
        if (isExtensionEnabled == false)
        {
            //logo height + button height
            EditorGUI.DrawRect(new Rect(0, logoHeight + 66, position.width, position.height), new Color(0, 0, 0, 0.5f));
            EditorGUI.BeginDisabledGroup(true);
        }

        DrawShortcutPart();
        
        GUILayout.Space(30f);
        
        // string selectComponent = EditorGUILayout.TextField("Select Component", GetSavedString(SelectComponent, "RectTransform"));
        bool autoCloseOnSelect = EditorGUILayout.Toggle("Auto Close On Select", UIElementHighlighterUtils.GetSavedBool(AutoCloseOnSelect, false));
        
        GUILayout.Space(10f);

        GUILayout.Label("Colors", UIElementHighlighterUtils.GetCenteredLabelStyle());
        Color fillColor = EditorGUILayout.ColorField("Fill Color", UIElementHighlighterUtils.GetSavedColor(FillColorKey, defaultFillColor));
        Color outlineColor = EditorGUILayout.ColorField("Outline Color", UIElementHighlighterUtils.GetSavedColor(OutlineColorKey, defaultOutlineColor));

        int ignoredLayerMask = DrawIgnoredLayersSection();
        DrawIgnoredTagsSection();
        
        if (isExtensionEnabled == false)
        {
            EditorGUI.EndDisabledGroup();
        }

        if (EditorGUI.EndChangeCheck())
        {
            UIElementHighlighterUtils.SaveColor(FillColorKey, fillColor);
            UIElementHighlighterUtils.SaveColor(OutlineColorKey, outlineColor);
            EditorPrefs.SetBool(AutoCloseOnSelect, autoCloseOnSelect);
            // EditorPrefs.SetString(SelectComponent, selectComponent);
            EditorPrefs.SetInt(IgnoredLayerMaskKey, ignoredLayerMask);
            EditorPrefs.SetString(IgnoredTagsKey, string.Join(",", _ignoredTags));
        }
    }

    private float CreateLogo()
    {
        GUILayout.Space(10f);
        float displayHeight = 0;

        if (logoTexture != null)
        {
            float maxWidth = 600f;
            float logoAspect = (float)logoTexture.height / logoTexture.width;
            float displayWidth = Mathf.Min(position.width, maxWidth);
            displayHeight = displayWidth * logoAspect;

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(logoTexture, GUILayout.Width(displayWidth), GUILayout.Height(displayHeight));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(10f);
        }

        return displayHeight;
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

    private void DrawShortcutPart()
    {
        GUILayout.Space(10f);
        GUILayout.Label("Shortcut", UIElementHighlighterUtils.GetCenteredLabelStyle());
        
        DrawShortcutButton();

        if (_isListeningForShortcut)
        {
            Event e = Event.current;
            
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
            {
                _isListeningForShortcut = false;
                Repaint();
                e.Use();
                return;
            }

            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.LeftControl || e.keyCode == KeyCode.RightControl ||
                    e.keyCode == KeyCode.LeftShift || e.keyCode == KeyCode.RightShift ||
                    e.keyCode == KeyCode.LeftAlt || e.keyCode == KeyCode.RightAlt)
                {
                    e.Use(); // keep listening
                }
                else
                {
                    UIElementHighlighterBinding newBinding = new UIElementHighlighterBinding
                    {
                        mainKey = e.keyCode,
                        mouseButton = -1,
                        ctrl = e.control || e.command,
                        shift = e.shift,
                        alt = e.alt
                    };

                    UIElementHighlighterUtils.SaveShortcut(newBinding);
                    _isListeningForShortcut = false;
                    Repaint();
                    e.Use();
                }
            }
            else if (e.type == EventType.MouseDown)
            {
                UIElementHighlighterBinding newBinding = new UIElementHighlighterBinding
                {
                    mainKey = KeyCode.None,
                    mouseButton = e.button,
                    ctrl = e.control || e.command,
                    shift = e.shift,
                    alt = e.alt
                };

                UIElementHighlighterUtils.SaveShortcut(newBinding);
                _isListeningForShortcut = false;
                Repaint();
                e.Use();
            }
        }

    }

    private void DrawShortcutButton()
    {
        UIElementHighlighterBinding current = UIElementHighlighterUtils.LoadShortcut();
        
        string buttonText;
        if (_isListeningForShortcut)
        {
            buttonText = "Press key or mouse button to set shortcut... (CTRL/SHIFT/ALT + Key or Mouse Button)";
        }
        else
        {
            buttonText = $"Click to change shortcut\n(Current: {current})";
        }
        
        if (_isListeningForShortcut == true)
        {
            GUI.enabled = false;
        }

        GUIStyle shortcutButtonStyle = new GUIStyle(GUI.skin.button);
        shortcutButtonStyle.wordWrap = true;
        shortcutButtonStyle.alignment = TextAnchor.MiddleCenter;
        shortcutButtonStyle.fontSize = 12;
        shortcutButtonStyle.fixedHeight = 40;
        
        if (GUILayout.Button(buttonText, shortcutButtonStyle, GUILayout.ExpandWidth(true)))
        {
            _isListeningForShortcut = true;
            GUI.FocusControl(null); // remove keyboard focus
        }
        
        GUI.enabled = true;
    }

    private int DrawIgnoredLayersSection()
    {
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        GUILayout.Label("Ignored Layers", UIElementHighlighterUtils.GetCenteredLabelStyle());
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
        GUILayout.Label("Ignored Tags", UIElementHighlighterUtils.GetCenteredLabelStyle(anchor: TextAnchor.LowerLeft));
        EditorGUI.indentLevel++;

        // Load ignored tags from EditorPrefs
        string savedIgnoredTags = EditorPrefs.GetString(IgnoredTagsKey, "");
        _ignoredTags = new List<string>(savedIgnoredTags.Split(','));

        foreach (string tag in InternalEditorUtility.tags)
        {
            bool isIgnored = _ignoredTags.Contains(tag);
            bool shouldBeIgnored = EditorGUILayout.Toggle(tag, isIgnored);
            if (shouldBeIgnored && !isIgnored)
            {
                _ignoredTags.Add(tag);
            }
            else if (!shouldBeIgnored && isIgnored)
            {
                _ignoredTags.Remove(tag);
            }
        }

        EditorGUI.indentLevel--;
    }
    
    public static Color GetFillColor()
    {
        return UIElementHighlighterUtils.GetSavedColor(FillColorKey, defaultFillColor);
    }

    public static Color GetOutlineColor()
    {
        return UIElementHighlighterUtils.GetSavedColor(OutlineColorKey, defaultOutlineColor);
    }
    
    public static string GetSelectedComponent()
    {
        return UIElementHighlighterUtils.GetSavedString(SelectComponent, "RectTransform");
    }

    public static bool GetAutoCloseOnSelect()
    {
        return UIElementHighlighterUtils.GetSavedBool(AutoCloseOnSelect, false);
    }
    
    public static int GetIgnoredLayerMask()
    {
        return EditorPrefs.GetInt(IgnoredLayerMaskKey, 0);
    }
    
    public static string GetIgnoredTags()
    {
        return EditorPrefs.GetString(IgnoredTagsKey, "");
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
