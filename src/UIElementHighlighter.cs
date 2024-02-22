using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class UIElementHighlighter
{
    static UIElementHighlighter()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        // Check if the UIElementSelector window is open. If not, return early.
        if (!UIElementSelector.IsOpen)
        {
            return;
        }

        // Proceed with drawing only if the UIElementSelector window is open.
        if (UIElementSelector.hoveredElement != null)
        {
            DrawBorder(UIElementSelector.hoveredElement);
        }
    }

    private static void DrawBorder(RectTransform rectTransform)
    {
        if (rectTransform == null) return;

        // Convert rectTransform corners to world space
        Vector3[] worldCorners = new Vector3[4];
        rectTransform.GetWorldCorners(worldCorners);
        
        // Use Handles to draw a solid rectangle with an outline
        Handles.DrawSolidRectangleWithOutline(worldCorners, UIElementSelector.FillColor, UIElementSelector.OutlineColor);
    }
}