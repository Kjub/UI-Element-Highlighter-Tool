using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class UIElementSelector : EditorWindow
{

    private Vector2 settingsScrollPosition;
    
    
    private static List<RectTransform> uiElements = new List<RectTransform>();
    private static RectTransform _hoveredElement;
    private Vector2 scrollPosition;
    
    private bool listUpdated = true; // Initially set to true to draw the first time
    private GUIStyle leftAlignedButtonStyle;

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

    [MenuItem("Tools/UI-EHT/Element Selector")]
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
        InitializeLeftAlignedButtonStyle();
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
        uiElements.Clear();
    }

    private static void OnPrefabStageClosing(PrefabStage stage)
    {
        uiElements.Clear();
    }
    
    private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        uiElements.Clear();
    }

    private void InitializeLeftAlignedButtonStyle()
    {
        leftAlignedButtonStyle = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleLeft };
    }

    private void PlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode || state == PlayModeStateChange.ExitingPlayMode)
        {
            uiElements.Clear();
            listUpdated = true; // Ensure redraw after clearing
            Repaint();
        }
    }

    private void UpdateUIElementsList(IEnumerable<RectTransform> elements)
    {
        uiElements.Clear(); // Clear existing items
        
        // Load settings from EditorPrefs
        int ignoredLayerMask = EditorPrefs.GetInt(UIElementHighlighterSettings.IgnoredLayerMaskKey, 0);
        string[] ignoredTags = EditorPrefs.GetString(UIElementHighlighterSettings.IgnoredTagsKey, "").Split(',');


        foreach (RectTransform element in elements)
        {
            GameObject go = element.gameObject;
            // Skip elements on ignored layers or with ignored tags
            if (((1 << go.layer) & ignoredLayerMask) != 0 || ignoredTags.Contains(go.tag))
            {
                continue;
            }

            // If not ignored, add to the list
            uiElements.Add(element);
        }

        listUpdated = true; // Indicate that the list has been updated
        Repaint(); // Request the window to repaint
    }

    private void OnGUI()
    {
        AddHeader();

        AddElements();
        
        AddSettingsButton();
    }

    private void AddHeader()
    {
        // Centered label "Hierarchy"
        GUIStyle centeredStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold};
        EditorGUILayout.LabelField("Hierarchy", centeredStyle);
    }

    private void AddElements()
    {
        // Add some space after the label before the list starts
        EditorGUILayout.Space();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        bool anyHoverDetectedThisFrame = false;
        foreach (RectTransform rectTransform in uiElements)
        {
            anyHoverDetectedThisFrame |= DrawElement(rectTransform); // Draw each element and check for hover
        }

        EditorGUILayout.EndScrollView();

        if (Event.current.type == EventType.Repaint && !anyHoverDetectedThisFrame)
        {
            hoveredElement = null; // Reset hovered element if no hover detected
        }
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
        int depth = CalculateDepth(rectTransform) - 1;

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(20 * depth);
        Rect buttonRect = GUILayoutUtility.GetRect(new GUIContent(rectTransform.name), leftAlignedButtonStyle, GUILayout.Height(20));

        if (GUI.Button(buttonRect, rectTransform.name, leftAlignedButtonStyle))
        {
            Selection.activeGameObject = rectTransform.gameObject;
        }

        EditorGUILayout.EndHorizontal();
        
        if (Event.current.type == EventType.Repaint)
        {
            Rect lastRect = GUILayoutUtility.GetLastRect();
            DrawHierarchyLines(depth, lastRect);
        }

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
