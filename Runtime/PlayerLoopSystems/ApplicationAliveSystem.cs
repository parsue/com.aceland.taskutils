using AceLand.PlayerLoopHack;
using UnityEngine;
using UnityEngine.LowLevel;

namespace AceLand.TaskUtils.PlayerLoopSystems
{
    internal class ApplicationAliveSystem : IPlayerLoopSystem
    {
        internal ApplicationAliveSystem() => SystemStart();

        private PlayerLoopSystem _system;
        
        private void SystemStart()
        {
            _system = this.CreatePlayerLoopSystem();
            _system.InsertSystem(PlayerLoopType.TimeUpdate);
        }

        internal void SystemStop()
        {
            _system.RemoveSystem(PlayerLoopType.TimeUpdate);
        }
        
        public void SystemUpdate()
        {
            if (Application.isPlaying) return;
            
            TaskHandler.OnApplicationEnd();
        }
    }
}