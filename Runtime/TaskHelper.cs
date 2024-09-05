using UnityEngine;

namespace AceLand.TasksUtils
{
    internal static class TaskHelper
    {
        internal static bool TryGetMono(object obj, out MonoBehaviour mono)
        {
            mono = obj as MonoBehaviour;
            return mono != null;
        }
    }
}
