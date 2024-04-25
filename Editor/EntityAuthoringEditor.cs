using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Cuku.ECS
{
    [CustomEditor(typeof(EntityAuthoring))]
    public class EntityAuthoringEditor : Editor
    {
        private string[] contexts;

        private void OnEnable()
            => contexts = ContextExtensions.Contexts()
            .Select(context => context.ContextInfo.Name).ToArray();

        public override void OnInspectorGUI()
        {
            var entity = (EntityAuthoring)target;

            if (string.IsNullOrEmpty(entity.Context))
                EditorGUILayout.HelpBox("Select Context!", MessageType.Error);
            else
            {
                var invalidComponents = new List<string>();
                for (int i = 0; i < entity.Components.Length; i++)
                {
                    var component = entity.Components[i];
                    if (component == null ||
                        !entity.Context.ToContext().ContextInfo.ComponentTypes
                            .Any(c => c == component.GetType()))
                        invalidComponents.Add($"{i} - {component?.ToString().Split('.').Last()}");
                }
                if (invalidComponents.Count > 0)
                    EditorGUILayout.HelpBox($"Components are not defined in the selected Context:\n" +
                        $"{invalidComponents.Aggregate((c, n) => $"{c}\n{n}")}",
                        MessageType.Error);
            }

            GUILayout.BeginVertical();
            EditorGUILayout.LabelField(nameof(entity.Context), new GUIStyle(EditorStyles.label) { fontStyle = FontStyle.Bold });
            int selectedIndex = EditorGUILayout.Popup("", ContextIndex(entity.Context), contexts);
            GUILayout.Space(10);
            GUILayout.EndVertical();

            if (selectedIndex >= 0 && selectedIndex < contexts.Length)
                entity.Context = contexts[selectedIndex];

            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, new string[] { "m_Script", nameof(entity.Context) });
            serializedObject.ApplyModifiedProperties();
        }

        private int ContextIndex(string context)
        {
            for (int i = 0; i < contexts.Length; i++)
                if (contexts[i] == context)
                    return i;
            return -1;
        }
    }
}
