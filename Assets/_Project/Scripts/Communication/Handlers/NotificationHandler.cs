// Assets/_Project/Scripts/Communication/Handlers/NotificationHandler.cs

using Communication.Models;
using Communication.Utils;

namespace Communication.Handlers
{
    public class NotificationHandler : MessageHandlerBase
    {
        public delegate void NotificationReceived(WebSocketMessage message);
        public event NotificationReceived OnNotification;

        public override bool CanHandle(WebSocketMessage message)
        {
            // 非 response/error 且不是请求（没有 in_reply_to）视为通知
            return message.type != "response" &&
                   message.type != "error" &&
                   string.IsNullOrEmpty(message.in_reply_to);
        }

        public override void Handle(WebSocketMessage message)
        {
            WebSocketLogger.Info($"Notification received: {message.type}");
            OnNotification?.Invoke(message);
        }
    }
}