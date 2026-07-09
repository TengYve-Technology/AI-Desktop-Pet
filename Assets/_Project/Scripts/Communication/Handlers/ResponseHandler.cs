// Assets/_Project/Scripts/Communication/Handlers/ResponseHandler.cs

using Communication.Models;
using Communication.Utils;
using System.Collections.Generic;

namespace Communication.Handlers
{
    public class ResponseHandler : MessageHandlerBase
    {
        private readonly Dictionary<string, RequestContext> _pendingRequests;

        public ResponseHandler(Dictionary<string, RequestContext> pendingRequests)
        {
            _pendingRequests = pendingRequests;
        }

        public override bool CanHandle(WebSocketMessage message)
        {
            return message.type == "response" && !string.IsNullOrEmpty(message.in_reply_to);
        }

        public override void Handle(WebSocketMessage message)
        {
            if (_pendingRequests.TryGetValue(message.in_reply_to, out var context))
            {
                context.OnSuccess?.Invoke(message);
                _pendingRequests.Remove(message.in_reply_to);
                WebSocketLogger.Success($"Request {message.in_reply_to} completed");
            }
            else
            {
                WebSocketLogger.Warning($"Orphan response: {message.in_reply_to}");
            }
        }
    }
}