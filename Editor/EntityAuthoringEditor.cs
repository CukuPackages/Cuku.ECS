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
                    var invalidComponents = new List<string>();
                    for (int i = 0; i < entity.Components.Length; i++)
                    {
                        var component = entity.Components[i];
                        if (component == null ||
                            !entity.Context.ToContext().ContextInfo.ComponentTypes.Any(c => c == component.GetType()))
                            invalidComponents.Add($"Element {i} - {component?.ToString()}");
                    }
                    if (invalidComponents.Count > 0)
                        EditorGUILayout.HelpBox($"Components are not defined in {entity.Context}:\n" +
                            $"{invalidComponents.Aggregate((c, n) => $"{c}\n{n}")}",
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
