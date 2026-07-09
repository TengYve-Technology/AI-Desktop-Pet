// Assets/_Project/Scripts/Communication/Handlers/ErrorHandler.cs

using Communication.Models;
using Communication.Utils;
using System.Collections.Generic;

namespace Communication.Handlers
{
    public class ErrorHandler : MessageHandlerBase
    {
        private readonly Dictionary<string, RequestContext> _pendingRequests;

        public ErrorHandler(Dictionary<string, RequestContext> pendingRequests)
        {
            _pendingRequests = pendingRequests;
        }

        public override bool CanHandle(WebSocketMessage message)
        {
            return message.type == "error";
        }

        public override void Handle(WebSocketMessage message)
        {
            var errorMsg = message.error ?? "Unknown error";

            if (!string.IsNullOrEmpty(message.in_reply_to) &&
                _pendingRequests.TryGetValue(message.in_reply_to, out var context))
            {
                context.OnError?.Invoke(errorMsg);
                _pendingRequests.Remove(message.in_reply_to);
            }
            else
            {
                WebSocketLogger.Error($"Server error: {errorMsg}");
            }
        }
    }
}