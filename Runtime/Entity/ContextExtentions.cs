#if CUKU_ECS
using Entitas;
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
    public static class ContextExtentions
    {
        #region Context

        private static string contextInstanceMethodName = "get_Instance";
        private static string createEntityMethodName = "CreateEntity";
        private static string getEntitiesMethodName = "GetEntities";

        private static Type[] contextTypes;

        static ContextExtentions()
        {
            contextTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => typeof(IContext).IsAssignableFrom(type) && !type.IsInterface && !type.IsGenericTypeDefinition)
                .ToArray();
        }

        public static IContext[] Contexts()
        {
            var contexts = new IContext[contextTypes.Length];
            for (int i = 0; i < contextTypes.Length; i++)
            {
                contexts[i] = (IContext)contextTypes[i].Instance();
            }
            return contexts;
        }

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

        public static Type ContextType(this string context)
            => Array.Find(contextTypes, match => match.FullName == context);

        public static IContext ToContext(this string context)
            => context.ContextType().Instance() as IContext;

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

                // Get archtypes as components
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
        public static void CreateEntity(this IContext context, params IComponent[] components)
            => context.CreateEntity().AddComponents(components);

        /// <summary>
        /// Create <see cref="Entity"/> in <paramref name="context"/>
        /// and add <see cref="IComponent"/>s from <paramref name="componentIndices"/>.
        /// </summary>
        public static void CreateEntity(this IContext context, params int[] componentIndices)
            => context.CreateEntity().AddComponents(componentIndices);

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
#endif
