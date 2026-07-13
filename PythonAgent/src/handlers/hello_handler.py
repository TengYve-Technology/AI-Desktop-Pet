"""
hello_handler.py —— 连接握手处理器

处理 type="hello" 的消息，在 Unity 客户端连接成功后首先发送，
用于确认连接建立并同步服务端时间。
"""
from datetime import datetime
from handlers.base import BaseHandler
from models.message import Message

class HelloHandler(BaseHandler):
    """连接握手处理器，匹配 type="hello" 的消息"""

    def can_handle(self, msg: Message) -> bool:
        """判断消息类型是否为 hello"""
        return msg.type == "hello"

    def handle(self, msg: Message) -> dict:
        """回复问候语和服务端时间，确认连接建立"""
        return msg.to_response({
            "greeting": "Hello Back!",
            "server_time": datetime.now().isoformat()
        })