#if CUKU_ECS
using Entitas;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Cuku.Assets.Assets;

namespace Cuku.ECS
{
    /// <summary>
    /// Provides <see cref="IContext"/> related utilities such as creating entities
    /// and serializing and deserializing contexts with entities and components.
    /// </summary>
    public static class ContextExtentions
    {
        #region Context

        private static string contextInstanceMethod = "get_Instance";
        private static string contextCreateEntityMethod = "CreateEntity";
        private static string contextGetEntitiesMethod = "GetEntities";

        private static Type[] contexts;


        static ContextExtentions()
        {
            contexts = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => typeof(IContext).IsAssignableFrom(type) && !type.IsInterface)
                .ToArray();
        }

        /// <summary>
        /// Find contextType by name.
        /// </summary>
        public static Type GetContext(this ContextData data) => GetContext(data.Context);

        /// <summary>
        /// Find Context Type by name.
        /// </summary>
        public static Type GetContext(string context)
            => Array.Find(contexts, match => match.Name == context);

        #endregion Context

        #region Entity

        /// <summary>
        /// Create <see cref="Entity"/> in <paramref name="context"/>.
        /// </summary>
        public static Entity CreateEntity(this IContext context)
            => (Entity)context.GetType().GetMethod(contextCreateEntityMethod).Invoke(context, null);

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
            var context = Activator.CreateInstance(contextType);

            var instanceMethod = context.GetType().GetMethod(contextInstanceMethod, BindingFlags.Static | BindingFlags.Public);
            var contextInstance = instanceMethod.Invoke(context, null);

            var creationMethod = context.GetType().GetMethod(contextCreateEntityMethod);

            for (int i = 0; i < count; i++)
            {
                entities[i] = (Entity)creationMethod.Invoke(contextInstance, null);
            }
            return entities;
        }

        /// <summary>
        /// Get <see cref="Entity"/> colleciton in <paramref name="context"/>.
        /// </summary>
        public static Entity[] GetEntities(this IContext context)
            => (Entity[])context.GetType().GetMethod(contextGetEntitiesMethod).Invoke(context, null);

        #endregion Entity

        #region Serialization / Deserialization

        private static readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
        };

        /// <summary>
        /// Deserialize contexts from Json asset and create their entities.
        /// </summary>
        public static async void LoadEntitiesAsync(string key)
        {
            var asset = await key.LoadAsset<UnityEngine.TextAsset>();
            foreach (var contextData in DeserializeContexs(asset.Value.text))
            {
                contextData.GetContext().CreateEntities(contextData.Entities);
            }
            asset.Key.UnloadAsset();
        }

        /// <summary>
        /// Serialize all entities in <paramref name="contexts"/> to Json.
        /// </summary>
        /// <param name="formatting">Json formatting.</param>
        public static string SerializeContexts(Formatting formatting = Formatting.None, params IContext[] contexts)
        {
            var serializedContexts = new ContextData[contexts.Length];
            for (int i = 0; i < serializedContexts.Length; i++)
            {
                var context = contexts[i];
                var serializedContext = new ContextData();
                serializedContext.Context = context.GetType().Name;

                var entities = context.GetEntities();
                var serializedEntities = new List<IComponent[]>();

                for (int j = 0; j < entities.Length; j++)
                {
                    var components = new List<IComponent>();
                    foreach (var component in entities[j].GetComponents())
                    {
                        if (component != null && component.GetType().IsDefined(typeof(SerializableAttribute), false))
                            components.Add(component);
                    }

                    if (components.Count > 0)
                    {
                        serializedEntities.Add(components.ToArray());
                    }
                }

                serializedContext.Entities = serializedEntities.ToArray();
                serializedContexts[i] = serializedContext;
            }

            return SerializeContextsData(formatting, serializedContexts);
        }

        /// <summary>
        /// Serialize all entities in <paramref name="contextsData"/> to Json.
        /// </summary>
        /// <param name="formatting">Json formatting.</param>
        public static string SerializeContextsData(Formatting formatting = Formatting.None, params ContextData[] contextsData)
            => JsonConvert.SerializeObject(contextsData, formatting, serializerSettings);

        /// <summary>
        /// Deserialize <see cref="ContextData"/> from json asset.
        /// </summary>
        public static ContextData[] DeserializeContexs(string data)
            => JsonConvert.DeserializeObject<ContextData[]>(data, serializerSettings);

        #endregion Serialization / Deserialization
    }
}
#endif
