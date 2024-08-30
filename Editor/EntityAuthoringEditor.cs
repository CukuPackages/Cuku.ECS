using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Cuku.ECS
{
    [CustomEditor(typeof(EntityAuthoring))]
    public class EntityAuthoringEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var entity = (EntityAuthoring)target;

            var hiddenComponents = string.Empty;

            GUILayout.BeginVertical();

            if (!entity.Context.Exists()) // Context
            {
                EditorGUILayout.HelpBox("Context is not valid", MessageType.Error);
                hiddenComponents = nameof(entity.Components);
            }
            else // Components
            {
                GUILayout.Space(20);
                if (!string.IsNullOrEmpty(entity.Context))
                {
                    var nonContextComponents = new List<string>();

                    var duplicateComponents = new List<string>();
                    var componentTypeSet = new HashSet<System.Type>();

                    for (int i = 0; i < entity.Components.Length; i++)
                    {
                        var component = entity.Components[i];

                        // Check if component is part of the context
                        if (component == null ||
                            !entity.Context.ToContext().ContextInfo.ComponentTypes.Any(c => c == component.GetType()))
                            nonContextComponents.Add($"Element {i} - {component?.ToString()}");

                        // Check for duplicate components
                        if (component != null && !componentTypeSet.Add(component.GetType()))
                            duplicateComponents.Add($"Element {i} - {component?.ToString()}");
                    }

                    if (nonContextComponents.Count > 0)
                        EditorGUILayout.HelpBox($"Components are not defined in {entity.Context}:\n" +
                            $"{nonContextComponents.Aggregate((c, n) => $"{c}\n{n}")}",
                            MessageType.Error);

                    if (duplicateComponents.Count > 0)
                        EditorGUILayout.HelpBox($"There are duplicate components:\n" +
                            $"{duplicateComponents.Aggregate((c, n) => $"{c}\n{n}")}",
                            MessageType.Error);
                }
            }

            GUILayout.EndVertical();

            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script", hiddenComponents);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
