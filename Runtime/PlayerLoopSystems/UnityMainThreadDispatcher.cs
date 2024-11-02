using System;
using System.Collections.Generic;
using AceLand.PlayerLoopHack;
using UnityEngine;
using UnityEngine.LowLevel;

namespace AceLand.TaskUtils.PlayerLoopSystems
{
    internal class UnityMainThreadDispatcher : IPlayerLoopSystem
    {
        private static readonly Queue<Action> ExecutionQueue = new();
        
        internal UnityMainThreadDispatcher() => SystemStart();

        private PlayerLoopSystem _system;
        
        private void SystemStart()
        {
            Debug.Log("Unity MainThread Dispatcher Start");
            _system = this.CreatePlayerLoopSystem();
            _system.InsertSystem(PlayerLoopType.Initialization);
            Promise.AddApplicationQuitListener(SystemStop);
        }

        private void SystemStop()
        {
            Debug.Log("Unity MainThread Dispatcher Stop");
            _system.RemoveSystem(PlayerLoopType.Initialization);
        }
        
        public void SystemUpdate()
        {
            lock (ExecutionQueue)
            {
                while (ExecutionQueue.Count > 0)
                {
                    ExecutionQueue.Dequeue()?.Invoke();
                }
            }
        }

        internal static void Enqueue(Action action)
        {
            lock (ExecutionQueue)
            {
                ExecutionQueue.Enqueue(action);
            }
        }

        internal void Enqueue<T>(Action<T> action, T arg)
        {
            lock (ExecutionQueue)
            {
                ExecutionQueue.Enqueue(() => action(arg));
            }
        }
    }
}
