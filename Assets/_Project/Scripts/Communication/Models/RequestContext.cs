// Assets/_Project/Scripts/Communication/Models/RequestContext.cs

using System;

namespace Communication.Models
{
    public class RequestContext
    {
        public string RequestId { get; set; }
        public Action<WebSocketMessage> OnSuccess { get; set; }
        public Action<string> OnError { get; set; }
        public Action OnTimeout { get; set; }
        public DateTime SentTime { get; set; }
        public int TimeoutSeconds { get; set; } = 10;

        public bool IsExpired()
        {
            return (DateTime.Now - SentTime).TotalSeconds > TimeoutSeconds;
        }
    }
}