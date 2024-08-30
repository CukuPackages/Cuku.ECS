using UnityEngine;
using Entitas;
using System;

namespace Cuku.ECS
{
    public class EntityAuthoring : MonoBehaviour
    {
        [SerializeField] string description;
        [SerializeField] bool destroy = true;

        [SerializeField, Space] public string Context;

        [SerializeReference, SubclassSelector]
        public IComponent[] Components = Array.Empty<IComponent>();


        private void OnEnable()
        {
            if (string.IsNullOrEmpty(Context))
            {
                Debug.LogError("Context is not set!");
                return;
            }
            Context.ToContext()?.CreateEntity(Components);
            if (destroy) Destroy(this);
        }
    }
}
