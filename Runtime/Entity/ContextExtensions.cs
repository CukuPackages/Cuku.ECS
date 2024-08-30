using Entitas;
using Entitas.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Cuku.ECS
{
    /// <summary>
    /// Provides <see cref="IContext"/> related utilities such as creating entities
    /// and serializing and deserializing contextTypes with entities and componentTypes.
    /// </summary>
    public static class ContextExtensions
    {
        #region Context

        private static string contextInstanceMethodName = "get_Instance";
        private static string createEntityMethodName = "CreateEntity";
        private static string getEntitiesMethodName = "GetEntities";

        private static Type[] contextTypes;

        static ContextExtensions()
        {
            contextTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => typeof(IContext).IsAssignableFrom(type) && !type.IsInterface && !type.IsGenericTypeDefinition)
                .ToArray();
        }

        /// <summary>
        /// Get all contexts
        /// </summary>
        public static IContext[] Contexts()
            => contextTypes.Select(type => (IContext)type.Instance()).ToArray();

        /// <summary>
        /// Get Context Instance from Context Type.
        /// </summary>
        public static object Instance(this Type contextType)
        {
            var context = Activator.CreateInstance(contextType);
            var instanceMethod = context.GetType().GetMethod(contextInstanceMethodName, BindingFlags.Static | BindingFlags.Public);
            return instanceMethod.Invoke(context, null);
        }

        /// <summary>
        /// Find contextType by name.
        /// </summary>
        public static Type ContextType(this ContextData data)
            => Array.Find(contextTypes, match => match.FullName == data.Context);

        /// <summary>
        /// Get Context type.
        /// </summary>
        public static Type ContextType(this string context)
            => Array.Find(contextTypes, match => match.FullName == context);

        /// <summary>
        /// Convert to <see cref="IContext"/>
        /// </summary>
        public static IContext ToContext(this string context)
        {
            if (!contextTypes.Any(type => type.Name == context))
            {
                UnityEngine.Debug.LogWarning($"\"{context}\" is not a vaild Context");
                return null;
            }
            return context.ContextType().Instance() as IContext;
        }

        /// <summary>
        /// Does context exist?
        /// </summary>
        public static bool Exists(this string context)
        {
            if (string.IsNullOrEmpty(context))
                return false;

            return Contexts().Any(existingContext => existingContext == context.ToContext());
        }

        public static Dictionary<string, IComponent[]> GetArchetypes()
        {
            var contextArchetypes = new Dictionary<string, IComponent[]>();
            foreach (var contextType in contextTypes)
            {
                // Get archetypes as indexes
                var archetypeIndexes = new HashSet<int[]>(new ArrayEqualityComparer<int>());
                var contextInstance = contextType.Instance();
                var getEntitiesMethod = contextType.GetEntitiesMethod();

                foreach (var entity in (Entity[])getEntitiesMethod.Invoke(contextInstance, null))
                {
                    archetypeIndexes.Add(entity.GetComponentIndexes());
                }

                // Get archtypes as Components
                var componentTypes = ((IContext)contextInstance).ContextInfo.ComponentTypes;
                var archteypeComponents = new IComponent[archetypeIndexes.Count];

                var archetypeCount = 0;
                foreach (var archetype in archetypeIndexes)
                {
                    var components = new IComponent[archetype.Length];
                    for (int i = 0; i < archetype.Length; i++)
                    {
                        components[i] = Activator.CreateInstance(componentTypes[archetype[i]]) as IComponent;
                    }
                    archteypeComponents = components;
                    archetypeCount++;
                }

                contextArchetypes.Add(contextType.Name, archteypeComponents);
            }

            return contextArchetypes;
        }

        public static void ContextObserverBehaviour(this IContext context)
        {
            bool exists = false;
            foreach (var observer in UnityEngine.Object.FindObjectsByType<ContextObserverBehaviour>(sortMode: UnityEngine.FindObjectsSortMode.None))
            {
                if (observer.Context.GetType() == context.GetType())
                    exists = true;
            }
            if (!exists) context.CreateContextObserver();
        }

        private static MethodInfo GetEntitiesMethod(this Type contextType, int parameters = 0)
            => Activator.CreateInstance(contextType)
                .GetType().GetMethods()
                .FirstOrDefault(m => m.Name == getEntitiesMethodName && m.GetParameters().Length == parameters);

        #endregion Context

        #region Entity

        /// <summary>
        /// Create <see cref="Entity"/> in <paramref name="context"/>.
        /// </summary>
        public static Entity CreateEntity(this IContext context)
            => (Entity)context.GetType().GetMethod(createEntityMethodName).Invoke(context, null);

        /// <summary>
        /// Create <see cref="Entity"/> in <paramref name="context"/>
        /// and add <paramref name="components"/>.
        /// </summary>
        public static Entity CreateEntity(this IContext context, params IComponent[] components)
        {
            var entity = context.CreateEntity();
            entity.AddComponents(components);
            return entity;
        }

        /// <summary>
        /// Create <see cref="Entity"/> in <paramref name="context"/>
        /// and add <see cref="IComponent"/>s from <paramref name="componentIndices"/>.
        /// </summary>
        public static Entity CreateEntity(this IContext context, params int[] componentIndices)
        {
            var entity = context.CreateEntity();
            entity.AddComponents(componentIndices);
            return entity;
        }

        /// <summary>
        /// Create entities from serialized <see cref="ContextData"/>.
        /// </summary>
        public static void CreateEntities(string data)
        {
            foreach (var contextData in data.DeserializeContexts())
                contextData.ContextType().CreateEntities(contextData.Entities);
        }

        /// <summary>
        /// Create <see cref="Entity"/> collection in <paramref name="context"/>.
        /// </summary>
        public static void CreateEntities(this Type context, params IComponent[][] entities)
        {
            var createdEntities = context.CreateEntities(entities.Length);
            for (int i = 0; i < createdEntities.Length; i++)
            {
                createdEntities[i].AddComponents(entities[i]);
            }
        }

        /// <summary>
        /// Create <see cref="Entity"/> collection in <paramref name="contextType"/>.
        /// </summary>
        public static Entity[] CreateEntities(this Type contextType, int count)
        {
            var entities = new Entity[count];

            var contextInstance = contextType.Instance();

            var context = Activator.CreateInstance(contextType);
            var createEntityMethod = context.GetType().GetMethod(createEntityMethodName);

            for (int i = 0; i < count; i++)
            {
                entities[i] = (Entity)createEntityMethod.Invoke(contextInstance, null);
            }
            return entities;
        }

        /// <summary>
        /// Get <see cref="Entity"/> colleciton in <paramref name="context"/>.
        /// </summary>
        public static Entity[] GetEntities(this IContext context)
            => (Entity[])context.GetType().GetEntitiesMethod().Invoke(context, null);

        #endregion Entity
    }
}
