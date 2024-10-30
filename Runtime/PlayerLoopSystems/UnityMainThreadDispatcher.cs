using System;
using System.Collections.Generic;
using AceLand.PlayerLoopHack;
using UnityEngine;
using UnityEngine.LowLevel;

namespace AceLand.TaskUtils.PlayerLoopSystems
{
    internal class UnityMainThreadDispatcher : IPlayerLoopSystem
    {
        private static readonly Queue<Action> _executionQueue = new();
        
        internal UnityMainThreadDispatcher() => SystemStart();

        private PlayerLoopSystem _system;
        
        private void SystemStart()
        {
            Debug.Log("Unity MainThread Dispatcher Start");
            _system = this.CreatePlayerLoopSystem();
            _system.InsertSystem(PlayerLoopType.EarlyUpdate, 0);
            TaskHelper.AddApplicationQuitListener(SystemStop);
        }

        private void SystemStop()
        {
            Debug.Log("Unity MainThread Dispatcher Stop");
            _system.RemoveSystem(PlayerLoopType.EarlyUpdate);
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

        internal static void Enqueue(Action action)
        {
            lock (_executionQueue)
            {
                _executionQueue.Enqueue(action);
            }
        }

        public void Enqueue<T>(Action<T> action, T arg)
        {
            lock (_executionQueue)
            {
                _executionQueue.Enqueue(() => action(arg));
            }
        }
    }
}
