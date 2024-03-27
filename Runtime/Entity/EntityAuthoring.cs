#if CUKU_ECS && ODIN_INSPECTOR
using Entitas;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

namespace Cuku.ECS
{
    public class EntityAuthoring : SerializedMonoBehaviour
    {
        [PropertySpace(20), OnValueChanged(nameof(ClearComponents), true)]
        [ValueDropdown(nameof(AvailableContexts), ExcludeExistingValuesInList = true, DrawDropdownForListElements = false)]
        public string Context;

        [PropertySpace(20)]
        [ShowIf(nameof(IsValidContext))]
        [ValueDropdown(nameof(AvaliableComponents), ExcludeExistingValuesInList = true, ExpandAllMenuItems = true)]
        [InfoBox("Only one Component Type per Entity is allowed!", InfoMessageType.Error,
            VisibleIf = nameof(DuplicateComponents))]
        public IComponent[] Components = new IComponent[0];

        private void Start()
        {
            Context.ToContext().CreateEntity(Components);
            GameObject.Destroy(this);
        }

        private ValueDropdownList<string> AvailableContexts()
        {
            var contexts = ContextExtentions.Contexts()
                .Select(context => context.ContextInfo.Name);
            var valueDropdownList = new ValueDropdownList<string>();
            foreach (var context in contexts)
                valueDropdownList.Add(context);
            return valueDropdownList;
        }

        private bool IsValidContext() => !string.IsNullOrEmpty(Context);

        private void ClearComponents() => Components = new IComponent[0];

        private IEnumerable<IComponent> AvaliableComponents()
        {
            if (!IsValidContext())
                return Enumerable.Empty<IComponent>();

            return (from componentType in ComponentTypes()
                    select (IComponent)Activator.CreateInstance(componentType)).ToList();
        }

        private bool DuplicateComponents()
        {
            if (!IsValidContext() || Components == null)
                return false;

            foreach (var component in Components)
            {
                // Verify that there are no duplicate components
                var componentTypeCount = 0;
                foreach (var otherComponent in Components)
                {
                    if (otherComponent.GetType() == component.GetType())
                        componentTypeCount++;
                    if (componentTypeCount > 1)
                        return true;
                }
            }
            return false;
        }

        private Type[] ComponentTypes()
            => Context.ToContext().ContextInfo.ComponentTypes;
    }
}
#endif
