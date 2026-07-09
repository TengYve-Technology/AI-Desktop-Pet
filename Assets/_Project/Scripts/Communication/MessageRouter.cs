// Assets/_Project/Scripts/Communication/MessageRouter.cs

using Communication.Models;
using Communication.Utils;
using System.Collections.Generic;

namespace Communication
{
    public class MessageRouter
    {
        private readonly List<MessageHandlerBase> _handlers = new();

        public void RegisterHandler(MessageHandlerBase handler)
        {
            _handlers.Add(handler);
        }

        public void Route(WebSocketMessage message)
        {
            foreach (var handler in _handlers)
            {
                if (handler.CanHandle(message))
                {
                    handler.Handle(message);
                    return;
                }
            }

            WebSocketLogger.Warning($"No handler for message type: {message.type}");
        }
    }
}