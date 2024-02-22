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

    static UIElementClickDetection()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        EditorApplication.hierarchyChanged += OnHierarchyChanged;
        OnHierarchyChanged(); // Initial population
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;

        if (e.type == EventType.MouseDown && e.button == 1) // Right-click
        {
            Vector2 mousePosition = e.mousePosition;
            mousePosition.y = SceneView.currentDrawingSceneView.camera.pixelHeight - mousePosition.y;

            List<RectTransform> hitUIElements = new List<RectTransform>();
            Camera sceneCamera = SceneView.currentDrawingSceneView.camera;

            foreach (var rectTransform in allRectTransforms)
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

            e.Use();
        }
    }

// Utility method to check if a point is inside a rectangle
private static bool IsPointInRectangle(Vector2 point, Vector2[] rectangleCorners)
{
    // Assuming rectangleCorners are [bottomLeft, topLeft, topRight, bottomRight]
    var edge1 = rectangleCorners[1] - rectangleCorners[0];
    var edge2 = rectangleCorners[3] - rectangleCorners[0];
    var pointRelativeToBottomLeft = point - rectangleCorners[0];

    float dot1 = Vector2.Dot(pointRelativeToBottomLeft, edge1);
    float dot2 = Vector2.Dot(pointRelativeToBottomLeft, edge2);

    return (dot1 >= 0 && dot1 <= edge1.sqrMagnitude && dot2 >= 0 && dot2 <= edge2.sqrMagnitude);
}

    private static IEnumerable<RectTransform> GetAllRectTransforms()
    {
        if (allRectTransforms.Count == 0)
        {
            OnHierarchyChanged();
        }

        return allRectTransforms;
    }

    private static void OnHierarchyChanged()
    {
        allRectTransforms.Clear();
        Canvas[] canvases = Object.FindObjectsOfType<Canvas>(true);
        foreach (var canvas in canvases)
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