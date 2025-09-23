using AceLand.Library.Mono;
using UnityEngine;

namespace AceLand.TaskUtils.Mono
{
    internal sealed class PromiseAgent : Singleton<PromiseAgent>
    {
        internal bool Destroyed;

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