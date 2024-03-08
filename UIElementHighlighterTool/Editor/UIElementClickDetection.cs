using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[InitializeOnLoad]
public static class UIElementClickDetection
{
    public static List<RectTransform> allRectTransforms = new List<RectTransform>();
    public delegate void ElementsClickedHandler(List<RectTransform> clickedElements);
    public static event ElementsClickedHandler OnElementsClicked;
    
    private const string ExtensionEnabledKey = "UIElementHighlighter_Enabled";
    private static bool isRightMouseDown = false;
    private static bool hasDragged = false;

    static UIElementClickDetection()
    {
        bool isExtensionEnabled = EditorPrefs.GetBool(ExtensionEnabledKey, true);
        ExtensionEnabled(isExtensionEnabled);
    }

    public static void ExtensionEnabled(bool enabled)
    {
        if (enabled == true)
        {
            SceneView.duringSceneGui += OnSceneGUI;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            OnHierarchyChanged(); // Initial population
        }
        else
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
        }
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;
        
        if (e.type == EventType.MouseDown && e.button == 1)
        {
            isRightMouseDown = true;
            hasDragged = false;
        }
        else if (e.type == EventType.MouseDrag && isRightMouseDown)
        {
            hasDragged = true;
        }
        else if (e.type == EventType.MouseUp && e.button == 1)
        {
            if (isRightMouseDown && !hasDragged)
            {
                DetectElementUnderClick(e.mousePosition);
                // e.Use(); // Mark the event as used
            }
            
            isRightMouseDown = false;
            hasDragged = false;
        }
    }

    private static void DetectElementUnderClick(Vector2 eventMousePosition)
    {
        Vector2 mousePosition = eventMousePosition;
        mousePosition.y = SceneView.currentDrawingSceneView.camera.pixelHeight - mousePosition.y;

        List<RectTransform> hitUIElements = new List<RectTransform>();
        Camera sceneCamera = SceneView.currentDrawingSceneView.camera;

        foreach (RectTransform rectTransform in allRectTransforms)
        {
            Vector3[] worldCorners = new Vector3[4];
            rectTransform.GetWorldCorners(worldCorners);
            Vector2[] screenCorners = new Vector2[4];
            for (int i = 0; i < 4; i++)
            {
                screenCorners[i] = RectTransformUtility.WorldToScreenPoint(sceneCamera, worldCorners[i]);
            }

            Rect rect = Rect.MinMaxRect(screenCorners.Min(corner => corner.x), screenCorners.Min(corner => corner.y), screenCorners.Max(corner => corner.x), screenCorners.Max(corner => corner.y));
            if (rect.Contains(mousePosition))
            {
                hitUIElements.Add(rectTransform);
            }
        }

        if (hitUIElements.Any())
        {
            if (!UIElementSelector.IsOpen)
            {
                UIElementSelector.ShowWindow();
            }
            OnElementsClicked?.Invoke(hitUIElements);
        }
    }

    private static void OnHierarchyChanged()
    {
        allRectTransforms.Clear();
        Canvas[] canvases = Object.FindObjectsOfType<Canvas>(true);
        foreach (Canvas canvas in canvases)
        {
            allRectTransforms.AddRange(canvas.GetComponentsInChildren<RectTransform>(true));
        }
        
        // In case of being in a prefab stage, attempt to find RectTransforms in the prefab.
        var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
        if (prefabStage != null)
        {
            var rectTransformsInPrefab = prefabStage.prefabContentsRoot.GetComponentsInChildren<RectTransform>(true);
            allRectTransforms.AddRange(rectTransformsInPrefab);
        }
    }
}