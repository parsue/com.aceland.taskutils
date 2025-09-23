using System;
using System.Collections.Generic;
using AceLand.PlayerLoopHack;
using UnityEngine;

namespace AceLand.TaskUtils.PlayerLoopSystems
{
    internal class UnityMainThreadDispatchers
    {
        internal UnityMainThreadDispatchers() => StartDispatchers();
        
        private static readonly Dictionary<PlayerLoopState, DispatcherSystem> systems = new();

        private void StartDispatchers()
        {
            var states = (PlayerLoopState[])Enum.GetValues(typeof(PlayerLoopState));
            systems.Clear();
            foreach (var state in states)
            {
                var system = new DispatcherSystem(state);
                systems[state] = system;
            }
            Promise.AddApplicationQuitListener(StopDispatchers);
            Debug.Log($"Unity MainThread Dispatchers Started: {systems.Count} systems");
        }

        private void StopDispatchers()
        {
            foreach (var system in systems.Values)
                system.SystemStop();
            systems.Clear();
            Debug.Log($"Unity MainThread Dispatcher Stop: {systems.Count} systems");
        }

        internal static void Enqueue(Action action, PlayerLoopState state = PlayerLoopState.Initialization)
        {
            if (action == null) return;
            var system = systems[state];
            system.Enqueue(action);
        }
    }
}
