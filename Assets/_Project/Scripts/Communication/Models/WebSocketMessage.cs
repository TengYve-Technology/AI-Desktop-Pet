// Assets/_Project/Scripts/Communication/Models/WebSocketMessage.cs

using System;

namespace Communication.Models
{
    [Serializable]
    public class WebSocketMessage
    {
        public string id;
        public string type;
        public string protocol = "v1";
        public string timestamp;
        public object data;
        public string in_reply_to;
        public string error;
    }

    [Serializable]
    public class ChatData
    {
        public string text;
    }

    [Serializable]
    public class ResponseData
    {
        public string greeting;
        public string reply;
        public string status;
        public string server_time;
        public string client_id;
    }
}