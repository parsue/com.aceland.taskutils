using UnityEngine;

namespace AceLand.TaskUtils.Mono
{
    internal sealed class PromiseAgent : MonoBehaviour
    {
        private static PromiseAgent Instance { get; set; }
        internal bool Destroyed;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(this);
        }

        private void OnEnable()
        {
            Debug.Log("Promise Agent Enabling");
            Promise.PromiseAgentReady(this, true);
            Destroyed = false;
        }

        private void OnDisable()
        {
            Debug.Log("Promise Agent Disabling");
            Promise.PromiseAgentReady(this, false);
        }

        private void OnDestroy()
        {
            Destroyed = true;
            Debug.Log("Promise Agent Destroyed");
        }
    }
}