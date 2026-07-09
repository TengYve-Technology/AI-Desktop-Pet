// Assets/_Project/Scripts/Communication/MessageHandlerBase.cs

using Communication.Models;

namespace Communication
{
    public abstract class MessageHandlerBase
    {
        public abstract bool CanHandle(WebSocketMessage message);
        public abstract void Handle(WebSocketMessage message);
    }
}