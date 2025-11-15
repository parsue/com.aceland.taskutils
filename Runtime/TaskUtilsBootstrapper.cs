using AceLand.TaskUtils.Core;
using AceLand.TaskUtils.Mono;
using UnityEngine;

namespace AceLand.TaskUtils
{
    internal static class TaskUtilsBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Initial()
        {
            PromiseHelper.Initial();
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InitialPromiseAgent()
        {
            var go = new GameObject();
            go.AddComponent<PromiseAgent>();
            go.name = "Promise Agent";
            Debug.Log("Promise Agent Initialized");
        }
    }
}
