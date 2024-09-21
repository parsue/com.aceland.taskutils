using AceLand.TaskUtils.PlayerLoopSystems;
using UnityEngine;

namespace AceLand.TaskUtils
{
    internal static class TaskUtilsBootstrapper
    {
        private static ApplicationAliveSystem _system;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Initial()
        {
            _system = new ApplicationAliveSystem();
        }
    }
}