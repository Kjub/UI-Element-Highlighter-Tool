using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class UIElementSelector : EditorWindow
{
    
    private const string ShowOnlyEnabledPrefKey = "UIElementHighlighter_ShowEnabled";

    private Vector2 _settingsScrollPosition;
    private static List<RectTransform> _uiElements = new List<RectTransform>();
    private static RectTransform _hoveredElement;
    private Vector2 scrollPosition;
    
    private bool _listUpdated = true; // Initially set to true to draw the first time
    private GUIStyle _leftAlignedButtonStyle;
    
    private bool _showOnlyEnabled = false; // Default to showing all elements
    private bool _isInitialized;

    public static bool IsOpen
    {
        get { return Resources.FindObjectsOfTypeAll<UIElementSelector>().Length > 0; }
    }

    public static RectTransform hoveredElement
    {
        get => _hoveredElement;
        private set
        {
            if (_hoveredElement != value)
            {
                _hoveredElement = value;
                SceneView.RepaintAll(); // Ensure the Scene View is updated when hovered element changes
            }
        }
    }

    [MenuItem("Tools/UI Element Highlighter/Element Selector")]
    public static void ShowWindow()
    {
        GetWindow<UIElementSelector>("UI Element Selector").Show();
        
        PrefabStage.prefabStageOpened += OnPrefabStageOpened;
        PrefabStage.prefabStageClosing += OnPrefabStageClosing;
        EditorSceneManager.sceneOpened += OnSceneOpened;
    }

    private void OnEnable()
    {
        EditorApplication.playModeStateChanged += PlayModeStateChanged;
        UIElementClickDetection.OnElementsClicked += UpdateUIElementsList;
    }

    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= PlayModeStateChanged;
        UIElementClickDetection.OnElementsClicked -= UpdateUIElementsList;
    }
    
    private void OnDestroy()
    {
        PrefabStage.prefabStageOpened -= OnPrefabStageOpened;
        PrefabStage.prefabStageClosing -= OnPrefabStageClosing;
        EditorSceneManager.sceneOpened -= OnSceneOpened;
    }

    private static void OnPrefabStageOpened(PrefabStage stage)
    {
        _uiElements.Clear();
    }

    private static void OnPrefabStageClosing(PrefabStage stage)
    {
        _uiElements.Clear();
    }
    
    private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        _uiElements.Clear();
    }

    private void InitializeLeftAlignedButtonStyle()
    {
        _leftAlignedButtonStyle = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleLeft, richText = true};
    }

    private void PlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode || state == PlayModeStateChange.ExitingPlayMode)
        {
            _uiElements.Clear();
            _listUpdated = true; // Ensure redraw after clearing
            Repaint();
        }
    }

    private void UpdateUIElementsList(IEnumerable<RectTransform> elements)
    {
        _uiElements.Clear(); // Clear existing items
        
        // Load settings from EditorPrefs
        int ignoredLayerMask = UIElementHighlighterSettings.GetIgnoredLayerMask();
        string[] ignoredTags = UIElementHighlighterSettings.GetIgnoredTags().Split(',');
        
        foreach (RectTransform element in elements)
        {
            if (element == null)
            {
                continue;
            }
            
            GameObject go = element.gameObject;
            // Skip elements on ignored layers or with ignored tags
            if (((1 << go.layer) & ignoredLayerMask) != 0 || ignoredTags.Contains(go.tag))
            {
                continue;
            }

            // If not ignored, add to the list
            _uiElements.Add(element);
        }

        _listUpdated = true; // Indicate that the list has been updated
        Repaint(); // Request the window to repaint
    }

    private void OnGUI()
    {
        if (!_isInitialized)
        {
            InitializeLeftAlignedButtonStyle();
            _showOnlyEnabled = EditorPrefs.GetBool(ShowOnlyEnabledPrefKey, false);
            _isInitialized = true;
        }
        
        AddHeader();

        AddElements();
        
        AddSettingsButton();
    }

    private void AddHeader()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace(); // This centers the following content.
        GUIStyle centeredStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
        EditorGUILayout.LabelField("Hierarchy", centeredStyle, GUILayout.ExpandWidth(false)); // False allows it to stay centered.
        GUILayout.FlexibleSpace(); // Add flexible space on both sides to keep the label centered.

        AddShowOnlyEnabledToggle();
        
        EditorGUILayout.EndHorizontal();
    }

    private void AddShowOnlyEnabledToggle()
    {
        EditorGUI.BeginChangeCheck();
        _showOnlyEnabled = EditorGUILayout.ToggleLeft("Show only active elements", _showOnlyEnabled, GUILayout.Width(200));
        if (EditorGUI.EndChangeCheck())
        {
            // Save the new toggle state
            EditorPrefs.SetBool(ShowOnlyEnabledPrefKey, _showOnlyEnabled);
        }
    }

    private void AddElements()
    {
        // Add some space after the label before the list starts
        EditorGUILayout.Space();
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        bool anyHoverDetectedThisFrame = false;
        bool foundRemovedElements = false;
        foreach (RectTransform rectTransform in _uiElements)
        {
            if (rectTransform == null)
            {
                foundRemovedElements = true;
                continue;
            }
            
            if (_showOnlyEnabled && IsEnabledIncludingParents(rectTransform.gameObject) == false)
            {
                // If we're only showing enabled elements, skip disabled ones
                continue;
            }

            anyHoverDetectedThisFrame |= DrawElement(rectTransform); // Draw each element and check for hover
        }

        if (foundRemovedElements == true)
        {
            _uiElements.RemoveAll(rectTransform => rectTransform == null);
        }

        EditorGUILayout.EndScrollView();

        if (Event.current.type == EventType.Repaint && !anyHoverDetectedThisFrame)
        {
            hoveredElement = null; // Reset hovered element if no hover detected
        }
    }
    
    private bool IsEnabledIncludingParents(GameObject gameObject)
    {
        // If the GameObject itself is not active, return false immediately
        if (!gameObject.activeInHierarchy) return false;

        // Recursively check the parent
        Transform parent = gameObject.transform.parent;
        while (parent != null)
        {
            if (!parent.gameObject.activeInHierarchy) return false;
            parent = parent.parent;
        }

        // If we've made it here, the GameObject and all its parents are active
        return true;
    }

    private void AddSettingsButton()
    {
        GUILayout.FlexibleSpace();
        // Settings button
        if (GUILayout.Button("Open Highlighter Settings", GUILayout.Height(25)))
        {
            UIElementHighlighterSettings.ShowWindow();
        }
    }

    private bool DrawElement(RectTransform rectTransform)
    {
        int depth = Mathf.Max(0, CalculateDepth(rectTransform) - 1);
        bool elementIsEnabled = rectTransform.gameObject.activeSelf; // Get the enabled state of the GameObject

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(20 * depth);
        string statusText = elementIsEnabled ? "Active" : "Inactive";
        string buttonText = $"{rectTransform.name} - <b><color={(elementIsEnabled ? "green" : "red")}>{statusText}</color></b>";
        
        if (GUILayout.Button(buttonText, _leftAlignedButtonStyle))
        {
            Selection.activeGameObject = rectTransform.gameObject;
            if (UIElementHighlighterSettings.GetAutoCloseOnSelect() == true)
            {
                EditorGUILayout.EndHorizontal();
                Close();
                return false;
            }
        }
        
        EditorGUILayout.EndHorizontal();
        
        if (Event.current.type == EventType.Repaint)
        {
            Rect lastRect = GUILayoutUtility.GetLastRect();
            DrawHierarchyLines(depth, lastRect);
        }

        Rect buttonRect = GUILayoutUtility.GetLastRect();
        // Check for hover
        if (buttonRect.Contains(Event.current.mousePosition))
        {
            hoveredElement = rectTransform;
            return true; // Hover detected
        }

        return false; // No hover detected
    }
    
    private void DrawHierarchyLines(int depth, Rect rect)
    {
        Texture2D lineTexture = new Texture2D(1, 1);
        lineTexture.SetPixel(0, 0, Color.gray);
        lineTexture.Apply();

        for (int i = 0; i < depth; i++)
        {
            float indent = 20 * i + 10; // Adjust based on your indentation logic
            GUI.DrawTexture(new Rect(indent, rect.yMin, 2, rect.height), lineTexture);
            if (i == depth - 1)
            {
                // Draw horizontal line to the text
                GUI.DrawTexture(new Rect(indent, rect.yMin + rect.height / 2, 10, 2), lineTexture);
            }
        }
    }

    private int CalculateDepth(Transform element)
    {
        int depth = 0;
        while (element.parent != null)
        {
            depth++;
            element = element.parent;
        }
        return depth;
    }
    
}
