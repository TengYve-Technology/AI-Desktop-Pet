# PythonAgent/src/handlers/ping_handler.py
from datetime import datetime
from handlers.base import BaseHandler
from models.message import Message

class PingHandler(BaseHandler):
    def can_handle(self, msg: Message) -> bool:
        return msg.type == "ping"

    def handle(self, msg: Message) -> dict:
        return msg.to_response({
            "status": "pong",
            "server_time": datetime.now().isoformat()
        })