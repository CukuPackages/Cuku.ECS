using Cuku.Utilities;
using Entitas;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Cuku.ECS
{
    public static class Serialization
    {
        private static readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
        };

        /// <summary>
        /// Deserialize context types from Json asset and create their entities.
        /// </summary>
        public static async void LoadEntities(this string key)
        {
            foreach (var context in (await key.LoadTextAsync()).DeserializeContexts())
                context.CreateEntities();
        }

        /// <summary>
        /// Create entities from serialized <see cref="ContextData"/>.
        /// </summary>
        public static void CreateEntities(this string data)
        {
            foreach (var context in data.DeserializeContexts())
                context.CreateEntities();
        }

        public static string SerializeEntities(this IContext context, Formatting formatting = Formatting.None, params IComponent[][] entities)
            => SerializeContextsData(formatting,
                new ContextData()
                {
                    Context = context.GetType().Name,
                    Entities = entities
                });

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
                        if (component != null && component.GetType().IsDefined(typeof(SerializableAttribute), false))
                            components.Add(component);

                    if (components.Count > 0)
                        serializedEntities.Add(components.ToArray());
                }

                serializedContext.Entities = serializedEntities.ToArray();
                serializedContexts[i] = serializedContext;
            }

            return SerializeContextsData(formatting, serializedContexts);
        }

        /// <summary>
        /// Deserialize <see cref="ContextData"/> from json asset.
        /// </summary>
        public static ContextData[] DeserializeContexts(this string data)
            => JsonConvert.DeserializeObject<ContextData[]>(data, serializerSettings);

        private static void CreateEntities(this ContextData context)
            => context.ContextType().CreateEntities(context.Entities);

        /// <summary>
        /// Serialize all entities in <paramref name="contextsData"/> to Json.
        /// </summary>
        /// <param name="formatting">Json formatting.</param>
        private static string SerializeContextsData(Formatting formatting = Formatting.None, params ContextData[] contextsData)
            => JsonConvert.SerializeObject(contextsData, formatting, serializerSettings);
    }
}
