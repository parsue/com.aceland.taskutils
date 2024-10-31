using System;
using System.Threading;
using AceLand.PlayerLoopHack;
using UnityEngine;
using UnityEngine.LowLevel;

namespace AceLand.TaskUtils.PlayerLoopSystems
{
    internal class ApplicationAliveSystem : IPlayerLoopSystem
    {
        internal ApplicationAliveSystem() => SystemStart();
        internal static CancellationTokenSource ApplicationAliveTokenSource;
        internal static event Action OnApplicationQuit;

        private PlayerLoopSystem _system;
        
        private void SystemStart()
        {
            Debug.Log("Application Alive System Start");
            ApplicationAliveTokenSource = new CancellationTokenSource();
            _system = this.CreatePlayerLoopSystem();
            _system.InsertSystem(PlayerLoopType.TimeUpdate, 0);
        }

        private void SystemStop()
        {
            Debug.Log("Application Alive System Stop");
            _system.RemoveSystem(PlayerLoopType.TimeUpdate);
        }
        
        public void SystemUpdate()
        {
            if (Application.isPlaying) return;
            
            OnApplicationEnd();
        }

        private void OnApplicationEnd()
        {
            SystemStop();
            ApplicationAliveTokenSource?.Cancel();
            ApplicationAliveTokenSource?.Dispose();
            OnApplicationQuit?.Invoke();
            Debug.Log("Application End");
        }
    }
}