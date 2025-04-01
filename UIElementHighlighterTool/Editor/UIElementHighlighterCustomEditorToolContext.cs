// using System;
// using UnityEditor;
// using UnityEditor.Actions;
// using UnityEditor.EditorTools;
// using UnityEngine.UIElements;
//
// [EditorToolContext("Custom Editor Tool Context")]
// public class UIElementHighlighterCustomEditorToolContext : EditorToolContext
// {
//     public override void OnToolGUI(EditorWindow _) { ... }
//     protected override Type GetEditorToolType(Tool tool) { ... }
//
//     public override void PopulateMenu(DropdownMenu menu)
//     {
//         ContextMenuUtility.AddClipboardEntriesTo(menu);
//     }
// }