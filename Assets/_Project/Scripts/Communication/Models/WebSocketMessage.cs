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
        public ChatResponseData response_data;
        public ChatData chat_data;
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

    [Serializable]
    public class ChatResponseData
    {
        public bool success;
        public string reply;
        public string error;
    }

    [Serializable]
    public class MessageDataWrapper
    {
        public ChatResponseData data;
    }
}