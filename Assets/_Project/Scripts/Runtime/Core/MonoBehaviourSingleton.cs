using UnityEngine;

namespace Vertigo.Core
{
    /// <summary>
    /// Generic scene-scoped MonoBehaviour Singleton base class.
    /// Automatically enforces a single active instance and clears the static reference when destroyed.
    /// </summary>
    public abstract class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviourSingleton<T>
    {
        public static T Instance { get; private set; }

        protected virtual void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = (T)this;
        }

        protected virtual void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
