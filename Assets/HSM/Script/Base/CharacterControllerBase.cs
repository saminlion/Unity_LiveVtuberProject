using UnityEngine;
using System.Collections.Generic;

namespace BaseController
{
    public abstract class CharacterControllerBase : MonoBehaviour, ICharacterController
    {
        protected Dictionary<string, float> pendingValues = new();

        public virtual void SetParameters(Dictionary<string, float> data)
        {
            pendingValues = new(data);
        }

        public virtual void ApplyParameters()
        {
            foreach (var entry in pendingValues)
            {
                ApplyParameter(entry.Key, entry.Value);
            }
        }

        // Live2D/VRM에서 개발 구현
        protected abstract void ApplyParameter(string key, float value);
    }
}