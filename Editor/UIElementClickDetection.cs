﻿using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UIElementHighlighterTool.Editor;
using Object = UnityEngine.Object;

[InitializeOnLoad]
public static class UIElementClickDetection
{
    public static List<RectTransform> allRectTransforms = new List<RectTransform>();
    public delegate void ElementsClickedHandler(List<RectTransform> clickedElements);
    public static event ElementsClickedHandler OnElementsClicked;
    
    private const string ExtensionEnabledKey = "UIElementHighlighter_Enabled";
    
    private static Vector2 _mousePosition = new Vector2();
    private static Camera _sceneDrawingCamera = null;

    private static bool IsEnabled = false;

    static UIElementClickDetection()
    {
        IsEnabled = EditorPrefs.GetBool(ExtensionEnabledKey, true);
        ExtensionEnabled(IsEnabled);
    }

    public static void ExtensionEnabled(bool enabled)
    {
        IsEnabled = enabled;
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
        UIElementHighlighterBinding binding = UIElementHighlighterUtils.LoadShortcut();

        Event e = Event.current;
        _mousePosition = e.mousePosition;
        _sceneDrawingCamera = sceneView.camera;

        bool modifierMatch =
            binding.ctrl == (e.control || e.command) &&
            binding.shift == e.shift &&
            binding.alt == e.alt;

        if (binding.IsMouse)
        {
            if (e.type == EventType.MouseDown && e.button == binding.mouseButton && modifierMatch)
            {
                DetectElementUnderClick(_mousePosition);
                e.Use();
            }
        }
        else
        {
            if (e.type == EventType.KeyDown && e.keyCode == binding.mainKey && modifierMatch)
            {
                DetectElementUnderClick(_mousePosition);
                e.Use();
            }
        }

    }
    
    [MenuItem("CONTEXT/GameObjectToolContext/UI Highlight")]
    static void OnDetectClick()
    {
        DetectElementUnderClick(_mousePosition);
    }
    
    [MenuItem("CONTEXT/GameObjectToolContext/UI Highlight", true)]
    static bool OnDetectClickValidate()
    {
        return IsEnabled;
    }


    private static void DetectElementUnderClick(Vector2 eventMousePosition)
    {
        Vector2 mousePosition = eventMousePosition;
        mousePosition.y = _sceneDrawingCamera.pixelHeight - mousePosition.y;

        List<RectTransform> hitUIElements = new List<RectTransform>();
        
        string selectedComponent = UIElementHighlighterSettings.GetSelectedComponent();
        Type selectedComponentType = UIElementHighlighterUtils.GetTypeFromName(selectedComponent);

        if (selectedComponentType == null)
        {
            selectedComponentType = typeof(RectTransform);
        }

        foreach (RectTransform rectTransform in allRectTransforms)
        {
            if (rectTransform.GetComponent(selectedComponentType) == null)
            {
                continue;
            }
            
            Vector3[] worldCorners = new Vector3[4];
            rectTransform.GetWorldCorners(worldCorners);
            Vector2[] screenCorners = new Vector2[4];
            for (int i = 0; i < 4; i++)
            {
                screenCorners[i] = RectTransformUtility.WorldToScreenPoint(_sceneDrawingCamera, worldCorners[i]);
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
        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.InstanceID);
        foreach (Canvas canvas in canvases)
        {
            allRectTransforms.AddRange(canvas.GetComponentsInChildren<RectTransform>(true));
        }
        
        // In case of being in a prefab stage, attempt to find RectTransforms in the prefab.
        PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
        if (prefabStage != null)
        {
            RectTransform[] rectTransformsInPrefab = prefabStage.prefabContentsRoot.GetComponentsInChildren<RectTransform>(true);
            allRectTransforms.AddRange(rectTransformsInPrefab);
        }
    }
}