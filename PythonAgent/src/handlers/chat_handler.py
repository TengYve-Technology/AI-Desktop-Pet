"""
chat_handler.py —— 聊天消息处理器

处理 type="chat" 的消息，目前仅返回确认回执。
注意：实际聊天逻辑在 server.py 中直接走 PetAgent.process_message()，
此处理器仅在 PetAgent 未拦截时作为备用。
"""
from datetime import datetime
from handlers.base import BaseHandler
from models.message import Message

class ChatHandler(BaseHandler):
    """聊天消息处理器，匹配 type="chat" 的消息"""

    def can_handle(self, msg: Message) -> bool:
        """判断消息类型是否为 chat"""
        return msg.type == "chat"

    def handle(self, msg: Message) -> dict:
        """
        处理聊天消息，返回确认回执和服务端时间

        注意：此处理器仅做简单回显，实际的 LLM 对话逻辑
        在 server.py 中由 PetAgent 处理。
        """
        user_text = msg.data.get("text", "") if msg.data else ""
        return msg.to_response({
            "reply": f"Received: {user_text}",
            "server_time": datetime.now().isoformat()
        })