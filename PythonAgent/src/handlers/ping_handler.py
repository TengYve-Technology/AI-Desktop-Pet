"""
ping_handler.py —— 心跳检测处理器

处理 type="ping" 的消息，Unity 客户端每 30 秒发送一次，
用于保持 WebSocket 连接活跃。服务端回复 "pong" 确认存活。
"""
from datetime import datetime
from handlers.base import BaseHandler
from models.message import Message

class PingHandler(BaseHandler):
    """心跳检测处理器，匹配 type="ping" 的消息"""

    def can_handle(self, msg: Message) -> bool:
        """判断消息类型是否为 ping"""
        return msg.type == "ping"

    def handle(self, msg: Message) -> dict:
        """回复 pong 状态和服务端时间，确认连接存活"""
        return msg.to_response({
            "status": "pong",
            "server_time": datetime.now().isoformat()
        })