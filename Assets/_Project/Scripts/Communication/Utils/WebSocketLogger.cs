// Assets/_Project/Scripts/Communication/Utils/WebSocketLogger.cs

using UnityEngine;

namespace Communication.Utils
{
    public static class WebSocketLogger
    {
        private const string Prefix = "[WebSocket]";

        public static void Info(string message)
        {
            Debug.Log($"{Prefix} {message}");
        }

        public static void Warning(string message)
        {
            Debug.LogWarning($"{Prefix} {message}");
        }

        public static void Error(string message)
        {
            Debug.LogError($"{Prefix} {message}");
        }

        public static void Success(string message)
        {
            Debug.Log($"{Prefix} OK {message}");
        }
    }
}