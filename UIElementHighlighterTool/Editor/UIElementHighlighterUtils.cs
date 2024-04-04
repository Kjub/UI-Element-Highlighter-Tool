using System;
using UnityEngine;

namespace UIElementHighlighterTool.Editor
{
    public static class UIElementHighlighterUtils
    {
        public static Type GetTypeFromName(string typeName)
        {
            Type type = Type.GetType(typeName);

            if (type != null) return type;

            // Handling common Unity namespaces as a fallback
            string[] commonUnityNamespaces = new string[]
            {
                "UnityEngine.",
                "UnityEngine.UI."
            };

            foreach (string ns in commonUnityNamespaces)
            {
                type = Type.GetType(ns + typeName + ", UnityEngine");
                if (type != null) return type;

                type = Type.GetType(ns + typeName + ", UnityEngine.UI");
                if (type != null) return type;
            }

            // If not found, log an error
            Debug.LogError($"Type not found for component name: {typeName}");
            return null;
        }
    }
}