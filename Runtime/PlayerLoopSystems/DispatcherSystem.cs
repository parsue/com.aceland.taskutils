using System;
using System.Collections.Generic;
using AceLand.PlayerLoopHack;
using UnityEngine;
using UnityEngine.LowLevel;

namespace AceLand.TaskUtils.PlayerLoopSystems
{
    internal class DispatcherSystem : IPlayerLoopSystem
    {
        public static DispatcherSystem Build(PlayerLoopState state) => new(state);
        private DispatcherSystem(PlayerLoopState state) => SystemStart(state);
        
        private readonly Queue<Action> _executionQueue = new();
        private PlayerLoopState _playerLoopState;
        private PlayerLoopSystem _system;
        
        private void SystemStart(PlayerLoopState state)
        {
            _playerLoopState = state;
            _system = this.CreatePlayerLoopSystem();
            _system.InjectSystem(state, 0);
            Promise.AddApplicationQuitListener(SystemStop);
        }

        internal void SystemStop()
        {
            _system.RemoveSystem(_playerLoopState);
        }
        
        public void SystemUpdate()
        {
            lock (_executionQueue)
            {
                while (_executionQueue.Count > 0)
                {
                    _executionQueue.Dequeue()?.Invoke();
                }
            }
        }

        internal void Enqueue(Action action)
        {
            lock (_executionQueue)
            {
                _executionQueue.Enqueue(action);
            }
        }

        internal void Enqueue<T>(Action<T> action, T arg)
        {
            lock (_executionQueue)
            {
                _executionQueue.Enqueue(() => action(arg));
            }
        }
    }
}