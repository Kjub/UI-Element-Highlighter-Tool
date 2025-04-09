using UnityEditor;
using UnityEngine;

namespace UIElementHighlighterTool.Editor
{
    public class UIElementHighlighterWelcome : EditorWindow
    {
        private const string WelcomeShownKey = "UIElementHighlighter_WelcomeShown";
        private Texture2D _welcomeLogo;

        [InitializeOnLoadMethod]
        private static void ShowOnFirstInstall()
        {
            if (!EditorPrefs.GetBool(WelcomeShownKey, false))
            {
                EditorApplication.update += OpenOnStartup;
            }
        }

        private static void OpenOnStartup()
        {
            EditorApplication.update -= OpenOnStartup;
            ShowManually();
            EditorPrefs.SetBool(WelcomeShownKey, true);
        }
        
        private static void ShowManually()
        {
            UIElementHighlighterWelcome window = GetWindow<UIElementHighlighterWelcome>("Welcome");
            window.minSize = new Vector2(300, 200);
            window.maxSize = new Vector2(450, 500);
            window.position = new Rect(
                (Screen.currentResolution.width - window.minSize.x) / 2,
                (Screen.currentResolution.height - window.minSize.y) / 2,
                window.maxSize.x,
                window.maxSize.y
            );
        }

        private void OnEnable()
        {
            _welcomeLogo = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Packages/com.kjub.uielementhighlighter/Editor/Assets/logo512x512.png"
            );

            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnSceneGUI(SceneView obj)
        {
            Event currentEvent = Event.current;
            if (currentEvent.type == EventType.KeyDown && currentEvent.keyCode == KeyCode.KeypadMinus)
            {
                ShowManually();
            }
        }

        private void OnGUI()
        {
            GUILayout.Space(10);

            // Draw logo (square, centered)
            if (_welcomeLogo != null)
            {
                float logoSize = Mathf.Min(position.width * 0.5f, 256f); // Max 256px width
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label(_welcomeLogo, GUILayout.Width(logoSize), GUILayout.Height(logoSize));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.Space(10);
            }

            // Welcome Text
            GUILayout.Label("👋 Welcome to UI Element Highlighter", UIElementHighlighterUtils.GetCenteredLabelStyle(16));
            GUILayout.Space(10);
            GUIStyle wrapStyle = new GUIStyle(EditorStyles.label);
            wrapStyle.wordWrap = true;
            wrapStyle.richText = true;
            wrapStyle.fontSize = 14;


            GUILayout.Label(
                $"This tool lets you detect, highlight and search through overlapping UI elements by pressing <color=\"green\">{UIElementHighlighterUtils.LoadShortcut()}</color> on top of UI element in the Scene.\n\n" +
                $"You can customize the shortcut, colors and ignored layers/tags in the Settings window.",
                wrapStyle,
                GUILayout.ExpandWidth(true)
            );
            GUILayout.Space(20);

#if UNITY_6000_0_OR_NEWER
            
            EditorGUILayout.HelpBox("In Unity6 and above, you can right click in scene view and select \"UI Highlight\" to highlight elements too.", MessageType.Info);
            
#endif

            GUILayout.FlexibleSpace();

            // Open Settings button
            if (GUILayout.Button("Open Settings", GUILayout.Height(40)))
            {
                UIElementHighlighterSettings.ShowWindow();
                Close();
            }
        }
    }
}
