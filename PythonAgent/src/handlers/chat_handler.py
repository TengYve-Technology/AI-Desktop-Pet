# PythonAgent/src/handlers/chat_handler.py
from datetime import datetime
from handlers.base import BaseHandler
from models.message import Message

class ChatHandler(BaseHandler):
    def can_handle(self, msg: Message) -> bool:
        return msg.type == "chat"

    def handle(self, msg: Message) -> dict:
        user_text = msg.data.get("text", "") if msg.data else ""
        return msg.to_response({
            "reply": f"Received: {user_text}",
            "server_time": datetime.now().isoformat()
        })