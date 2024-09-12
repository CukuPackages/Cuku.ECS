using UnityEngine;

namespace Cuku.Utilities
{
    public static class Math
    {

        // Lerp add value
        public static float Add(this float target,
                                float value,
                                float damp,
                                Vector2 clamp = default)
        {
            var newValue = target + value;
            if (clamp != default)
                newValue = Mathf.Clamp(newValue, clamp.x, clamp.y);
            return Mathf.Lerp(target, newValue, damp);
        }
    }
}
