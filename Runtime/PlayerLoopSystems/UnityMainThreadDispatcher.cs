using System;
using System.Collections.Generic;
using AceLand.PlayerLoopHack;
using UnityEngine;
using UnityEngine.LowLevel;

namespace AceLand.TaskUtils.PlayerLoopSystems
{
    internal class UnityMainThreadDispatcher : IPlayerLoopSystem
    {
        private static readonly Queue<Action> executionQueue = new();
        
        internal UnityMainThreadDispatcher() => SystemStart();

        private PlayerLoopSystem _system;
        
        private void SystemStart()
        {
            Debug.Log("Unity MainThread Dispatcher Start");
            _system = this.CreatePlayerLoopSystem();
            _system.InjectSystem(PlayerLoopState.Initialization);
            Promise.AddApplicationQuitListener(SystemStop);
        }

        private void SystemStop()
        {
            Debug.Log("Unity MainThread Dispatcher Stop");
            _system.RemoveSystem(PlayerLoopState.Initialization);
        }
        
        public void SystemUpdate()
        {
            lock (executionQueue)
            {
                while (executionQueue.Count > 0)
                    executionQueue.Dequeue()?.Invoke();
            }
        }

        internal static void Enqueue(Action action)
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
