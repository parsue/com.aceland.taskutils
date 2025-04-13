using System;
using System.Collections.Generic;
using AceLand.PlayerLoopHack;
using UnityEngine;
using UnityEngine.LowLevel;

namespace AceLand.TaskUtils.PlayerLoopSystems
{
    internal class DispatcherSystem : IPlayerLoopSystem
    {
        internal DispatcherSystem(PlayerLoopState state) => SystemStart(state);
        
        private static readonly Queue<Action> executionQueue = new();
        private PlayerLoopState currentPlayerLoopState;
        private PlayerLoopSystem _system;
        
        private void SystemStart(PlayerLoopState state)
        {
            currentPlayerLoopState = state;
            _system = this.CreatePlayerLoopSystem();
            _system.InjectSystem(state, 0);
            Promise.AddApplicationQuitListener(SystemStop);
            Debug.Log($"Dispatcher {state} Start");
        }

        internal void SystemStop()
        {
            _system.RemoveSystem(currentPlayerLoopState);
            Debug.Log("Unity MainThread Dispatcher Stop");
        }
        
        public void SystemUpdate()
        {
            lock (executionQueue)
            {
                while (executionQueue.Count > 0)
                    executionQueue.Dequeue()?.Invoke();
            }
        }

        internal void Enqueue(Action action)
        {
            lock (executionQueue)
            {
                executionQueue.Enqueue(action);
            }
        }

        internal void Enqueue<T>(Action<T> action, T arg)
        {
            lock (executionQueue)
            {
                executionQueue.Enqueue(() => action(arg));
            }
        }
    }
}