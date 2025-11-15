using AceLand.TaskUtils.Handles;
using AceLand.TaskUtils.PlayerLoopSystems;

namespace AceLand.TaskUtils.Core
{
    internal static class PromiseHelper
    {
        public static ApplicationAliveSystem AliveSystem { get; private set; }
        public static UnityMainThreadDispatchers MainThreadDispatcher { get; private set; }
        public static PromiseDispatcher PromiseDispatcher { get; private set; }

        public static void Initial()
        {
            AliveSystem = ApplicationAliveSystem.Build();
            MainThreadDispatcher = UnityMainThreadDispatchers.Build();
            PromiseDispatcher = PromiseDispatcher.Build();
        }
    }
}