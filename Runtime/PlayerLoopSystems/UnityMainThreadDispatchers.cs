using System;
using System.Collections.Generic;
using AceLand.PlayerLoopHack;
using UnityEngine;

namespace AceLand.TaskUtils.PlayerLoopSystems
{
    internal class UnityMainThreadDispatchers
    {
        internal static UnityMainThreadDispatchers Build() => new();
        private UnityMainThreadDispatchers() => StartDispatchers();
        
        private readonly Dictionary<PlayerLoopState, DispatcherSystem> _systems = new();

        private void StartDispatchers()
        {
            var states = (PlayerLoopState[])Enum.GetValues(typeof(PlayerLoopState));
            _systems.Clear();
            foreach (var state in states)
            {
                var system = DispatcherSystem.Build(state);
                _systems[state] = system;
            }
            Promise.AddApplicationQuitListener(StopDispatchers);
            Debug.Log($"Unity MainThread Dispatchers Started: {_systems.Count} systems");
        }

        private void StopDispatchers()
        {
            foreach (var system in _systems.Values)
                system.SystemStop();
            _systems.Clear();
            Debug.Log($"Unity MainThread Dispatcher Stop: {_systems.Count} systems");
        }

        internal void Enqueue(Action action, PlayerLoopState state = PlayerLoopState.Initialization)
        {
            if (action == null) return;
            var system = _systems[state];
            system.Enqueue(action);
        }
    }
}
