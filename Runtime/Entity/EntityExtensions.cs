using Entitas;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cuku.ECS
{
    /// <summary>
    /// Provides <see cref="Entity"/> related utilities such as
    /// adding, removing and replacing Components.
    /// </summary>
    public static class EntityExtensions
    {
        public static bool HasComponent(this Entity entity, IComponent component)
            => entity.HasComponent(Array.IndexOf(entity.ContextInfo.ComponentTypes, component.GetType()));

        public static bool HasAnyComponent(this Entity entity, IComponent[] components)
        {
            foreach (var layer in components)
                if (entity.HasComponent(layer))
                    return true;
            return false;
        }

        public static IComponent GetAnyComponent(this Entity entity, IComponent[] components)
        {
            foreach (var component in components)
                if (entity.HasComponent(component))
                    return component;
            return default;
        }

        public static IComponent GetComponent(string name)
        {
            foreach (var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()))
                if (typeof(IComponent).IsAssignableFrom(type) && !type.IsInterface && type.Name == name)
                    return (IComponent)Activator.CreateInstance(type);
            return null; // MUST NEVER HAPPEN!
        }

        /// <summary>
        /// Add <paramref name="components"/> to <paramref name="entity"/>.
        /// </summary>
        public static void AddComponents(this Entity entity, params IComponent[] components)
        {
            var componentTypes = entity.ContextInfo.ComponentTypes;
            for (int i = 0; i < components.Length; i++)
            {
                var component = components[i];
                var index = Array.IndexOf(componentTypes, component.GetType());
                if (!entity.HasComponent(index))
                    entity.AddComponent(index, component);
            }
        }

        /// <summary>
        /// Add Components matched by <paramref name="indices"/> to <paramref name="entity"/>.
        /// </summary>
        /// <param name="indices">Component indices in the <paramref name="ComponentTypes"/>.</param>
        public static void AddComponents(this Entity entity, int[] indices)
        {
            var componentTypes = entity.ContextInfo.ComponentTypes;
            for (int i = 0; i < indices.Length; i++)
            {
                var index = indices[i];
                var component = entity.CreateComponent(index, componentTypes[index]);
                if (!entity.HasComponent(index))
                    entity.AddComponent(index, component);
            }
        }

        /// <summary>
        /// Remove <paramref name="components"/> to <paramref name="entity"/>.
        /// </summary>
        public static void RemoveComponents(this Entity entity, params IComponent[] components)
        {
            var componentTypes = entity.ContextInfo.ComponentTypes;
            for (int i = 0; i < components.Length; i++)
            {
                var component = components[i];
                var index = Array.IndexOf(componentTypes, component.GetType());
                if (entity.HasComponent(index))
                    entity.RemoveComponent(index);
            }
        }

        /// <summary>
        /// Replace <paramref name="components"/> of <paramref name="entity"/>.
        /// </summary>
        public static void ReplaceComponents(this Entity entity, params IComponent[] components)
        {
            var componentTypes = entity.ContextInfo.ComponentTypes;
            for (int i = 0; i < components.Length; i++)
            {
                var component = components[i];
                var index = Array.IndexOf(componentTypes, component.GetType());
                if (!entity.HasComponent(index))
                    entity.ReplaceComponent(index, component);
            }
        }

        /// <summary>
        /// Subtract Components with <paramref name="indices"/> from <paramref name="entities"/>.
        /// </summary>
        public static List<IComponent> SubtractComponents(this Entity[] entities, params int[] indices)
        {
            var remainderComponents = new List<IComponent>();
            for (int i = 0; i < entities.Length; i++)
                remainderComponents.AddRange(entities[i].SubtractComponents(indices));
            return remainderComponents;
        }

        /// <summary>
        /// Subtract Components with <paramref name="indices"/> from <paramref name="entity"/>.
        /// </summary>
        public static IComponent[] SubtractComponents(this Entity entity, params int[] indices)
        {
            var componentIndices = entity.GetComponentIndexes();
            var remainderIndices = new List<int>();
            for (var i = 0; i < componentIndices.Length; i++)
            {
                var entityIndex = componentIndices[i];
                var found = false;
                for (var j = 0; j < indices.Length; j++)
                    if (entityIndex == indices[j])
                    {
                        found = true;
                        break;
                    }
                if (!found)
                    remainderIndices.Add(entityIndex);
            }

            var remainderComponents = new IComponent[remainderIndices.Count];
            for (int i = 0; i < remainderIndices.Count; i++)
            {
                var index = remainderIndices[i];
                remainderComponents[i] = entity.GetComponent(index);
            }

            return remainderComponents;
        }

        public static IComponent[][] ToComponents(this Entity[] entities)
            => entities.Select(entity => entity.GetComponents()).ToArray();

        public static int[] Indices(this Entity entity, params IComponent[] components)
        {
            var indices = new List<int>();
            var componentTypes = entity.ContextInfo.ComponentTypes;
            foreach (var component in components)
                indices.Add(Array.IndexOf(componentTypes, component.GetType()));
            return indices.ToArray();
        }

        public static string Name(this IComponent Component)
            => Component.GetType().Name.Replace(nameof(Component), "");
    }
}
