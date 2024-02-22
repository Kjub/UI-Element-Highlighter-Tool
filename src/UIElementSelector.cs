using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class UIElementSelector : EditorWindow
{
    private static List<RectTransform> uiElements = new List<RectTransform>();
    private static RectTransform _hoveredElement;
    private Vector2 scrollPosition;
    
    private bool listUpdated = true; // Initially set to true to draw the first time
    private GUIStyle leftAlignedButtonStyle;
    
    private static Color fillColor = new Color(1, 1, 0, 0.25f); // Default semi-transparent yellow
    private static Color outlineColor = Color.yellow; // Default solid yellow
    
    public static Color FillColor { get => fillColor; set => fillColor = value; }
    public static Color OutlineColor { get => outlineColor; set => outlineColor = value; }
    
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

    [MenuItem("Tools/UI Element Selector")]
    public static void ShowWindow()
    {
        GetWindow<UIElementSelector>("UI Element Selector").Show();
    }

    public static void ShowAndRefresh(List<RectTransform> elements)
    {
        var window = GetWindow<UIElementSelector>("UI Element Selector", true);
        window.UpdateUIElementsList(elements);
        window.Show();
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

    private void UpdateUIElementsList(List<RectTransform> elements)
    {
        uiElements = elements;
        listUpdated = true; // Mark that the list has been updated
        Repaint(); // Request a repaint which will trigger OnGUI
    }

    private void OnGUI()
    {
        // Color pickers for customization
        EditorGUILayout.Space(); // Add some space at the start
        EditorGUILayout.LabelField("Highlight Colors", EditorStyles.boldLabel);
        fillColor = EditorGUILayout.ColorField("Fill Color", fillColor);
        outlineColor = EditorGUILayout.ColorField("Outline Color", outlineColor);

        // Add a bit more space between the color pickers and the list
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        // Centered label "Hierarchy"
        GUIStyle centeredStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
        EditorGUILayout.LabelField("Hierarchy", centeredStyle);

        // Add some space after the label before the list starts
        EditorGUILayout.Space();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        bool anyHoverDetectedThisFrame = false;
        foreach (var rectTransform in uiElements)
        {
            anyHoverDetectedThisFrame |= DrawElement(rectTransform); // Draw each element and check for hover
        }

        EditorGUILayout.EndScrollView();

        if (Event.current.type == EventType.Repaint && !anyHoverDetectedThisFrame)
        {
            hoveredElement = null; // Reset hovered element if no hover detected
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
