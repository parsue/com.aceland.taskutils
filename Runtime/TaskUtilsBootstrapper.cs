using AceLand.TaskUtils.Mono;
using AceLand.TaskUtils.PlayerLoopSystems;
using UnityEngine;

namespace AceLand.TaskUtils
{
    internal static class TaskUtilsBootstrapper
    {
        private static ApplicationAliveSystem _aliveSystem;
        private static UnityMainThreadDispatchers _dispatchers;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Initial()
        {
            _aliveSystem = new ApplicationAliveSystem();
            _dispatchers = new UnityMainThreadDispatchers();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InitialPromiseAgent()
        {
            var go = new GameObject();
            go.AddComponent<PromiseDispatcher>();
            go.name = "Promise Agent";
            Debug.Log("Promise Agent Initialized");
        }
    }
}
