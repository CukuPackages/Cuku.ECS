using UnityEngine;
using Entitas;
using System;

namespace Cuku.ECS
{
    public class EntityAuthoring : MonoBehaviour
    {
        public string Context;

        [SerializeReference, SubclassSelector]
        public IComponent[] Components = Array.Empty<IComponent>();

        [SerializeField, Tooltip("Destroy after Entity is created")]
        bool destroy = true;

        private void Start()
        {
            if (string.IsNullOrEmpty(Context))
            {
                Debug.LogError("Context is not set!");
                return;
            }
            Context.ToContext().CreateEntity(Components);
            if (destroy) Destroy(this.gameObject);
        }
    }
}
